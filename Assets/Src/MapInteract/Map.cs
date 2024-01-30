using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Map
{
    //��ͼ�������
    //��ȡĳ���ܱߵĸ���
    public static Vector3Int GetRoundSlotPos(Vector3Int Pos, int tar)//�����Ͽ�ʼ˳ʱ��1~6��ţ�0��������
    {
        Vector3Int endPos;
        switch (tar)
        {
            case 0:
                endPos = Pos;
                break;
            case 1:
                endPos = new Vector3Int(Pos.x - 1, Pos.y + (Pos.x + 1) % 2, Pos.z);
                break;
            case 2:
                endPos = new Vector3Int(Pos.x, Pos.y + 1, Pos.z);
                break;
            case 3:
                endPos = new Vector3Int(Pos.x + 1, Pos.y + (Pos.x + 1) % 2, Pos.z);
                break;
            case 4:
                endPos = new Vector3Int(Pos.x + 1, Pos.y - Pos.x % 2, Pos.z);
                break;
            case 5:
                endPos = new Vector3Int(Pos.x, Pos.y - 1, Pos.z);
                break;
            case 6:
                endPos = new Vector3Int(Pos.x - 1, Pos.y - Pos.x % 2, Pos.z);
                break;
            default:
                endPos = Vector3Int.zero;
                break;
        }
        return endPos;
    }
    //ˢ�¿�����
    public static void UpdateZOC()
    {
        FixGameData.FGD.ZoneMap.ClearAllTiles();

        ArmyBelong EnemySide = (ArmyBelong)(((int)GameManager.GM.ActionSide + 1) % 2);
        PiecePool EnemyPool;
        if(EnemySide == ArmyBelong.Human)
        {
            EnemyPool = FixGameData.FGD.HumanPiecePool;
        }
        else
        {
            EnemyPool = FixGameData.FGD.CrashPiecePool;;
        }

        foreach(Tuple<string,int,int> piece in EnemyPool.childList)
        {
            Vector3Int pos = new Vector3Int(piece.Item2, piece.Item3, 0);
            FixGameData.FGD.ZoneMap.SetTile(pos, FixSystemData.GlobalZoneList["ZOC"].Top);

            for(int i = 1; i < 7; i++)
            {
                FixGameData.FGD.ZoneMap.SetTile(
                    GetRoundSlotPos(pos, i), 
                    FixSystemData.GlobalZoneList["ZOC"].Top);
            }
        }

    }
}
