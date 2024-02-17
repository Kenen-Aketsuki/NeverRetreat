using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Burst.Intrinsics.X86.Avx;

public static class Map
{
    //��ͼ�������
    //��ȡĳ���ܱߵĸ���,�����Ͽ�ʼ˳ʱ��1~6��ţ�0��������
    public static Vector3Int GetRoundSlotPos(Vector3Int Pos, int direction)
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
    public static Tuple<int, Vector3Int> GetSideAddr(Vector3Int Pos, int direction)
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

        //�������� �� 0
        tmpName = FixGameData.FGD.MapList[0].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name]);
        else lst.Add(null);
        //���� �� 1
        tmpName = FixGameData.FGD.MapList[1 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //��· �� 2
        tmpName = FixGameData.FGD.MapList[4 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //������ʩ �� 3
        tmpName = FixGameData.FGD.MapList[7].GetTile(targetPos);
        if (tmpName != null && FixSystemData.GlobalFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name]);
        else if(tmpName != null && FixSystemData.GlobalSpFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalSpFacilityList[tmpName.name]);
        else lst.Add(null);
        //�����ʩ �� 4
        tmpName = FixGameData.FGD.MapList[8 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //���������� �� 5
        tmpName = FixGameData.FGD.MapList[11 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //����������� �� 6
        tmpName = FixGameData.FGD.MapList[14].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name]);
        else lst.Add(null);

        //��ȡ������ �� 7
        tmpName = FixGameData.FGD.ZOCMap.GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);
        //��ȡ������� �� 8
        tmpName = FixGameData.FGD.ZoneMap.GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);

        return lst;
    }
    
    //��ȡ�ٽ��ƶ������ģ����뵱ǰλ�úͷ���
    public static float GetNearMov(Vector3Int Pos, int Dir , ArmyBelong ActionSide)
    {
        //����ø��ез���λ���򲻿�ͨ��
        if(ActionSide == ArmyBelong.Human)
        {
            if (FixGameData.FGD.CrashPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count != 0 || //����ͨ���з����ڸ���
                FixGameData.FGD.HumanPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count >= GetHereStack(GetRoundSlotPos(Pos, Dir),ActionSide)) return -1;//���߳����ѵ�
        }
        else
        {
            if (FixGameData.FGD.HumanPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count != 0 ||
                FixGameData.FGD.CrashPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count >= GetHereStack(GetRoundSlotPos(Pos, Dir), ActionSide)) return -1;
        }

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
    //��ȡ�Ƿ��ܹ�ͨ������
    public static float GetSupplyCost(Vector3Int Pos, int Dir)
    {
        float supply = -1;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);
        for(int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //ͳ�ƹ���
            if (supply < 0 && Land.enterCount != -2) supply = Land.enterCount;
            else if(Land.name == "������")
            {
                supply = -1;
                break;
            }
        }

        return Math.Min(supply, 1);
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
            FixGameData.FGD.ZOCMap.SetTile(pos, FixSystemData.GlobalZoneList["ZOC"].Top);

            for(int i = 1; i < 7; i++)
            {
                Vector3Int tmp = GetRoundSlotPos(pos, i);
                if (FixGameData.FGD.ZoneMap.GetTile(tmp) != null || !canSetZoc(pos, i)) continue;
                FixGameData.FGD.ZOCMap.SetTile(
                    tmp, 
                    FixSystemData.GlobalZoneList["ZOC"].Top);
            }
        }

    }
    //ˢ���ƶ���
    public static void UpdateMoveArea()
    {
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        foreach (KeyValuePair<Vector3Int, CellInfo> pos in GameManager.GM.MoveArea)
        {
            if(pos.Value.moveCost == float.PositiveInfinity) continue;
            if (FixGameData.FGD.ZOCMap.HasTile(pos.Key))
            {
                FixGameData.FGD.MoveAreaMap.SetTile(pos.Key, FixGameData.FGD.MoveZocArea);
            }
            else
            {
                FixGameData.FGD.MoveAreaMap.SetTile(pos.Key, FixGameData.FGD.MoveArea);
            }
        }
    }
    //�������Ӷѵ���־
    public static void UpdatePieceStackSign()
    {
        //������Ӷѵ���־
        FixGameData.FGD.MultiPieceMap.ClearAllTiles();
        Vector3Int tmp;
        List<GameObject> lst;
        //���෽
        foreach (Tuple<string, int, int> child in FixGameData.FGD.HumanPiecePool.childList)
        {
            tmp = new Vector3Int(child.Item2, child.Item3, 0);
            lst = FixGameData.FGD.HumanPiecePool.getChildByPos(tmp);
            if (FixGameData.FGD.MultiPieceMap.GetTile(tmp) == null &&
                lst.Count > 1)
            {
                //Debug.Log(FixGameData.FGD.InteractMap.CellToWorld(tmp));
                FixGameData.FGD.MultiPieceMap.SetTile(tmp, FixGameData.FGD.MultiPieceIcon);
                OB_Piece.needChenkVisibility.Add(tmp);
            }
        }
        //������
        foreach (Tuple<string, int, int> child in FixGameData.FGD.CrashPiecePool.childList)
        {
            tmp = new Vector3Int(child.Item2, child.Item3, 0);
            lst = FixGameData.FGD.CrashPiecePool.getChildByPos(tmp);
            if (FixGameData.FGD.MultiPieceMap.GetTile(tmp) == null &&
                lst.Count > 1)
            {
                //Debug.Log(FixGameData.FGD.InteractMap.CellToWorld(tmp));
                FixGameData.FGD.MultiPieceMap.SetTile(tmp, FixGameData.FGD.MultiPieceIcon);
                OB_Piece.needChenkVisibility.Add(tmp);
            }
        }

        OB_Piece.CheckVisibility();
    }
    //��ȡ��ǰ����������
    public static void UpdateCrashBindwith()
    {
        int count = 0;

        //ͳ������
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece tmpTrans = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (tmpTrans != null)  count += tmpTrans.getPieceData().crashLoad;
        }
        //ͳ����϶
        count += FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure").ToList().Count * 10;

        GameManager.GM.CrashBandwidth = count;
    }


    //A*Ѱ·�㷨�������ж�·������
    public static List<CellInfo> AStarPathSerch(Vector3Int Start,Vector3Int End,float Distence)
    {
        Dictionary<Vector3Int, CellInfo> ToSearchStack = new Dictionary<Vector3Int, CellInfo>();//��������ջ
        Dictionary<Vector3Int, CellInfo> Searched = new Dictionary<Vector3Int, CellInfo>();//�������Ķ���
        List<CellInfo> Path = new List<CellInfo>();//·��

        //�㷨��ʼ
        CellInfo presentCell;
        ToSearchStack.Add(Start, new CellInfo(Start, Start, End, -7, 0, 0, -1));
        while(ToSearchStack.Count > 0)
        {
            //��ջ��������������
            presentCell = ToSearchStack.First().Value;
            ToSearchStack.Remove(presentCell.Positian);

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

            //δ��������������ΧһȦ������������
            for (int i = 1; i < 7; i++)
            {
                //��ʱ����
                CellInfo tmp = new CellInfo(
                    GetRoundSlotPos(presentCell.Positian, i),
                    presentCell.Positian,
                    End,
                    i,
                    GetSupplyCost(presentCell.Positian, i),
                    presentCell.usedCost,
                    presentCell.passedCell);


                //�ж��Ƿ��������������ƶ����ù⡢��ͼ�񲻿ɽ���
                if (tmp.usedCost > Distence)
                {
                    continue;
                }
                //���������յ㣬�����̽���
                if (ToSearchStack.ContainsKey(tmp.Positian) && ToSearchStack[tmp.Positian].Cost > tmp.Cost)
                {
                    ToSearchStack[tmp.Positian] = tmp;//���򵱺������ƶ����۸��ٵ�����¼���
                }
                else if(!Searched.ContainsKey(tmp.Positian))
                {
                   ToSearchStack.TryAdd(tmp.Positian, tmp);//û����ֱ�Ӽ���
                }
            }
            //������������
            ToSearchStack = ToSearchStack.OrderBy(x => x.Value.F).ThenByDescending(x => x.Value.Cost).ToDictionary(x => x.Key, x => x.Value);
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

    //DijkstraѰ·�㷨,���ڵ�λ�ƶ�
    public static Dictionary<Vector3Int,CellInfo> DijkstraPathSerch(Vector3Int Start, float CurrentMov)
    {
        //���е�·
        Dictionary<Vector3Int, CellInfo> allPath = new Dictionary<Vector3Int, CellInfo>();
        //��ѡ������
        List<Vector3Int> Searched = new List<Vector3Int>();

        //װ�س�ʼ���
        CellInfo lastSelect = new CellInfo(Start, -7, 0, 0, 0);//��һ��ѡ��ĵص�
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
                float Mov = GetNearMov(lastSelect.Positian, i, GameManager.GM.ActionSide);
                
                if (Mov == -2)
                {
                    Mov = CurrentMov - lastSelect.usedCost;//·������Ϊʣ��ȫ��
                }
                
                CellInfo tmpCell = new CellInfo(
                    tmpPos,
                    i,
                    GetPassDamge(tmpPos, i, GameManager.GM.ActionSide),
                    Mov,
                    lastSelect.usedCost
                    );

                
                //�������ɽ���ĵؿ�
                if (tmpCell.moveCost <= 0 || CurrentMov < tmpCell.usedCost) tmpCell.setMove(float.PositiveInfinity);

                //���������ֵ��
                if(allPath.ContainsKey(tmpPos) &&
                    (allPath[tmpPos] == null || allPath[tmpPos].Positian != tmpPos))
                {
                    allPath[tmpPos] = tmpCell;
                    continue;
                }

                //�滻���滻���ظ�ѡ��������½ڵ�� Cost С��ԭ�ڵ�� Cost  ��ԭ��Ϊ��ʱ����
                if (!Searched.Contains(tmpPos) &&
                    allPath.ContainsKey(tmpPos) &&
                    allPath[tmpCell.Positian].CostD > tmpCell.CostD)
                {
                    allPath[tmpCell.Positian] = tmpCell;
                }
                else
                {
                    //���벻���ڵĵ�
                    allPath.TryAdd(tmpPos, tmpCell);
                }
                
            }

            //��ȥ��ֵ
            allPath = allPath.Where(x=>x.Value != null).ToDictionary(x=>x.Key,x=>x.Value);
            //����
            allPath = allPath.OrderBy(x => Searched.Contains(x.Key) ? 1000 : x.Value.CostD).ToDictionary(x => x.Key, x => x.Value);

            //ѡ�� Cost ��С��δ����ǹ��Ľڵ�
            lastSelect = null;
            foreach (KeyValuePair<Vector3Int, CellInfo> kvp in allPath)
            {
                if (kvp.Value.moveCost == float.PositiveInfinity) continue;
                lastSelect = kvp.Value;
                break;
            }
            if (lastSelect == null || Searched.Contains(lastSelect.Positian)) break;//û�п�ѡ��ģ��˳�
            else Searched.Add(lastSelect.Positian);
            //ѭ��,ֱ�����нڵ������ֵ�����Ҳ�����ѡ����һ��
        }


        return allPath;
    }
    //Dijkstra·�������㷨
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

    //����ש�����������㷨
    public static List<CellInfo> PowerfulBrickAreaSearch(Vector3Int Start,int Range)
    {
        //������
        List<CellInfo> Area = new List<CellInfo>();
        //����������
        List<CellInfo> ToSelect = new List<CellInfo>();
        //120�ȼ������µ���ʼ�ص�
        Vector3Int PolarStart = CellTo120Dig(Start);


        ToSelect.Add(new CellInfo(Start, -7, 0, 0, 0));
        //�㷨��ʼ ����������Ϊ��ʱ����
        while(ToSelect.Count > 0)
        {
            //����
            CellInfo SelectCell = ToSelect[0];
            ToSelect.RemoveAt(0);
            //����������
            Area.Add(SelectCell);

            //����ѡ��ĵ�Ԫ������ѡ�����
            for(int i = 1; i < 7; i++)
            {
                CellInfo tmpCell = new CellInfo(
                    GetRoundSlotPos(SelectCell.Positian, i),
                    Start);
                if(tmpCell.distance > Range || !FixGameData.FGD.InteractMap.HasTile(tmpCell.Positian))
                {
                    continue;
                }else if(!Area.Contains(tmpCell) && !ToSelect.Contains(tmpCell))
                {
                    ToSelect.Add(tmpCell);
                }
            }
        }

        return Area;
    }
    //��ֱ�ǵ�120�ȼ�����ת��
    public static Vector3Int CellTo120Dig(Vector3Int pos)
    {
        int x = pos.x + (pos.y - (pos.y < 0 ? 1 : 0)) / 2 - (Math.Abs(pos.y) + 1) % 2;
        int y = pos.y;

        return new Vector3Int(x, y, pos.z);
    }
    //�����
    public static int HexDistence(Vector3Int pos1,Vector3Int pos2)
    {
        pos1 = CellTo120Dig(pos1);
        pos2 = CellTo120Dig(pos2);

        return Math.Max(Math.Max(Math.Abs(pos1.x - pos2.x), Math.Abs(pos1.y - pos2.y)), Math.Abs(pos1.y - pos2.y - pos1.x + pos2.x));
    }
}

//�洢A*�㷨�ĵ�ͼ��Ϣ
public class CellInfo
{
    public Vector3Int DebugPos => Positian * new Vector3Int(-1, 1, 0) + new Vector3Int(21, 21, fromDir);

    //λ��
    public Vector3Int Positian { get; private set; }
    //�ƶ����
    public float moveCost { get; private set; }
    //��������
    public float hpCost { get; private set; }
    //�������
    public float Cost { get { return moveCost + passedCell; } }
    //Dijkstra�õ�Cost
    public float CostD { get { return (moveCost == float.PositiveInfinity) ? float.PositiveInfinity : (hpCost * FixSystemData.AStar + usedCost); } }
    //�����ĵؿ�
    public int passedCell { get; private set; }

    //�����յ���룬ŷ�Ͼ����ƽ��
    public float distance { get; private set; }
    //��������
    public float usedCost { get; private set; }
    
    //���մ���: Cost + usedCost Ϊ��ʷ���ۣ�distance Ϊδ��Ԥ�ڴ���
    public float F { get{ return Cost + usedCost + distance; } }
    
    //���Է������ڷ���׷��
    public int fromDir { get; private set; }

    //��ǰλ��, ��һ��λ�ã��յ�, ǰ������
    public CellInfo(Vector3Int Pos,Vector3Int from, Vector3Int end, int Dir,float moveCost, float usedCost,int passedCell)
    {
        Positian = Pos;
        this.moveCost = moveCost == -1 ? float.PositiveInfinity : moveCost;
        hpCost = Map.GetPassDamge(from, Dir, GameManager.GM.ActionSide);

        Vector3Int fm = Map.CellTo120Dig(Pos);
        Vector3Int ed = Map.CellTo120Dig(end);
        distance = Math.Max(Math.Max(Math.Abs(fm.x - ed.x), Math.Abs(fm.y - ed.y)), Math.Abs(fm.y - ed.y - fm.x + ed.x));

        this.usedCost = usedCost + this.moveCost;
        fromDir = (Dir + 2) % 6 + 1;
        this.passedCell = passedCell + 1;

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
    
    public CellInfo(Vector3Int Pos,Vector3Int Start)
    {
        Positian = Pos;
        Vector3Int fm = Map.CellTo120Dig(Pos);
        Vector3Int ed = Map.CellTo120Dig(Start);
        distance = Math.Max(Math.Max(Math.Abs(fm.x - ed.x), Math.Abs(fm.y - ed.y)), Math.Abs(fm.y - ed.y - fm.x + ed.x));
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
