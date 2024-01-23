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

        //测试
        /*
        int i = 0, j = 0;
        foreach(KeyValuePair<string,XmlNode> key in FixSystemData.HumanOrganizationList)
        {
            BasicUtility.SpawnPiece(key.Key, new Vector3Int(i, j, 10));
            j += 3;
            if (j >= 30) j = 0;
            if (j == 0) i += 3;
        }
        i = 0; j = -3;
        foreach (KeyValuePair<string, XmlNode> key in FixSystemData.CrashOrganizationList)
        {
            BasicUtility.SpawnPiece(key.Key, new Vector3Int(i, j, 10));
            j -= 3;
            if (j >= 30) j = 0;
            if (j == 0) i += 3;
        }
        */
        //map.SetTile(new Vector3Int(21, 21, 0), tile);
        //map.BoxFill(new Vector3Int(0, 0, 0), tile, -21, -21, 21, 21);
        /*
        Dictionary<string,XmlNode>.KeyCollection KeyList = FixSystemData.HumanOrganizationList.Keys;
        int i = -21,j;
        TileBase tile;
        for (; i <= 21; i++)
        {
            for (j = -21; j <= 21; j++)
            {
                (string a, XmlNode b) = FixSystemData.HumanOrganizationList.ElementAt(j + 21);
                if (j % 2 == 0 && i % 2 == 0) 
                {
                    tile = tile1;
                    BasicUtility.SpawnPiece(a, map.CellToWorld(new Vector3Int(i, j, 0)));
                }
                else tile = tile2;
                map.SetTile(new Vector3Int(i, j, 0), tile);
            }
            Debug.Log(i);
        }
        */

        GameUtility.从预设中读取地图(false,"");
        //GameUtility.mapSize = new Vector2Int(42, 42);
        //BasicUtility.saveTerrain("D:\\轩辕明月\\桌游\\毕业设计\\NeverRetreat\\GameData\\StartState\\Terrain.xml");
        //BasicUtility.saveFacillitys("D:\\轩辕明月\\桌游\\毕业设计\\NeverRetreat\\GameData\\StartState\\Facility.xml");


    }

}
