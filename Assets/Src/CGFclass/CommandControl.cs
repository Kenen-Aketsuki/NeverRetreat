using System.Collections;
using System.Collections.Generic;
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

    public bool CommandTranslate(string command,OB_Piece piece, out bool canBack)
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
        bool moveResu = pic.MoveTo(end);
        if (moveResu) ToActionPiece.Enqueue(pic);
        
        return moveResu;
    }

    public bool AttackCommand(OB_Piece pic, int dir)
    {
        Vector3Int target = Map.GetRoundSlotPos(pic.piecePosition, dir);
        List<GameObject> picList = GameManager.GM.EnemyPool.getChildByPos(target);
        if(picList.Count == 0)
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
}
