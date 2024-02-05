using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public static List<LandShape> GetNearInfo(Vector3Int Pos,int Dir)
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
        List<LandShape> Lands = GetNearInfo(Pos, Dir);

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
            if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_All.Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.ADD) MovAdd += Land.MOV_All.Item2;
        }
        if(Mov < 0 ) return Mov;
        else return Math.Max((Mov + MovAdd) , 1) * MovMULTY;
    }
    //检查能否延申控制区
    static bool canSetZoc(Vector3Int Pos, int Dir)
    {
        bool canset = true;
        List<LandShape> Lands = GetNearInfo(Pos, Dir);

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

    //位置、移动力、高度、堆叠、能否跨越获得控制区、是否需要全部
    public static Tuple<Vector3Int, float, int, int, bool, bool> GetNextSlotWithData(Vector3Int Pos,int direction)//可以认为是从Pos对应位置向某个方向移动
    {
        float Mov = -1;
        int height = 0;
        int Stk = 2;
        bool canZoc = true;
        bool AllMovNeed = false;

        float MovAdd = 0;
        float MovMULTY = 1;

        string tmpStr;
        //获取边坐标
        Tuple<int, Vector3Int> tmpTup = GetSideAddr(Pos, direction);
        //获取目的坐标
        Vector3Int endPos = GetRoundSlotPos(Pos, direction);

        //计算基础地形
        if (FixGameData.FGD.MapList[0].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[0].GetTile(endPos).name;
            BasicLandShape tmpLand = FixSystemData.GlobalBasicTerrainList[tmpStr];
            Mov = tmpLand.enterCount;
            height = tmpLand.height;
            if (!tmpLand.canZoc) canZoc = false;
        }
        

        //计算河流
        if (FixGameData.FGD.MapList[1+tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            AllMovNeed = true;
            canZoc = false;
        }

        //计算道路
        if (FixGameData.FGD.MapList[4 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[4 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];
            height = tmpLand.height;
            if (Mov == -1) Mov = tmpLand.enterCount;
            else MovAdd += tmpLand.MOV_All.Item2;
        }


        //计算格内设施
        if (FixGameData.FGD.MapList[7].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[7].GetTile(endPos).name;
            Facility tmpLand = null;
            FixSystemData.GlobalFacilityList.TryGetValue(tmpStr, out tmpLand);
            if(tmpLand != null)
            {
                height = tmpLand.height;
                if (canZoc) canZoc = tmpLand.canZoc;
                if (tmpLand.STK_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) Stk += (int)tmpLand.STK_All.Item2;
                //计算公共的
                if (Mov == -1) Mov = tmpLand.enterCount;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

                //计算阵营的
                if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
            }
        }
        //特殊设施
        if (FixGameData.FGD.MapList[7].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[7].GetTile(endPos).name;
            SpecialFacility tmpLand = null;
            FixSystemData.GlobalSpFacilityList.TryGetValue(tmpStr, out tmpLand);
            if (tmpLand != null)
            {
                height = tmpLand.height;
                if (canZoc) canZoc = tmpLand.canZoc;
                if (tmpLand.STK_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) Stk += (int)tmpLand.STK_All.Item2;
                //计算公共的
                if (Mov == -1) Mov = tmpLand.enterCount;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

                //计算阵营的
                if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
            }
        }

        //计算格边设施
        if (FixGameData.FGD.MapList[8 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[8 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];
            
            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //计算公共的
            if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //计算阵营的
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }

        //计算格边特殊地形
        if (FixGameData.FGD.MapList[11 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[11 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalSpecialTerrainList[tmpStr];

            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //计算公共的
            if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //计算阵营的
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }
        //计算格内特殊地形
        if (FixGameData.FGD.MapList[14].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[14].GetTile(endPos).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];

            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //计算公共的
            if (Mov == -1) Mov = tmpLand.enterCount;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //计算阵营的
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }


        //位置、移动力、高度、堆叠、能否跨越获得控制区、是否有河流
        return new Tuple<Vector3Int, float, int, int, bool, bool>(endPos, Mov, height, Stk, canZoc, AllMovNeed);
    }

    //刷新控制区，未完善
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
}
