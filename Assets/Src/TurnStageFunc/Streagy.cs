using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Streagy
{
    public static void CulEvent()
    {
        foreach(Tuple<SpecialEvent,Vector3Int> tuple in GameManager.GM.CrashEventList)
        {
            switch (tuple.Item1)
            {
                case SpecialEvent.DataStrom:
                    Event_DataStrom(tuple.Item2);
                    break;
                case SpecialEvent.PosConfuse:
                    Event_PosConfuse(tuple.Item2);
                    break;
                case SpecialEvent.SpaceSplit:
                    Event_SpaceTaire(tuple.Item2);
                    break;
                case SpecialEvent.SpaceFix:
                    Event_SpaceFix(tuple.Item2);
                    break;
                default:
                    Debug.Log("未知事件：" + tuple.Item1);
                    break;
            }
        }

        foreach (Tuple<SpecialEvent, Vector3Int> tuple in GameManager.GM.HumanEventList)
        {
            switch (tuple.Item1)
            {
                case SpecialEvent.MentalAD:
                    Event_MentalAD();
                    break;
                case SpecialEvent.TrainTroop:
                    Event_TrainTroop();
                    break;
                case SpecialEvent.RetreatCiv:
                    Event_RetreatCiv();
                    break;
                default:
                    Debug.Log("未知事件：" + tuple.Item1);
                    break;
            }
        }
    }

    public static void Event_DataStrom(Vector3Int Pos)
    {
        if (FixGameData.FGD.ZoneMap.GetTile(Pos) != null) return;//被安定结界，取消执行

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 5);
        for(int i = 0; i < Area.Count; i++)
        {
            FacilityDataCell tmp = new FacilityDataCell("DataDisorderZone", Area[i].Positian, 0, 1, false, false);
            //被安定结界屏蔽
            if (FixGameData.FGD.ZoneMap.GetTile(tmp.Positian) != null) continue;

            FixGameData.FGD.MapList[14].SetTile(tmp.Positian, tmp.Data.Item2.Top);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "DataDisorderZone" && x.Positian == tmp.Positian);
            if(addr == -1)
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

    public static void Event_PosConfuse(Vector3Int Pos)
    {
        if (FixGameData.FGD.ZoneMap.GetTile(Pos) != null) return;//被安定结界，取消执行

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 5);
        for (int i = 0; i < Area.Count; i++)
        {
            FacilityDataCell tmp = new FacilityDataCell("PosDisorderZone", Area[i].Positian, 0, 1, false, false);
            //被安定结界屏蔽
            if (FixGameData.FGD.ZoneMap.GetTile(tmp.Positian) != null) continue;

            FixGameData.FGD.MapList[14].SetTile(tmp.Positian, tmp.Data.Item2.Top);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "PosDisorderZone" && x.Positian == tmp.Positian);
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

    public static void Event_SpaceTaire(Vector3Int Pos)
    {
        if (FixGameData.FGD.MapList[7].GetTile(Pos) == null)
        {
            FacilityDataCell tmp = new FacilityDataCell("DimensionFissure", Pos, 0, 1, false, false);
            FixGameData.FGD.MapList[7].SetTile(tmp.Positian, tmp.Data.Item2.Close);
            FixGameData.FGD.FacilityList.Add(tmp);
        }
    }

    public static void Event_SpaceFix(Vector3Int Pos)
    {
        int addr = FixGameData.FGD.SpecialFacilityList.FindIndex(x => x.Positian == Pos);
        if (addr != -1)
        {
            FixGameData.FGD.FacilityList.RemoveAt(addr);
            FixGameData.FGD.MapList[7].SetTile(Pos, null);
        }
    }

    public static void Event_MentalAD()
    {
        List<Tuple<string, int, int>> rec = FixGameData.FGD.CrashPiecePool.childList.Where(x => !x.Item1.Contains("Crash")).ToList();

        foreach (Tuple<string, int, int> troop in rec)
        {
            if (!troop.Item1.Contains("Human")) FixGameData.FGD.CrashPiecePool.getChildByID(troop.Item1).GetComponent<OB_Piece>().Betray();
            else FixGameData.FGD.CrashPiecePool.getChildByID(troop.Item1).GetComponent<OB_Piece>().TakeDemage(100);
        }
    }

    public static void Event_TrainTroop()
    {
        List<FacilityDataCell> shelterList = FixGameData.FGD.FacilityList.Where(x => x.Id == "Shelter").ToList();
        List<FacilityDataCell> guildList = FixGameData.FGD.FacilityList.Where(x => x.Id == "HunterGuild").ToList();

        int usableShelter = 0;
        int usableGuild = guildList.Count();

        foreach(FacilityDataCell cell in shelterList)
        {
            foreach(FacilityDataCell endCell in guildList.OrderBy(x => Map.HexDistence(x.Positian, cell.Positian)))
            {
                List<CellInfo> path = Map.AStarPathSerch(cell.Positian, endCell.Positian, 20);
                if(path != null)
                {
                    usableShelter++;
                    break;
                }
            }
        }

        int finalNum = Math.Min(usableGuild, usableShelter);
        finalNum = Math.Min(finalNum, 100 - GameManager.GM.MobilizationRate);
        GameManager.GM.MobilizationRate += finalNum;
        GameManager.GM.PreTrainTroop += finalNum * 2;

    }

    public static void Event_RetreatCiv()
    {
        List<FacilityDataCell> shelterList = FixGameData.FGD.FacilityList.Where(x => x.Id == "Shelter").ToList();
        List<FacilityDataCell> guildList = FixGameData.FGD.FacilityList.Where(x => x.Id == "Airpot").ToList();

        int usableShelter = 0;
        int usableGuild = guildList.Count();

        foreach (FacilityDataCell cell in shelterList)
        {
            FixGameData.FGD.MoveAreaMap.SetTile(cell.Positian, FixGameData.FGD.MoveArea);
            foreach (FacilityDataCell endCell in guildList.OrderBy(x=>Map.HexDistence(x.Positian,cell.Positian)))
            {
                FixGameData.FGD.AttackAreaMap.SetTile(endCell.Positian, FixGameData.FGD.MoveArea);
                List<CellInfo> path = Map.AStarPathSerch(cell.Positian, endCell.Positian, 20);
                if (path != null)
                {
                    FixGameData.FGD.MoveAreaMap.SetTile(cell.Positian, FixGameData.FGD.MoveZocArea);
                    usableShelter++;
                    break;
                }
            }
        }

        int finalNum = Math.Min(usableGuild, usableShelter);
        finalNum = Math.Min(finalNum, 100 - GameManager.GM.MobilizationRate);
        GameManager.GM.MobilizationRate += finalNum;
        FixGameData.FGD.retreatCivScore += finalNum * 5;
    }


    //获取空中单位的支援签
    public static void GetAirForce()
    {
        int potCount = FixGameData.FGD.FacilityList.Where(x => x.Id == "Airpot").Count();
        Dictionary<string, Tuple<Piece, int>> tmp = new Dictionary<string, Tuple<Piece, int>>();
        foreach (KeyValuePair<string, Tuple<Piece, int>> par in FixGameData.FGD.HumanSupportDic)
        {
            tmp.Add(par.Key, new Tuple<Piece, int>(par.Value.Item1, UnityEngine.Random.Range(0, 3) * potCount + 1));
        }

        FixGameData.FGD.HumanSupportDic = tmp.ToDictionary(x=>x.Key,x=>x.Value);
    }
}
