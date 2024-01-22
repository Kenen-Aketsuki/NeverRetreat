using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;

    public GameObject PiecePrefab;
    public GameObject PieceInfoPrefab;

    //棋子本体所在
    public Transform HumanPieceParent;
    public Transform CrashPieceParent;

    //棋子信息所在
    public Transform DataHumanPieceParent;
    public Transform DataCrashPieceParent;

    //地图层次
    //基础地形-河流-道路-设施(格内)-设施(格边)-特殊地形(格边)-特殊地形(格内)
    //    0   -  1 -  4 -    7     -    8     -      11      -    14 （起始地址，按照左-中-右排序）
    public List<Tilemap> MapList;

    private void Start()
    {
        FGD = this;
    }

    public static Vector3Int MapToWorld(int x,int y,int xSize,int ySize)
    {
        x = x - (int)Math.Floor((double)(xSize / 2));
        y = y - (int)Math.Floor((double)(ySize / 2));
        return new Vector3Int(x, y, 0);
    }
}
