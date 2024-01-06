using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        BasicUtility.DataInit();//紗墮方象
        Debug.Log("！！！！！！");
        foreach(KeyValuePair<string,BasicLandShape> land in FixSystemData.GlobalBasicTerrainList)
        {
            Debug.Log(land.Key + " ！ "+ (land.Value.Top != null).ToString());
        }
        Debug.Log("！！！！！！");
        foreach (KeyValuePair<string, Facility> land in FixSystemData.GlobalFacilityList)
        {
            Debug.Log(land.Key + " ！ " + (land.Value.Top != null).ToString());
        }
    }
}
