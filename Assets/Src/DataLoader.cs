using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public Transform tar;
    // Start is called before the first frame update
    void Start()
    {
        BasicUtility.DataInit();//紗墮方象
        Debug.LogError("！！！！！！");
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
    }

    private void Update()
    {
        tar.position = tar.position + new Vector3(0.001f, 0, 0);
    }
}
