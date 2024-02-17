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
    //地图操作相关
    //获取某格周边的格子,从左上开始顺时针1~6编号，0代表自身
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
    //获取边元素的位置。二元组为“目标地图”与“位置”
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
    //获取相邻格子的数据,传入当前位置、方向
    //列表顺序：0 基础地形 - 1 河流 - 2 道路 - 3 格内设施 - 4 格边设施 - 5 格边特殊地形 - 6 格内特殊地形 - 7 控制区 - 8 安定结界
    public static List<LandShape> GetPLaceInfo(Vector3Int Pos,int Dir)
    {
        List<LandShape> lst = new List<LandShape>();
        Vector3Int targetPos = GetRoundSlotPos(Pos, Dir);
        Tuple<int, Vector3Int> sidePos = GetSideAddr(Pos, Dir);
        TileBase tmpName;

        //基础地形 — 0
        tmpName = FixGameData.FGD.MapList[0].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name]);
        else lst.Add(null);
        //河流 — 1
        tmpName = FixGameData.FGD.MapList[1 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //道路 — 2
        tmpName = FixGameData.FGD.MapList[4 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格内设施 — 3
        tmpName = FixGameData.FGD.MapList[7].GetTile(targetPos);
        if (tmpName != null && FixSystemData.GlobalFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name]);
        else if(tmpName != null && FixSystemData.GlobalSpFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalSpFacilityList[tmpName.name]);
        else lst.Add(null);
        //格边设施 — 4
        tmpName = FixGameData.FGD.MapList[8 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格边特殊地形 — 5
        tmpName = FixGameData.FGD.MapList[11 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格内特殊地形 — 6
        tmpName = FixGameData.FGD.MapList[14].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name]);
        else lst.Add(null);

        //获取控制区 — 7
        tmpName = FixGameData.FGD.ZOCMap.GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);
        //获取安定结界 — 8
        tmpName = FixGameData.FGD.ZoneMap.GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);

        return lst;
    }
    
    //获取临近移动力消耗，输入当前位置和方向
    public static float GetNearMov(Vector3Int Pos, int Dir , ArmyBelong ActionSide)
    {
        //如果该格有敌方单位，则不可通过
        if(ActionSide == ArmyBelong.Human)
        {
            if (FixGameData.FGD.CrashPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count != 0 || //不能通过敌方所在格子
                FixGameData.FGD.HumanPiecePool.getChildByPos(GetRoundSlotPos(Pos, Dir)).Count >= GetHereStack(GetRoundSlotPos(Pos, Dir),ActionSide)) return -1;//或者超过堆叠
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

            //统计共有
            if (Mov < 0 || Land.enterCount == -1) Mov = Land.enterCount;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ADD) MovAdd += Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ALL) Mov = -2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.NOPE) Mov = -1;
            //统计阵营
            if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_IFF(ActionSide).Item2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.ADD) MovAdd += Land.MOV_IFF(ActionSide).Item2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.ALL) Mov = -2;
            else if (Land.MOV_IFF(ActionSide) != null && Land.MOV_IFF(ActionSide).Item1 == FixWay.NOPE) Mov = -1;
        }
        if(Mov < 0 ) return Mov;
        else return Math.Max((Mov + MovAdd) , 1) * MovMULTY;
    }
    //获取是否能够通过补给
    public static float GetSupplyCost(Vector3Int Pos, int Dir)
    {
        float supply = -1;
        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);
        for(int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //统计共有
            if (supply < 0 && Land.enterCount != -2) supply = Land.enterCount;
            else if(Land.name == "控制区")
            {
                supply = -1;
                break;
            }
        }

        return Math.Min(supply, 1);
    }
    
    //检查能否延申控制区
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
    //获取目标格的堆叠
    public static int GetHereStack(Vector3Int Pos,ArmyBelong ActionSide)
    {
        float FixSTK = 0;
        List<LandShape> Lands = GetPLaceInfo(Pos, 0);
        foreach(LandShape Land in Lands)
        {
            if(Land == null) continue;
            if (Land.STK_All != null && Land.STK_All.Item1 == FixWay.ADD) FixSTK += Land.STK_All.Item2;
            //统计阵营
            if (Land.STK_IFF(ActionSide) != null && Land.STK_IFF(ActionSide).Item1 == FixWay.MULTY) FixSTK += Land.STK_IFF(ActionSide).Item2;
        }

        return (int)FixSTK + 2;
    }
    //获取目标格的高度
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
    //获取目标边的高度
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
    //获取目标方向的攻击力(带修正)
    public static float GetTargetATK(Vector3Int Pos,int Dir,ArmyBelong ActionSide, float BaseATK)
    {
        float 加 = 0;
        float 乘 = 1;

        List<LandShape> Lands = GetPLaceInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;

            //统计共有
            if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.MULTY) 乘 *= Land.ATK_All.Item2;
            else if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.ADD) 加 += Land.ATK_All.Item2;
            else if (Land.ATK_All != null && Land.ATK_All.Item1 == FixWay.NOPE) 乘 = 0;
            //统计阵营
            if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.MULTY) 乘 *= Land.ATK_IFF(ActionSide).Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.ADD) 加 += Land.ATK_IFF(ActionSide).Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.NOPE) 乘 = 0;
        }

        return (BaseATK + 加) * 乘;
    }
    //获取目标方向的防御力(带修正)
    public static float GetTargetDEFK(Vector3Int Pos, ArmyBelong ActionSide, float BaseDEF)
    {
        float 加 = 0;
        float 乘 = 1;

        List<LandShape> Lands = GetPLaceInfo(Pos, 0);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //统计共有
            if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.MULTY) 乘 *= Land.DEF_All.Item2;
            else if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.ADD) 加 += Land.DEF_All.Item2;
            else if (Land.DEF_All != null && Land.DEF_All.Item1 == FixWay.NOPE) 乘 = 0;
            //统计阵营
            if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.MULTY) 乘 *= Land.DEF_IFF(ActionSide).Item2;
            else if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.ADD) 加 += Land.DEF_IFF(ActionSide).Item2;
            else if (Land.DEF_IFF(ActionSide) != null && Land.DEF_IFF(ActionSide).Item1 == FixWay.NOPE) 乘 = 0;
        }

        return (BaseDEF + 加) * 乘;
    }
    //获取通过掉血
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
    //获取战果加成
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

    //刷新控制区
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
    //刷新移动区
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
    //布设棋子堆叠标志
    public static void UpdatePieceStackSign()
    {
        //清空棋子堆叠标志
        FixGameData.FGD.MultiPieceMap.ClearAllTiles();
        Vector3Int tmp;
        List<GameObject> lst;
        //人类方
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
        //崩坏方
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
    //获取当前崩坏方带宽
    public static void UpdateCrashBindwith()
    {
        int count = 0;

        //统计棋子
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            OB_Piece tmpTrans = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
            if (tmpTrans != null)  count += tmpTrans.getPieceData().crashLoad;
        }
        //统计裂隙
        count += FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure").ToList().Count * 10;

        GameManager.GM.CrashBandwidth = count;
    }


    //A*寻路算法，用于判定路径存在
    public static List<CellInfo> AStarPathSerch(Vector3Int Start,Vector3Int End,float Distence)
    {
        Dictionary<Vector3Int, CellInfo> ToSearchStack = new Dictionary<Vector3Int, CellInfo>();//待搜索的栈
        Dictionary<Vector3Int, CellInfo> Searched = new Dictionary<Vector3Int, CellInfo>();//已搜索的队列
        List<CellInfo> Path = new List<CellInfo>();//路径

        //算法开始
        CellInfo presentCell;
        ToSearchStack.Add(Start, new CellInfo(Start, Start, End, -7, 0, 0, -1));
        while(ToSearchStack.Count > 0)
        {
            //出栈，入已搜索队列
            presentCell = ToSearchStack.First().Value;
            ToSearchStack.Remove(presentCell.Positian);

            if(!Searched.TryAdd(presentCell.Positian, presentCell) &&
                Searched[presentCell.Positian].usedCost > presentCell.usedCost)
            {
                Searched[presentCell.Positian] = presentCell;
            }

            //判定是否结束。条件：到达终点
            if (presentCell.ArriveEnd(End))
            {
                break;
            }

            //未结束或跳过则将周围一圈加入搜索队列
            for (int i = 1; i < 7; i++)
            {
                //临时变量
                CellInfo tmp = new CellInfo(
                    GetRoundSlotPos(presentCell.Positian, i),
                    presentCell.Positian,
                    End,
                    i,
                    GetSupplyCost(presentCell.Positian, i),
                    presentCell.usedCost,
                    presentCell.passedCell);


                //判定是否跳过。条件：移动力用光、地图格不可进入
                if (tmp.usedCost > Distence)
                {
                    continue;
                }
                //若搜索到终点，则立刻结束
                if (ToSearchStack.ContainsKey(tmp.Positian) && ToSearchStack[tmp.Positian].Cost > tmp.Cost)
                {
                    ToSearchStack[tmp.Positian] = tmp;//有则当后来者移动代价更少的情况下加入
                }
                else if(!Searched.ContainsKey(tmp.Positian))
                {
                   ToSearchStack.TryAdd(tmp.Positian, tmp);//没有则直接加入
                }
            }
            //搜索队列排序
            ToSearchStack = ToSearchStack.OrderBy(x => x.Value.F).ThenByDescending(x => x.Value.Cost).ToDictionary(x => x.Key, x => x.Value);
        }

        //回溯路径
        if (Searched.ContainsKey(End))
        {
            Vector3Int tmpPos = End;
            for (; Searched[tmpPos].fromDir > 0;)
            {
                Path.Add(Searched[tmpPos]);
                tmpPos = GetRoundSlotPos(tmpPos, Searched[tmpPos].fromDir);
            }
            Path.Add(Searched[tmpPos]);
            //反向，获得路径
            Path.Reverse();
        }
        else Path = null;//返回空路径，表示寻路失败

        return Path;
    }

    //Dijkstra寻路算法,用于单位移动
    public static Dictionary<Vector3Int,CellInfo> DijkstraPathSerch(Vector3Int Start, float CurrentMov)
    {
        //所有道路
        Dictionary<Vector3Int, CellInfo> allPath = new Dictionary<Vector3Int, CellInfo>();
        //已选定队列
        List<Vector3Int> Searched = new List<Vector3Int>();

        //装载初始起点
        CellInfo lastSelect = new CellInfo(Start, -7, 0, 0, 0);//上一个选择的地点
        allPath[Start] = lastSelect;
        Searched.Add(Start);
        allPath = allPath.OrderBy(x => x.Value.Cost).ToDictionary(x => x.Key, x => x.Value);

        

        //算法开始
        while (true)
        {
            //获取所有可能的下一个选项
            for (int i = 1; i < 7; i++)
            {
                Vector3Int tmpPos = GetRoundSlotPos(lastSelect.Positian, i);
                float Mov = GetNearMov(lastSelect.Positian, i, GameManager.GM.ActionSide);
                
                if (Mov == -2)
                {
                    Mov = CurrentMov - lastSelect.usedCost;//路过消耗为剩余全部
                }
                
                CellInfo tmpCell = new CellInfo(
                    tmpPos,
                    i,
                    GetPassDamge(tmpPos, i, GameManager.GM.ActionSide),
                    Mov,
                    lastSelect.usedCost
                    );

                
                //跳过不可进入的地块
                if (tmpCell.moveCost <= 0 || CurrentMov < tmpCell.usedCost) tmpCell.setMove(float.PositiveInfinity);

                //修正错误键值对
                if(allPath.ContainsKey(tmpPos) &&
                    (allPath[tmpPos] == null || allPath[tmpPos].Positian != tmpPos))
                {
                    allPath[tmpPos] = tmpCell;
                    continue;
                }

                //替换可替换的重复选项，条件：新节点的 Cost 小于原节点的 Cost  或原本为空时加入
                if (!Searched.Contains(tmpPos) &&
                    allPath.ContainsKey(tmpPos) &&
                    allPath[tmpCell.Positian].CostD > tmpCell.CostD)
                {
                    allPath[tmpCell.Positian] = tmpCell;
                }
                else
                {
                    //加入不存在的点
                    allPath.TryAdd(tmpPos, tmpCell);
                }
                
            }

            //除去空值
            allPath = allPath.Where(x=>x.Value != null).ToDictionary(x=>x.Key,x=>x.Value);
            //排序
            allPath = allPath.OrderBy(x => Searched.Contains(x.Key) ? 1000 : x.Value.CostD).ToDictionary(x => x.Key, x => x.Value);

            //选择 Cost 最小且未被标记过的节点
            lastSelect = null;
            foreach (KeyValuePair<Vector3Int, CellInfo> kvp in allPath)
            {
                if (kvp.Value.moveCost == float.PositiveInfinity) continue;
                lastSelect = kvp.Value;
                break;
            }
            if (lastSelect == null || Searched.Contains(lastSelect.Positian)) break;//没有可选择的，退出
            else Searched.Add(lastSelect.Positian);
            //循环,直到所有节点均被赋值，或找不到可选的下一个
        }


        return allPath;
    }
    //Dijkstra路径回溯算法
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
        //反向，获得路径
        Path.Reverse();

        return Path;
    }

    //力大砖飞区域搜索算法
    public static List<CellInfo> PowerfulBrickAreaSearch(Vector3Int Start,int Range)
    {
        //已搜索
        List<CellInfo> Area = new List<CellInfo>();
        //待搜索队列
        List<CellInfo> ToSelect = new List<CellInfo>();
        //120度极坐标下的起始地点
        Vector3Int PolarStart = CellTo120Dig(Start);


        ToSelect.Add(new CellInfo(Start, -7, 0, 0, 0));
        //算法开始 待搜索队列为空时结束
        while(ToSelect.Count > 0)
        {
            //出队
            CellInfo SelectCell = ToSelect[0];
            ToSelect.RemoveAt(0);
            //加入搜索区
            Area.Add(SelectCell);

            //将可选择的单元格加入待选择队列
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
    //类直角到120度极坐标转换
    public static Vector3Int CellTo120Dig(Vector3Int pos)
    {
        int x = pos.x + (pos.y - (pos.y < 0 ? 1 : 0)) / 2 - (Math.Abs(pos.y) + 1) % 2;
        int y = pos.y;

        return new Vector3Int(x, y, pos.z);
    }
    //求距离
    public static int HexDistence(Vector3Int pos1,Vector3Int pos2)
    {
        pos1 = CellTo120Dig(pos1);
        pos2 = CellTo120Dig(pos2);

        return Math.Max(Math.Max(Math.Abs(pos1.x - pos2.x), Math.Abs(pos1.y - pos2.y)), Math.Abs(pos1.y - pos2.y - pos1.x + pos2.x));
    }
}

//存储A*算法的地图信息
public class CellInfo
{
    public Vector3Int DebugPos => Positian * new Vector3Int(-1, 1, 0) + new Vector3Int(21, 21, fromDir);

    //位置
    public Vector3Int Positian { get; private set; }
    //移动损耗
    public float moveCost { get; private set; }
    //生命消耗
    public float hpCost { get; private set; }
    //移入代价
    public float Cost { get { return moveCost + passedCell; } }
    //Dijkstra用的Cost
    public float CostD { get { return (moveCost == float.PositiveInfinity) ? float.PositiveInfinity : (hpCost * FixSystemData.AStar + usedCost); } }
    //经过的地块
    public int passedCell { get; private set; }

    //距离终点距离，欧氏距离的平方
    public float distance { get; private set; }
    //已用消耗
    public float usedCost { get; private set; }
    
    //最终代价: Cost + usedCost 为历史代价，distance 为未来预期代价
    public float F { get{ return Cost + usedCost + distance; } }
    
    //来自方向，用于反向追溯
    public int fromDir { get; private set; }

    //当前位置, 上一个位置，终点, 前进方向
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

    //是否到达终点
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
