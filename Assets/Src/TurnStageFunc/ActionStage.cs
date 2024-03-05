using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ActionStage
{
    public static bool CommitFireStrick(Vector3Int Pos)
    {
        Action<int[], List<GameObject>> TakeAllDMG = (x, targetSet) =>
        {
            for(int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeDemage(x[i]);
            }
        };

        Action<int[], List<GameObject>> TakeAllStabel = (x, targetSet) =>
        {
            for (int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeUnstable(x[i]);
            }
        };

        if (!canHit(Pos))
        {
            Debug.Log("不中嘞哥");
            return false;
        }
        
        string fireRank = FixSystemData.fireRankForm.getData(GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2], Map.HexDistence(Pos, GameManager.GM.currentPosition));
        string[] resuSet = FixSystemData.fireStrikeJudgeForm.getResult(fireRank).Split("+");

        List<GameObject> targets = GameManager.GM.EnemyPool.getChildByPos(Pos);
        Action<int[], List<GameObject>> tarAct;

        foreach (string res in resuSet)
        {
            int dmg = int.Parse(res.Substring(0, res.Length - 1));

            int[] dmgSet = new int[targets.Count];
            for (int i = 0; i < dmg; i++)
            {
                dmgSet[UnityEngine.Random.Range(0, targets.Count)]++;
            }

            if (res.EndsWith("K")) tarAct = TakeAllDMG;
            else if (res.EndsWith("D")) tarAct = TakeAllStabel;
            else tarAct = null;

            if (tarAct != null) tarAct(dmgSet, targets);
        }
        return true;
    }

    static int getWeponVelocity()
    {
        int V = 0;
        switch (GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2])
        {
            case "Artillery":
                V = 564;
                break;
            case "SP-Artillery":
                V = 930;
                break;
            case "Machine-Grope":
                V = 1000;
                break;
        }
        return V;
    }

    public static bool canHit(Vector3Int Target)
    {
        int Velocity = getWeponVelocity();
        //int Velocity = 564;

        if (Velocity == 0) return true;

        int HexDis = Map.HexDistence(Target, GameManager.GM.currentPosition);
        //int HexDis = Map.HexDistence(Target, Vector3Int.zero);
        int Distance = HexDis * 500;//距离，单位:米
        float g = 9.8f;//重力加速度，米/秒^2


        float aTheta = Mathf.Asin(Distance * g / Mathf.Pow(Velocity, 2)) / 2;


        if (aTheta < -1 || aTheta > 1) return false;

        float Vx = Velocity * Mathf.Cos(aTheta);
        float Vh = Velocity * Mathf.Sin(aTheta);

        List<CellInfo> Line = Map.LineSerch(GameManager.GM.currentPosition, Target);
        //List<CellInfo> Line = Map.LineSerch(Vector3Int.zero, Target);

        for (int i = 1; i < Line.Count - 1; i++)
        {
            //Debug.Log("高度：" + (TanTheta * i * 500 - Mathf.Pow(i * 500, 2) * gCosTheta2V));
            float t = i * 500 / Vx;
            float h = Vh * t - 0.5f * g * Mathf.Pow(t, 2);
            Debug.Log("地块高度" + Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir) + "  高度：" + h);

            if (h < Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir))
            {
                return false;
            }
        }

        return true;

    }
}
