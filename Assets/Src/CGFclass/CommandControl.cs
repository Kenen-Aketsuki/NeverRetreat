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


    //���ж�����
    public Queue<OB_Piece> ToActionPiece = new Queue<OB_Piece>();
    //�����б��Թ����ص�Ϊ����������������Ϊ
    public Dictionary<Vector3Int,List<OB_Piece>> AttackList = new Dictionary<Vector3Int,List<OB_Piece>>();
    //�ж���������
    public List<OB_Piece> EndMovePiece = new List<OB_Piece>();

    public void CommandTranslate(string command,OB_Piece piece)
    {
        command = command.Replace("(", "").Replace(")", "");
        string[] commands = command.Split(',');

        if (piece.isSpecialPiece)
        {

        }
        else
        {
            int act;
            int dir;
            int.TryParse(commands[0], out act);
            int.TryParse(commands[1], out dir);

            dir = Mathf.Max(dir, 0);
            dir = Mathf.Min(dir, 6);

            switch (act)
            {
                case 0:
                    MoveCommand(piece, dir);
                    break;
                case 1:
                    AttackCommand(piece, dir);
                    break;
                case 2:
                    EndActionCommand(piece);
                    break;
                default:
                    Debug.Log(command + "\nָ�����");
                    break;
            }
        }
    }

    public bool MoveCommand(OB_Piece pic, int dir)
    {
        Vector3Int end = Map.GetRoundSlotPos(GameManager.GM.currentPosition, dir);
        return pic.MoveTo(end);
    }

    public void AttackCommand(OB_Piece pic, int dir)
    {

    }

    public void EndActionCommand(OB_Piece pic)
    {

    }
}
