using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;
    public Camera CameraNow;
    public SelfCatchScroll scrollView;

    //棋子结构
    public GameObject PiecePrefab;
    public GameObject PieceInfoPrefab;
    public GameObject UIPieceCell;

    //棋子本体所在
    public Transform HumanPieceParent;
    public Transform CrashPieceParent;

    //棋子信息所在
    public Transform DataHumanPieceParent;
    public Transform DataCrashPieceParent;

    //棋子池
    public PiecePool HumanPiecePool;
    public PiecePool CrashPiecePool;

    //地图层次
    //基础地形-河流-道路-设施(格内)-设施(格边)-特殊地形(格边)-特殊地形(格内)
    //    0   -  1 -  4 -    7     -    8     -      11      -    14 （起始地址，按照左-中-右排序）
    public List<Tilemap> MapList;
    //交互用地图
    public Tilemap InteractMap;
    //区域地图 
    public Tilemap ZoneMap;
    //堆叠标志
    public Tilemap MultiPieceMap;
    //控制区
    public Tilemap ZOCMap;
    //移动范围地图
    public Tilemap MoveAreaMap;

    //特殊瓦片
    public Tile InteractFill;//填充交互用
    public Tile MultiPieceIcon;//堆叠标记
    public Tile MoveArea;//移动区域
    public Tile MoveZocArea;//有ZOC的移动区域

    //游戏内数据
    //恒定设施列表
    public List<FacilityDataCell> SpecialFacilityList = new List<FacilityDataCell>();
    //临时设施
    public List<FacilityDataCell> FacilityList = new List<FacilityDataCell>();
    //特殊地形
    public List<FacilityDataCell> SpecialTerrainList = new List<FacilityDataCell>();
    //最大回合数
    public int MaxRoundCount;
    //回合信息
    public List<TurnData> TurnDatas;

    //人类方部队准备列表
    public List<Tuple<string, string,int>> HumanLoadList;
    //崩坏方部队准备列表
    public List<Tuple<string, string,int>> CrashLoadList;

    private void Start()
    {
        FGD = this;
    }

    public static Vector3Int MapToWorld(int x,int y)//存档坐标转游戏地图坐标
    {
        x = x - (int)Math.Floor((double)(GameUtility.mapSize.x / 2));
        y = y - (int)Math.Floor((double)(GameUtility.mapSize.y / 2));
        return new Vector3Int(x, y, 0);
    }

    public static Vector2Int WorldToMap(Vector3Int pos)//游戏地图坐标转存档坐标
    {
        int x = pos.x + (int)Math.Floor((double)(GameUtility.mapSize.x / 2));
        int y = pos.y + (int)Math.Floor((double)(GameUtility.mapSize.y / 2));
        return new Vector2Int(x, y);
    }
}

public class FacilityDataCell
{
    public Tuple<Type,LandShape> Data
    {
        get
        {
            LandShape tmp = null;
            Type type = null;
            if (FixSystemData.GlobalFacilityList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalFacilityList[Id];
                type = typeof(Facility);
            }
            else if (FixSystemData.GlobalSpFacilityList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalSpFacilityList[Id];
                type = typeof(SpecialFacility);
            }
            else if (FixSystemData.GlobalSpecialTerrainList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalSpecialTerrainList[Id];
                type = typeof(Facility);
            }
            return new Tuple<Type, LandShape>(type, tmp);
        }
    }

    public string Id { get; private set; }
    public Vector3Int Positian { get; private set; }
    public int dir { get; private set; }
    public int LastTime { get; private set; }//存在时间
    //是否激活
    public bool active { get; private set; }

    public FacilityDataCell(string id, Vector3Int Pos,int Dir,int lastTime,bool atSide,bool active)
    {
        LastTime = lastTime;
        Id = id;
        if (atSide)
        {
            Tuple<int, Vector3Int> tmpP = Map.GetSideAddr(Pos, Dir);
            Positian = tmpP.Item2;
            dir = tmpP.Item1 + 1;
        }
        else
        {
            Positian = Pos;
            dir = 0;
        }
        this.active = active;
    }

    public bool PassTime()
    {
        LastTime--;
        return LastTime > 0;
    }

    public void RemoveSelf()
    {
        int startAddr = 0;
        if(Data.Item1 == typeof(Facility) && dir == 0)
        {
            Facility tmpf = (Facility)Data.Item2;
            //本格设施
            startAddr = tmpf.isSpecialLandShape ? 14 : 7;
        }
        else if((Data.Item1 == typeof(Facility) && dir != 0))
        {
            Facility tmpf = (Facility)Data.Item2;
            //本格设施
            startAddr = tmpf.isSpecialLandShape ? 11 : 8;
            startAddr += dir - 1;
        }

        FixGameData.FGD.MapList[startAddr].SetTile(Positian, null);

    }
}