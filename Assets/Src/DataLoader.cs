using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DataLoader : MonoBehaviour
{
    public Tilemap map;
    public TileBase tile1;
    public TileBase tile2;

    void Start()
    {
        BasicUtility.DataInit();//加载数据

        //HttpConnect.instance.InitServe(x=>Debug.Log("Serve init success"));// 初始化服务端

        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("Airpot", new Vector3Int(-1, 3), 0, 114514, false));
        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("MetroStation", new Vector3Int(-1, 3), 0, int.MaxValue, false));

        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("MetroStation", new Vector3Int(1, -5), 0, int.MaxValue, false));

        //GameUtility.mapSize = new Vector2Int(42, 42);
        //BasicUtility.saveTerrain("D:\\轩辕明月\\桌游\\毕业设计\\NeverRetreat\\Saves\\ExampelSave\\Terrain.xml");
        //BasicUtility.saveFacillitys("D:\\轩辕明月\\桌游\\毕业设计\\NeverRetreat\\Saves\\ExampelSave\\Facility.xml");
        //GameUtility.从预设中读取地图(false, "");

        //BasicUtility.savePiece("D:\\轩辕明月\\桌游\\毕业设计\\NeverRetreat\\Saves\\ExampelSave\\Piece.xml");
        //GameUtility.从预设中读取棋子(true, "ExampelSave");

        //GameUtility.fromSave = true;
        //GameUtility.Save = "ExampelSave";
        //GameUtility.游戏初始化();

        GameUtility.fromSave = false;
        GameUtility.Save = "";
        GameUtility.游戏初始化();

        //List<Tuple<string, int, int>> clist = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList;
        //Dictionary<int, Tuple<int, int>> listIndex = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex;
        //Debug.Log(clist.Count);
        //foreach (KeyValuePair<int, Tuple<int, int>> piece in listIndex)
        //{
        //    Debug.Log((piece.Key + 21) + "的长度为: " + piece.Value.Item2 + "起始坐标为: " + piece.Value.Item1);
        //}

    }

}
