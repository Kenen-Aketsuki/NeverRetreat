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
    //地图操作相关
    //获取某格周边的格子,从左上开始顺时针1~6编号，0代表自身
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
    //获取边元素的位置。二元组为“目标地图”与“位置”
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
    //获取相邻格子的数据,传入当前位置、方向
    //列表顺序：0 基础地形 - 1 河流 - 2 道路 - 3 格内设施 - 4 格边设施 - 5 格边特殊地形 - 6 格内特殊地形 - 7 控制区 - 8 安定结界
    public static List<LandShape> GetPLaceInfo(Vector3Int Pos,int Dir)
    {
        List<LandShape> lst = new List<LandShape>();
        Vector3Int targetPos = GetRoundSlotPos(Pos, Dir);
        Tuple<int, Vector3Int> sidePos = GetSideAddr(Pos, Dir);
        TileBase tmpName;

        //基础地形
        tmpName = FixGameData.FGD.MapList[0].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name]);
        else lst.Add(null);
        //河流
        tmpName = FixGameData.FGD.MapList[1 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //道路
        tmpName = FixGameData.FGD.MapList[4 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格内设施
        tmpName = FixGameData.FGD.MapList[7].GetTile(targetPos);
        if (tmpName != null && FixSystemData.GlobalFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name]);
        else if(tmpName != null && FixSystemData.GlobalSpFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalSpFacilityList[tmpName.name]);
        else lst.Add(null);
        //格边设施
        tmpName = FixGameData.FGD.MapList[8 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格边特殊地形
        tmpName = FixGameData.FGD.MapList[11 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //格内特殊地形
        tmpName = FixGameData.FGD.MapList[14].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name]);
        else lst.Add(null);

        //获取控制区
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 0));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);
        //获取安定结界
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 1));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);

        return lst;
    }
    
    //获取临近移动力消耗，输入当前位置和方向
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

    //A*寻路算法
    public static List<CellInfo> AStarPathSerch(Vector3Int Start,Vector3Int End,float CurrentMov)
    {
        List<CellInfo> ToSearchStack = new List<CellInfo>();//待搜索的栈
        Dictionary<Vector3Int, CellInfo> Searched = new Dictionary<Vector3Int, CellInfo>();//已搜索的队列
        List<CellInfo> Path = new List<CellInfo>();//路径

        //算法开始
        CellInfo presentCell;
        ToSearchStack.Add(new CellInfo(Start, Start, End, -100, 0, 0, 0));
        while(ToSearchStack.Count > 0)
        {
            //出栈，入已搜索队列
            presentCell = ToSearchStack[0];
            ToSearchStack.RemoveAt(0);

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
            //判定是否跳过。条件：移动力用光、地图格不可进入
            if (presentCell.usedCost > CurrentMov || presentCell.moveCost == -2)
            {
                continue;
            }

            //未结束或跳过则将周围一圈加入搜索队列
            for (int i = 1; i < 7; i++)
            {
                //六向
                float Mov = GetNearMov(presentCell.Positian, i, GameManager.GM.ActionSide);

                //临时变量
                CellInfo tmp = new CellInfo(
                    GetRoundSlotPos(presentCell.Positian, i),
                    presentCell.Positian,
                    End,
                    i,
                    Mov,
                    presentCell.usedCost,
                    presentCell.passedCell + 1);

                
                //路过消耗为剩余全部
                if (Mov == -2) tmp.setMove(CurrentMov - tmp.usedCost);
                //跳过不可进入的地块
                if (Mov < 0) continue;

                //查看此节点是否已在待搜索队列中
                if (!ToSearchStack.Contains(tmp)) ToSearchStack.Add(tmp);//没有则直接加入
                else 
                {
                    //有则当后来者移动代价更少的情况下加入
                    int addr = ToSearchStack.FindIndex(0, ToSearchStack.Count, delegate (CellInfo inc)
                    {
                        return inc.Positian == tmp.Positian;
                    });
                    if (ToSearchStack[addr].Cost > tmp.Cost) ToSearchStack[addr] = tmp;
                }
            }
            //搜索队列排序
            ToSearchStack = ToSearchStack.OrderBy(x => x.F).ThenByDescending(x => x.Cost).ToList();
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

    //dijkstra寻路（雾）算法
    public static Dictionary<Vector3Int,CellInfo> DijkstraPathSerch(Vector3Int Start, float CurrentMov)
    {
        //所有道路
        Dictionary<Vector3Int, CellInfo> allPath = new Dictionary<Vector3Int, CellInfo>();
        //已选定队列
        List<Vector3Int> Searched = new List<Vector3Int>();

        //X、Y范围
        int xMax = (int)Math.Ceiling(Start.x + CurrentMov);
        int xMin = (int)Math.Floor(Start.x - CurrentMov);
        int yMax = (int)Math.Ceiling(Start.y + CurrentMov);
        int yMin = (int)Math.Floor(Start.y - CurrentMov);

        //初始化待搜索格子
        for(int x = xMin; x < xMax + 1; x++)
        {
            for(int y = yMin; y < yMax + 1; y++)
            {
                Vector3Int here = new Vector3Int(x, y, 0);
                if (FixGameData.FGD.InteractMap.GetTile(here) == null) continue;
                allPath.Add(here, new CellInfo(here, -7, 0, float.PositiveInfinity, 0));
            }
        }

        //装载初始起点
        CellInfo lastSelect = new CellInfo(Start, -7, 0, float.PositiveInfinity, 0);//上一个选择的地点
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

                //路过消耗为剩余全部
                if (Mov == -2) tmpCell.setMove(CurrentMov - tmpCell.usedCost);
                //跳过不可进入的地块
                if (tmpCell.moveCost < 0 || CurrentMov < tmpCell.usedCost) tmpCell.setMove(float.PositiveInfinity);

                bl1 = !Searched.Contains(tmpPos);
                bl2 = allPath.ContainsKey(tmpPos);
                bool bl3 = bl2 && allPath[tmpCell.Positian].CostD > tmpCell.CostD;

                //替换可替换的重复选项，条件：新节点的 Cost 小于原节点的 Cost  或原本为空时加入
                if (!Searched.Contains(tmpPos) &&
                    allPath.ContainsKey(tmpPos) &&
                    allPath[tmpCell.Positian].CostD > tmpCell.CostD)
                {
                    allPath[tmpCell.Positian] = tmpCell;
                }
            }

            //排序
            allPath = allPath.OrderBy(x => Searched.Contains(x.Key) ? 1000 : x.Value.CostD).ToDictionary(x => x.Key, x => x.Value);

            _ = lastSelect.Positian;

            //选择 Cost 最小且未被标记过的节点
            lastSelect = null;
            foreach (KeyValuePair<Vector3Int, CellInfo> kvp in allPath)
            {
                if (kvp.Value.moveCost == float.PositiveInfinity) continue;
                lastSelect = kvp.Value;
                break;
            }
            if (Searched.Contains(lastSelect.Positian)) break;//没有可选择的，退出
            else Searched.Add(lastSelect.Positian);
            //循环,直到所有节点均被赋值，或找不到可选的下一个
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
        //反向，获得路径
        Path.Reverse();

        return Path;
    }
}

//存储A*算法的地图信息
public class CellInfo
{
    //位置
    public Vector3Int Positian { get; private set; }
    //移动损耗
    public float moveCost { get; private set; }
    //生命消耗
    public float hpCost { get; private set; }
    //移入代价
    public float Cost { get { return moveCost + hpCost * FixSystemData.AStar + passedCell * 20; } }
    //Dijkstra用的CCost
    public float CostD { get { return moveCost == float.PositiveInfinity ? float.PositiveInfinity : (hpCost * FixSystemData.AStar + usedCost); } }

    //距离终点距离，欧氏距离的平方
    public float distance { get; private set; }
    //已用消耗
    public float usedCost { get; private set; }
   //已走过的格子数
    public int passedCell { get; private set; }
    
    //最终代价: Cost + usedCost 为历史代价，distance 为未来预期代价
    public float F { get{ return Cost + usedCost + distance; } }
    
    //来自方向，用于反向追溯
    public int fromDir { get; private set; }

    //当前位置, 上一个位置，终点, 前进方向
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

        if (Positian == new Vector3Int(-2, 1, 0)) Debug.Log(Positian + " — " + this.moveCost + " 来自:" + fromDir);

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
