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
        BasicUtility.DataInit();//��������

        //HttpConnect.instance.InitServe(x=>Debug.Log("Serve init success"));// ��ʼ�������

        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("Airpot", new Vector3Int(-1, 3), 0, 114514, false));
        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("MetroStation", new Vector3Int(-1, 3), 0, int.MaxValue, false));

        //FixGameData.FGD.FacilityList.Add(new FacilityDataCell("MetroStation", new Vector3Int(1, -5), 0, int.MaxValue, false));

        //GameUtility.mapSize = new Vector2Int(42, 42);
        //BasicUtility.saveTerrain("D:\\��ԯ����\\����\\��ҵ���\\NeverRetreat\\Saves\\ExampelSave\\Terrain.xml");
        //BasicUtility.saveFacillitys("D:\\��ԯ����\\����\\��ҵ���\\NeverRetreat\\Saves\\ExampelSave\\Facility.xml");
        //GameUtility.��Ԥ���ж�ȡ��ͼ(false, "");

        //BasicUtility.savePiece("D:\\��ԯ����\\����\\��ҵ���\\NeverRetreat\\Saves\\ExampelSave\\Piece.xml");
        //GameUtility.��Ԥ���ж�ȡ����(true, "ExampelSave");

        //GameUtility.fromSave = true;
        //GameUtility.Save = "ExampelSave";
        //GameUtility.��Ϸ��ʼ��();

        GameUtility.fromSave = false;
        GameUtility.Save = "";
        GameUtility.��Ϸ��ʼ��();

        //List<Tuple<string, int, int>> clist = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList;
        //Dictionary<int, Tuple<int, int>> listIndex = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex;
        //Debug.Log(clist.Count);
        //foreach (KeyValuePair<int, Tuple<int, int>> piece in listIndex)
        //{
        //    Debug.Log((piece.Key + 21) + "�ĳ���Ϊ: " + piece.Value.Item2 + "��ʼ����Ϊ: " + piece.Value.Item1);
        //}

    }

}
