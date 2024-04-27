using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Calculate
{
    public static void EndStageCalculate()
    {
        
        //��������������ʩ�ĳ���ʱ�䣬ɾ��Ӧɾ�ĵؿ�
        for(int i = 0; i < FixGameData.FGD.SpecialTerrainList.Count; i++)
        {
            if (!FixGameData.FGD.SpecialTerrainList[i].PassTime())
            {
                FixGameData.FGD.SpecialTerrainList[i].RemoveSelf();
            }
        }
        FixGameData.FGD.SpecialTerrainList = FixGameData.FGD.SpecialTerrainList.Where(x => x.LastTime > 0).ToList();

        //���ÿ�����ӵĲ�����ͨѶ״̬
        for (int i = 0; i < FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>();
            pic.CheckGround(new CellInfo(pic.piecePosition, Vector3Int.zero));
            pic.CheckSupplyConnect();
        }

        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            pic.CheckGround(new CellInfo(pic.piecePosition, Vector3Int.zero));
            pic.CheckSupplyConnect();
        }
    }

}
