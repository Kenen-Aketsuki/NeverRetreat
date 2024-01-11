using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DataLoader : MonoBehaviour
{
    public Tilemap map;
    
    void Start()
    {
        BasicUtility.DataInit();//紗墮方象

        /*Debug.LogError("！！！！！！");
       foreach(KeyValuePair<string,XmlNode> land in FixSystemData.GlobalPieceDataList)
       {
           Debug.Log(land.Key + " ！ "+ land.Value.SelectSingleNode("name").InnerText);
       }
       Debug.LogError("！！！！！！");
       foreach (KeyValuePair<string, XmlNode> land in FixSystemData.HumanOrganizationList)
       {
           Debug.Log(land.Key + " ！ " + land.Value.Attributes["name"].Value);
       }
       Debug.LogError("！！！！！！");
       foreach (KeyValuePair<string, XmlNode> land in FixSystemData.CrashOrganizationList)
       {
           Debug.Log(land.Key + " ！ " + land.Value.Attributes["name"].Value);
       }
       */

        //霞編
        
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

    }

    private void Update()
    {
        
    }
}
