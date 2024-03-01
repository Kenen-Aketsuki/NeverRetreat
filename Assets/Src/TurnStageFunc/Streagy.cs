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
                    Debug.Log("δ֪�¼���" + tuple.Item1);
                    break;
            }
        }

        foreach (Tuple<SpecialEvent, Vector3Int> tuple in GameManager.GM.HumanEventList)
        {
            switch (tuple.Item1)
            {
                default:
                    Debug.Log("δ֪�¼���" + tuple.Item1);
                    break;
            }
        }
    }

    public static void Event_DataStrom(Vector3Int Pos)
    {
        if (FixGameData.FGD.ZoneMap.GetTile(Pos) != null) return;//��������磬ȡ��ִ��

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 5);
        for(int i = 0; i < Area.Count; i++)
        {
            FacilityDataCell tmp = new FacilityDataCell("DataDisorderZone", Area[i].Positian, 0, 1, false, false);
            //�������������
            if (FixGameData.FGD.ZoneMap.GetTile(tmp.Positian) != null) continue;

            FixGameData.FGD.MapList[14].SetTile(tmp.Positian, tmp.Data.Item2.Top);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "DataDisorderZone" && x.Positian == tmp.Positian);
            if(addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }
    }

    public static void Event_PosConfuse(Vector3Int Pos)
    {
        if (FixGameData.FGD.ZoneMap.GetTile(Pos) != null) return;//��������磬ȡ��ִ��

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 5);
        for (int i = 0; i < Area.Count; i++)
        {
            FacilityDataCell tmp = new FacilityDataCell("PosDisorderZone", Area[i].Positian, 0, 1, false, false);
            //�������������
            if (FixGameData.FGD.ZoneMap.GetTile(tmp.Positian) != null) continue;

            FixGameData.FGD.MapList[14].SetTile(tmp.Positian, tmp.Data.Item2.Top);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "PosDisorderZone" && x.Positian == tmp.Positian);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
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

    //��ȡ���е�λ��֧Ԯǩ
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