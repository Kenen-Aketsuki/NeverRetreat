using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Burst.Intrinsics.X86.Avx;

public static class Map
{
    //��ͼ�������
    //��ȡĳ���ܱߵĸ���,�����Ͽ�ʼ˳ʱ��1~6��ţ�0��������
    static Vector3Int GetRoundSlotPos(Vector3Int Pos, int direction)
    {
        Vector3Int endPos;
        int y = Math.Abs(Pos.y);
        switch (direction)
        {
            case 0:
                endPos = Pos;
                break;
            case 1:
                endPos = new Vector3Int(Pos.x + y % 2, Pos.y - 1, Pos.z);
                break;
            case 2:
                endPos = new Vector3Int(Pos.x + 1, Pos.y, Pos.z);
                break;
            case 3:
                endPos = new Vector3Int(Pos.x + y % 2, Pos.y + 1, Pos.z);
                break;
            case 4:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y + 1, Pos.z);
                break;
            case 5:
                endPos = new Vector3Int(Pos.x - 1, Pos.y, Pos.z);
                break;
            case 6:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y - 1, Pos.z);
                break;
            default:
                endPos = Vector3Int.zero;
                break;
        }
        return endPos;
    }
    //��ȡ��Ԫ�ص�λ�á���Ԫ��Ϊ��Ŀ���ͼ���롰λ�á�
    static Tuple<int, Vector3Int> GetSideAddr(Vector3Int Pos, int direction)
    {
        int map;
        int y = Math.Abs(Pos.y);
        Vector3Int endPos;
        switch (direction)
        {
            case 0:
                endPos = new Vector3Int(11, 45, 14);
                map = 0;
                break;
            case 1:
                endPos = Pos;
                map = 0;
                break;
            case 2:
                endPos = Pos;
                map = 1;
                break;
            case 3:
                endPos = Pos;
                map = 2;
                break;
            case 4:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y + 1, Pos.z);
                map = 0;
                break;
            case 5:
                endPos = new Vector3Int(Pos.x - 1, Pos.y, Pos.z);
                map = 1;
                break;
            case 6:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y - 1, Pos.z);
                map = 2;
                break;
            default:
                endPos = Vector3Int.zero;
                map = 0;
                break;
        }

        return new Tuple<int, Vector3Int>(map, endPos);
    }
    //��ȡ���ڸ��ӵ�����,���뵱ǰλ�á�����
    //�б�˳��0 �������� - 1 ���� - 2 ��· - 3 ������ʩ - 4 �����ʩ - 5 ���������� - 6 ����������� - 7 ������ - 8 �������
    public static List<LandShape> GetPLaceInfo(Vector3Int Pos,int Dir)
    {
        List<LandShape> lst = new List<LandShape>();
        Vector3Int targetPos = GetRoundSlotPos(Pos, Dir);
        Tuple<int, Vector3Int> sidePos = GetSideAddr(Pos, Dir);
        TileBase tmpName;

        //��������
        tmpName = FixGameData.FGD.MapList[0].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name]);
        else lst.Add(null);
        //����
        tmpName = FixGameData.FGD.MapList[1 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //��·
        tmpName = FixGameData.FGD.MapList[4 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //������ʩ
        tmpName = FixGameData.FGD.MapList[7].GetTile(targetPos);
        if (tmpName != null && FixSystemData.GlobalFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name]);
        else if(tmpName != null && FixSystemData.GlobalSpFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalSpFacilityList[tmpName.name]);
        else lst.Add(null);
        //�����ʩ
        tmpName = FixGameData.FGD.MapList[8 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //����������
        tmpName = FixGameData.FGD.MapList[11 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //�����������
        tmpName = FixGameData.FGD.MapList[14].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name]);
        else lst.Add(null);

        //��ȡ������
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 0));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);
        //��ȡ�������
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 1));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);

        return lst;
    }
    
    //��ȡ�ٽ��ƶ������ģ����뵱ǰλ�úͷ���
    public static float GetNearMov(Vector3Int Pos, int Dir , ArmyBelong ActionSide)
    {
        float Mov = -1;
        float MovAdd = 0;
        float MovMULTY = 1;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //ͳ�ƹ���
            if (Mov < 0 || Land.enterCount == -1) Mov = Land.enterCount;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ADD) MovAdd += Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ALL) Mov = -2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.NOPE) Mov = -1;
            //ͳ����Ӫ
            if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_IFF(ActionSide).Item2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.ADD) MovAdd += Land.MOV_IFF(ActionSide).Item2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.ALL) Mov = -2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.NOPE) Mov = -1;
        }
        if(Mov < 0 ) return Mov;
        else return Math.Max((Mov + MovAdd) , 1) * MovMULTY;
    }
    //����ܷ����������
    static bool canSetZoc(Vector3Int Pos, int Dir)
    {
        bool canset = true;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            if(!Land.canZoc)
            {
                canset = false;
                break;
            }
        }
        
        return canset;
    }
    //��ȡĿ���Ķѵ�
    public static int GetHereStack(Vector3Int Pos,ArmyBelong ActionSide)
    {
        float FixSTK = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, 0);
        foreach(LandShape Land in Lands)
        {
            if(Land == null) continue;
            if (Land.STK_All != null && Land.STK_All.Item1 == FixWay.ADD) FixSTK += Land.STK_All.Item2;
            //ͳ����Ӫ
            if (Land.STK_IFF(ActionSide) != null && Land.STK_IFF(ActionSide).Item1 == FixWay.MULTY) FixSTK += Land.STK_IFF(ActionSide).Item2;
        }

        return (int)FixSTK + 2;
    }
    //��ȡĿ���ĸ߶�
    public static int GetInCellHeight(Vector3Int Pos)
    {
        int height = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, 0);
        foreach (LandShape Land in Lands)
        {
            if (Land == null) continue;
            if (Land.height > height) height = Land.height;
        }

        return height;
    }
    //��ȡĿ��ߵĸ߶�
    public static int GetCellSideHeight(Vector3Int Pos,int Dir)
    {
        int height = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);
        foreach(LandShape Land in Lands)
        {
            if (Land == null || !Land.atSide) continue;
            if (Land.height > height) height = Land.height;
        }

        return height;
    }
    //��ȡĿ�귽��Ĺ�����(������)
    public static float GetTargetATK(Vector3Int Pos,int Dir,ArmyBelong ActionSide, float BaseATK)
    {
        float �� = 0;
        float �� = 1;

        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;

            //ͳ�ƹ���
            if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.MULTY) �� *= Land.ATK_All.Item2;
            else if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.ADD) �� += Land.ATK_All.Item2;
            else if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.NOPE) �� = 0;
            //ͳ����Ӫ
            if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.MULTY) �� *= Land.ATK_IFF(ActionSide).Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.ADD) �� += Land.ATK_IFF(ActionSide).Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.NOPE) �� = 0;
        }

        return (BaseATK + ��) * ��;
    }
    //��ȡĿ�귽��ķ�����(������)
    public static float GetTargetDEFK(Vector3Int Pos, ArmyBelong ActionSide, float BaseDEF)
    {
        float �� = 0;
        float �� = 1;

        List<LandShape> Lands = GetPLaceInfo(Pos, 0);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //ͳ�ƹ���
            if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.MULTY) �� *= Land.DEF_All.Item2;
            else if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.ADD) �� += Land.DEF_All.Item2;
            else if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.NOPE) �� = 0;
            //ͳ����Ӫ
            if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.MULTY) �� *= Land.DEF_IFF(ActionSide).Item2;
            else if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.ADD) �� += Land.DEF_IFF(ActionSide).Item2;
            else if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.NOPE) �� = 0;
        }

        return (BaseDEF + ��) * ��;
    }
    //��ȡͨ����Ѫ
    public static int GetPassDamge(Vector3Int Pos, int Dir,ArmyBelong ActionSide)
    {
        float HP = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);
        foreach (LandShape Land in Lands)
        {
            if (Land == null) continue;
            if (Land.HP_All != null) HP += Land.HP_All.Item2;
            if (Land.HP_IFF(ActionSide) != null) HP += Land.HP_IFF(ActionSide).Item2;
        }

        return (int)Math.Floor(HP);
    }
    //��ȡս���ӳ�
    public static int GetBattleRRK(Vector3Int Pos, int Dir, ArmyBelong ActionSide,int RRK)
    {
        float RRKmend = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);
        foreach (LandShape Land in Lands)
        {
            if (Land == null) continue;
            if (Land.RRK_All != null) RRKmend += Land.RRK_All.Item2;
            if (Land.RRK_IFF(ActionSide) != null) RRKmend += Land.RRK_IFF(ActionSide).Item2;
        }
        return (int)Math.Floor(RRK + RRKmend);
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
                Vector3Int tmp = GetRoundSlotPos(pos, i);
                if (FixGameData.FGD.ZoneMap.GetTile(tmp) != null || !canSetZoc(pos, i)) continue;
                FixGameData.FGD.ZoneMap.SetTile(
                    tmp, 
                    FixSystemData.GlobalZoneList["ZOC"].Top);
            }
        }

    }

    //A*Ѱ·�㷨
    public static List<CellInfo> AStarPathSerch(Vector3Int Start,Vector3Int End,float CurrentMov)
    {
        List<CellInfo> ToSearchStack = new List<CellInfo>();//��������ջ
        Dictionary<Vector3Int, CellInfo> Searched = new Dictionary<Vector3Int, CellInfo>();//�������Ķ���
        List<CellInfo> Path = new List<CellInfo>();//·��

        //�㷨��ʼ
        CellInfo presentCell;
        ToSearchStack.Add(new CellInfo(Start, Start, End, -100, 0, 0, 0));
        while(ToSearchStack.Count > 0)
        {
            //��ջ��������������
            presentCell = ToSearchStack[0];
            ToSearchStack.RemoveAt(0);

            if(!Searched.TryAdd(presentCell.Positian, presentCell) &&
                Searched[presentCell.Positian].usedCost > presentCell.usedCost)
            {
                Searched[presentCell.Positian] = presentCell;
            }

            //�ж��Ƿ�����������������յ�
            if (presentCell.ArriveEnd(End))
            {
                break;
            }
            //�ж��Ƿ��������������ƶ����ù⡢��ͼ�񲻿ɽ���
            if (presentCell.usedCost > CurrentMov || presentCell.moveCost == -2)
            {
                continue;
            }

            //δ��������������ΧһȦ������������
            for (int i = 1; i < 7; i++)
            {
                //����
                float Mov = GetNearMov(presentCell.Positian, i, GameManager.GM.ActionSide);

                //��ʱ����
                CellInfo tmp = new CellInfo(
                    GetRoundSlotPos(presentCell.Positian, i),
                    presentCell.Positian,
                    End,
                    i,
                    Mov,
                    presentCell.usedCost,
                    presentCell.passedCell + 1);

                
                //·������Ϊʣ��ȫ��
                if (Mov == -2) tmp.setMove(CurrentMov - tmp.usedCost);
                //�������ɽ���ĵؿ�
                if (Mov < 0) continue;

                //�鿴�˽ڵ��Ƿ����ڴ�����������
                if (!ToSearchStack.Contains(tmp)) ToSearchStack.Add(tmp);//û����ֱ�Ӽ���
                else 
                {
                    //���򵱺������ƶ����۸��ٵ�����¼���
                    int addr = ToSearchStack.FindIndex(0, ToSearchStack.Count, delegate (CellInfo inc)
                    {
                        return inc.Positian == tmp.Positian;
                    });
                    if (ToSearchStack[addr].Cost > tmp.Cost) ToSearchStack[addr] = tmp;
                }
            }
            //������������
            ToSearchStack = ToSearchStack.OrderBy(x => x.F).ThenByDescending(x => x.Cost).ToList();
        }

        //����·��
        if (Searched.ContainsKey(End))
        {
            Vector3Int tmpPos = End;
            for (; Searched[tmpPos].fromDir > 0;)
            {
                Path.Add(Searched[tmpPos]);
                tmpPos = GetRoundSlotPos(tmpPos, Searched[tmpPos].fromDir);
            }
            Path.Add(Searched[tmpPos]);
            //���򣬻��·��
            Path.Reverse();
        }
        else Path = null;//���ؿ�·������ʾѰ·ʧ��

        return Path;
    }

    //dijkstraѰ·�����㷨
    public static Dictionary<Vector3Int,CellInfo> DijkstraPathSerch(Vector3Int Start, float CurrentMov)
    {
        //���е�·
        Dictionary<Vector3Int, CellInfo> allPath = new Dictionary<Vector3Int, CellInfo>();
        //��ѡ������
        List<Vector3Int> Searched = new List<Vector3Int>();

        //X��Y��Χ
        int xMax = (int)Math.Ceiling(Start.x + CurrentMov);
        int xMin = (int)Math.Floor(Start.x - CurrentMov);
        int yMax = (int)Math.Ceiling(Start.y + CurrentMov);
        int yMin = (int)Math.Floor(Start.y - CurrentMov);

        //��ʼ������������
        for(int x = xMin; x < xMax + 1; x++)
        {
            for(int y = yMin; y < yMax + 1; y++)
            {
                Vector3Int here = new Vector3Int(x, y, 0);
                if (FixGameData.FGD.InteractMap.GetTile(here) == null) continue;
                allPath.Add(here, new CellInfo(here, -7, 0, float.PositiveInfinity, 0));
            }
        }

        //װ�س�ʼ���
        CellInfo lastSelect = new CellInfo(Start, -7, 0, float.PositiveInfinity, 0);//��һ��ѡ��ĵص�
        allPath[Start] = lastSelect;
        Searched.Add(Start);
        allPath = allPath.OrderBy(x => x.Value.Cost).ToDictionary(x => x.Key, x => x.Value);

        

        //�㷨��ʼ
        while (true)
        {
            //��ȡ���п��ܵ���һ��ѡ��
            for (int i = 1; i < 7; i++)
            {
                Vector3Int tmpPos = GetRoundSlotPos(lastSelect.Positian, i);
                float Mov = GetNearMov(tmpPos, i, GameManager.GM.ActionSide);

                CellInfo tmpCell = new CellInfo(
                    tmpPos,
                    i,
                    GetPassDamge(tmpPos, i, GameManager.GM.ActionSide),
                    Mov,
                    lastSelect.usedCost
                    );

                bool bl1 = tmpCell.moveCost < 0;
                bool bl2 = CurrentMov < tmpCell.usedCost;

                //·������Ϊʣ��ȫ��
                if (Mov == -2) tmpCell.setMove(CurrentMov - tmpCell.usedCost);
                //�������ɽ���ĵؿ�
                if (tmpCell.moveCost < 0 || CurrentMov < tmpCell.usedCost) tmpCell.setMove(float.PositiveInfinity);

                bl1 = !Searched.Contains(tmpPos);
                bl2 = allPath.ContainsKey(tmpPos);
                bool bl3 = bl2 && allPath[tmpCell.Positian].CostD > tmpCell.CostD;

                //�滻���滻���ظ�ѡ��������½ڵ�� Cost С��ԭ�ڵ�� Cost  ��ԭ��Ϊ��ʱ����
                if (!Searched.Contains(tmpPos) &&
                    allPath.ContainsKey(tmpPos) &&
                    allPath[tmpCell.Positian].CostD > tmpCell.CostD)
                {
                    allPath[tmpCell.Positian] = tmpCell;
                }
            }

            //����
            allPath = allPath.OrderBy(x => Searched.Contains(x.Key) ? 1000 : x.Value.CostD).ToDictionary(x => x.Key, x => x.Value);

            _ = lastSelect.Positian;

            //ѡ�� Cost ��С��δ����ǹ��Ľڵ�
            lastSelect = null;
            foreach (KeyValuePair<Vector3Int, CellInfo> kvp in allPath)
            {
                if (kvp.Value.moveCost == float.PositiveInfinity) continue;
                lastSelect = kvp.Value;
                break;
            }
            if (Searched.Contains(lastSelect.Positian)) break;//û�п�ѡ��ģ��˳�
            else Searched.Add(lastSelect.Positian);
            //ѭ��,ֱ�����нڵ������ֵ�����Ҳ�����ѡ����һ��
        }


        return allPath;
    }

    public static List<CellInfo> DijkstraPathReverse(Dictionary<Vector3Int, CellInfo> allPath, Vector3Int End)
    {
        List<CellInfo> Path = new List<CellInfo>();
        Vector3Int tmpPos = End;
        for (; allPath[tmpPos].fromDir > 0;)
        {
            Path.Add(allPath[tmpPos]);
            tmpPos = GetRoundSlotPos(tmpPos, allPath[tmpPos].fromDir);
        }
        Path.Add(allPath[tmpPos]);
        //���򣬻��·��
        Path.Reverse();

        return Path;
    }
}

//�洢A*�㷨�ĵ�ͼ��Ϣ
public class CellInfo
{
    //λ��
    public Vector3Int Positian { get; private set; }
    //�ƶ����
    public float moveCost { get; private set; }
    //��������
    public float hpCost { get; private set; }
    //�������
    public float Cost { get { return moveCost + hpCost * FixSystemData.AStar + passedCell * 20; } }
    //Dijkstra�õ�CCost
    public float CostD { get { return moveCost == float.PositiveInfinity ? float.PositiveInfinity : (hpCost * FixSystemData.AStar + usedCost); } }

    //�����յ���룬ŷ�Ͼ����ƽ��
    public float distance { get; private set; }
    //��������
    public float usedCost { get; private set; }
   //���߹��ĸ�����
    public int passedCell { get; private set; }
    
    //���մ���: Cost + usedCost Ϊ��ʷ���ۣ�distance Ϊδ��Ԥ�ڴ���
    public float F { get{ return Cost + usedCost + distance; } }
    
    //���Է������ڷ���׷��
    public int fromDir { get; private set; }

    //��ǰλ��, ��һ��λ�ã��յ�, ǰ������
    public CellInfo(Vector3Int Pos,Vector3Int from, Vector3Int end, int Dir,float moveCost, float usedCost, int passedCell)
    {
        Positian = Pos;
        this.moveCost = moveCost;
        hpCost = Map.GetPassDamge(from, Dir, GameManager.GM.ActionSide);

        Vector3 fm = FixGameData.FGD.InteractMap.CellToWorld(from);
        Vector3 ed = FixGameData.FGD.InteractMap.CellToWorld(end);

        distance = (float)(Math.Pow(fm.x - ed.x, 2) + Math.Pow(fm.y - ed.y, 2));
        this.usedCost = usedCost + moveCost;
        fromDir = (Dir + 2) % 6 + 1;
        this.passedCell = passedCell;

        if (Positian == new Vector3Int(-2, 1, 0)) Debug.Log(Positian + " �� " + this.moveCost + " ����:" + fromDir);

    }
    
    public CellInfo(Vector3Int Pos,int Dir,float Hp,float moveCost,float usedCost)
    {
        Positian = Pos;
        fromDir = (Dir + 2) % 6 + 1;
        this.moveCost = moveCost;
        if(moveCost == float.PositiveInfinity) this.usedCost = usedCost;
        else this.usedCost = usedCost + moveCost;
        hpCost = Hp;
    }
    
    public void setMove(float mov)
    {
        moveCost = mov;
    }

    //�Ƿ񵽴��յ�
    public bool ArriveEnd(Vector3Int end)
    {
        return Positian.x == end.x && Positian.y == end.y;
    }

    public override bool Equals(object obj)
    {
        return obj is CellInfo info &&
               Positian.x == info.Positian.x && Positian.y == info.Positian.y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Positian);
    }
}
