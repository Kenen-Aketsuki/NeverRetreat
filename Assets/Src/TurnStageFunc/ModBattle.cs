using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class ModBattle
{
    
    public static void CommitAttack(string nam,Vector3Int Pos)
    {
        Debug.Log(nam);

        switch (nam)
        {
            case "DataDeconstruct":
                CommitDataDeconstruct(Pos);
                break;
            case "ChaosData":
                CommitChaosData(Pos);
                break;
            case "ModEase":
                CommitModEase(Pos);
                break;
            case "DataOverLoad":
                // 当status等于"DataOverLoad"时执行的代码  
                break;
            case "AttackPatch":
                // 当status等于"AttackPatch"时执行的代码  
                break;
            case "FireWall":
                CommitFireWall(Pos);
                break;
            case "DefencePatch":
                // 当status等于"DefencePatch"时执行的代码  
                break;
            default:
                // 当status不是上述任何一个值时执行的默认代码  
                break;
        }
    }

    public static void PrepareAttack(string nam)
    {
        int range;
        switch (nam)
        {
            case "DataDeconstruct":
                range = 1;
                break;
            case "ChaosData":
                range = 1;
                break;
            case "ModEase":
                range = 3;
                break;
            case "DataOverLoad":
                range = 1;
                break;
            case "AttackPatch":
                range = 1;
                break;
            case "FireWall":
                range = 2;
                break;
            case "DefencePatch":
                range = 1;
                break;
            default:
                range = 0;
                break;
        }

        Map.SetArea(GameManager.GM.currentPiece.piecePosition, range, FixGameData.FGD.AttackAreaMap, FixGameData.FGD.MoveArea, true);
    }

    static void CommitDataDeconstruct(Vector3Int Pos)
    {

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 2);
        foreach(CellInfo Cell in Area)
        {
            if (FixGameData.FGD.MoveAreaMap.GetTile(Cell.Positian) != null) continue;
            //FixGameData.FGD.AttackAreaMap.SetTile(Cell.Positian, FixGameData.FGD.MoveArea);
            List<GameObject> tmp = GameManager.GM.EnemyPool.getChildByPos(Cell.Positian);
            foreach(GameObject pic in tmp)
            {
                pic.GetComponent<OB_Piece>().TakeUnstable(1);
            }
        }
    }

    static void CommitChaosData(Vector3Int Pos)
    {

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 2);
        int dir = Map.HexDirectionInt(GameManager.GM.currentPosition, Pos);
        int dir2 = (dir + 4) % 6 + 1;
        int dir3 = dir % 6 + 1;

        Area = Area.Where(
            x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir ||
            Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir2 ||
            Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == dir3).ToList();
        foreach (CellInfo cell in Area)
        {
            if (FixGameData.FGD.MoveAreaMap.GetTile(cell.Positian) != null) continue;
            
            List<GameObject> tmp = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
            foreach (GameObject pic in tmp)
            {
                pic.GetComponent<OB_Piece>().TakeUnstable(2);
            }
        }
    } 

    static void CommitModEase(Vector3Int Pos)
    {
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 1);
        foreach (CellInfo Cell in Area)
        {
            if (FixGameData.FGD.MoveAreaMap.GetTile(Cell.Positian) != null) continue;
            List<GameObject> tmp = GameManager.GM.EnemyPool.getChildByPos(Cell.Positian);
            foreach (GameObject pic in tmp)
            {
                pic.GetComponent<OB_Piece>().TakeUnstable(1);
            }
        }
    }

    static void CommitFireWall(Vector3Int Pos)
    {
        int dis = Map.HexDistence(Pos,GameManager.GM.currentPosition);
        int mainDir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        int subDir = (mainDir + 4) % 6 + 1;
        int mainDir2 = mainDir % 6 + 1;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, dis).Where(x => Map.HexDistence(GameManager.GM.currentPosition, x.Positian) == dis).ToList();
        //主方向，和一个主方向加一
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //副方向
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        //主方向上
        foreach (CellInfo area in Area1)
        {
            //主方向
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("Firewall", tmpT.Item2, mainDir, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmp.Positian, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "Firewall" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //主方向加一
            tmpT = Map.GetSideAddr(area.Positian, mainDir2);
            tmp = new FacilityDataCell("Firewall", tmpT.Item2, mainDir2, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmp.Positian, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "Firewall" && x.Positian == tmp.Positian && x.dir == mainDir2);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

        return;
        foreach (CellInfo area in Area2)
        {
            //主方向
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("Firewall", tmpT.Item2, mainDir, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmp.Positian, tmp.Data.Item2.GetSideTile(mainDir));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "Firewall" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //副方向
            tmpT = Map.GetSideAddr(area.Positian, subDir);
            tmp = new FacilityDataCell("Firewall", tmpT.Item2, subDir, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmp.Positian, tmp.Data.Item2.GetSideTile(subDir));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "Firewall" && x.Positian == tmp.Positian && x.dir == subDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

    }
}
