using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Calculate
{
    public static void EndStageCalculate()
    {
        
        //检查特殊地形与设施的持续时间，删除应删的地块
        for(int i = 0; i < FixGameData.FGD.SpecialTerrainList.Count; i++)
        {
            if (!FixGameData.FGD.SpecialTerrainList[i].PassTime())
            {
                FixGameData.FGD.SpecialTerrainList[i].RemoveSelf();
            }
        }
        FixGameData.FGD.SpecialTerrainList = FixGameData.FGD.SpecialTerrainList.Where(x => x.LastTime > 0).ToList();

        //检查每个棋子的补给与通讯状态
        for (int i = 0; i < FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>();
            pic.CheckSupplyConnect();
            if (pic.CheckStand()) OB_Piece.Death(pic.gameObject, pic.getPieceData());
        }

        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece pic = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            pic.CheckSupplyConnect();
            if (pic.CheckStand()) OB_Piece.Death(pic.gameObject, pic.getPieceData());
        }
    }

}
