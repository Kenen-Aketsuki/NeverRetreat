using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CommandControl
{
    static CommandControl _CC = null;
    public static CommandControl CC { get {
            if (_CC == null) _CC = new CommandControl();
            return _CC;
        }
    }


    //待行动棋子
    public Queue<OB_Piece> ToActionPiece = new Queue<OB_Piece>();
    //进攻列表，以攻击地点为键，攻击发起棋子为
    public Dictionary<Vector3Int,List<OB_Piece>> AttackList = new Dictionary<Vector3Int,List<OB_Piece>>();
    //行动结束棋子
    public List<OB_Piece> EndMovePiece = new List<OB_Piece>();
    

    public bool CommandTranslate(string command,OB_Piece piece, out bool canBack, bool canAttack = true)
    {
        command = command.Replace("(", "").Replace(")", "");
        string[] commands = command.Split(',');
        bool commandState;


        if (piece.isSpecialPiece)
        {
            commandState = true;
            canBack = false;
        }
        else
        {
            int act;
            int dir;
            int.TryParse(commands[0], out act);
            int.TryParse(commands[1], out dir);

            dir = Mathf.Max(dir, 1);
            dir = Mathf.Min(dir, 6);

            if (canAttack && act == 1) act = 0;

            Debug.Log(piece.gameObject.name + " — " + act + "-" + dir);

            switch (act)
            {
                case 0:
                    commandState = MoveCommand(piece, dir);
                    canBack = true;
                    break;
                case 1:
                    commandState = AttackCommand(piece, dir);
                    canBack = !commandState;
                    break;
                case 2:
                    EndActionCommand(piece);
                    canBack = true;
                    commandState = true;
                    break;
                default:
                    Debug.Log(command + "\n指令出错");
                    canBack = true;
                    commandState = false;
                    break;
            }
        }

        Debug.Log(commandState ? "success" : "faild");

        return commandState;
    }

    public bool MoveCommand(OB_Piece pic, int dir)
    {
        pic.PrepareMove();


        Vector3Int end = Map.GetRoundSlotPos(pic.piecePosition, dir);
        OB_Piece.needChenkVisibility.Add(pic.piecePosition);
        OB_Piece.needChenkVisibility.Add(end);

        bool moveResu = pic.MoveTo(end);
        if (moveResu) ToActionPiece.Enqueue(pic);

        OB_Piece.CheckVisibility();
        return moveResu;
    }

    public bool AttackCommand(OB_Piece pic, int dir)
    {
        Vector3Int target = Map.GetRoundSlotPos(pic.piecePosition, dir);
        List<GameObject> picList = GameManager.GM.EnemyPool.getChildByPos(target);
        if(picList.Count == 0 ||
            pic.ActionPoint <= 0)
        {
            ToActionPiece.Enqueue(pic);
            return false;
        }

        if (!AttackList.ContainsKey(target))
        {
            AttackList.Add(target, new List<OB_Piece>());
        }

        AttackList[target].Add(pic);
        return true;
    }

    public bool EndActionCommand(OB_Piece pic)
    {
        EndMovePiece.Add(pic);
        return true;
    }

    //结算进攻
    public void AttackCauclate()
    {
        Regex reg = new Regex(".*正常.*");
        foreach (KeyValuePair<Vector3Int, List<OB_Piece>> battle in AttackList)
        {
            float ATK = 0;
            float DEF = 0;
            ActionStage.CulATK(ref ATK, ref DEF, battle.Value, battle.Key);

            int RRKMend = 0;
            foreach (Vector3Int pos in Map.PowerfulBrickAreaSearch(battle.Key, FixGameData.FGD.maxFireSupportDic).Select(x => x.Positian))
            {
                int supListC = GameManager.GM.ActionPool.getChildByPos(pos)
                    .Select(x => x.GetComponent<OB_Piece>().getPieceData())
                    .Where(x => x.canSupport &&
                        Map.HexDistence(pos, battle.Key) <= x.activeArea &&
                        ActionStage.canHit(battle.Key, pos, x.PieceID, true)
                    ).Count();
                if (supListC > 0)
                {
                    RRKMend += supListC;
                    continue;
                }

                supListC = GameManager.GM.EnemyPool.getChildByPos(pos)
                    .Select(x => x.GetComponent<OB_Piece>().getPieceData())
                    .Where(x => x.canSupport &&
                        reg.IsMatch(x.ConnectStr) &&
                        reg.IsMatch(x.SupplyStr) &&
                        reg.IsMatch(x.HealthStr) &&
                        Map.HexDistence(pos, battle.Key) <= x.activeArea &&
                        ActionStage.canHit(battle.Key, pos, x.PieceID, true)
                    ).Count();
                if (supListC > 0)
                {
                    RRKMend -= supListC;
                }
            }

            ActionStage.CommitAttack(ATK, DEF, battle.Value, battle.Key, RRKMend);

            foreach(OB_Piece pic in battle.Value)
            {
                pic.ActionPoint--;
                //棋子回到待行动序列
                ToActionPiece.Enqueue(pic);
            }
        }
    }
}
