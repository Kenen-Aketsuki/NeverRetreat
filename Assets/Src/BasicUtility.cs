using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public static class BasicUtility
{
    public static void DataInit()
    {
        XmlDocument XmlDoc = new XmlDocument();
        //初始化固定数据
        FixSystemData.InitPath();
        //读取地形、设施等地图信息信息
        foreach(string file in Directory.GetFiles(FixSystemData.TerrainDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            XmlNodeList child = XmlDoc.DocumentElement.ChildNodes;
            foreach(XmlNode node in child)
            {
                switch (node.Attributes["Type"].Value)
                {
                    case "BasicTerrain":
                        FixSystemData.GlobalTerrainList.Add(
                            node.Attributes["id"].Value,
                            new MiddleLandShape(node)
                            );
                        break;
                    default:
                        UnityEngine.Debug.Log(node.Attributes["id"].Value);
                        break;
                }
            }
        }
        //读取棋子信息
    }
}

enum ArmyBelong
{
    Human,
    ModCrash
}

enum TerrainType
{
    BasicTerrain,
    FixFacility,
    TempFacility,
    SpecialTerrain
}

enum FixData//可提供修正的数值项
{
    ATK,//进攻方战力
    DEF,//防守方战力
    RRK,//战果评级，即裁定表的列
    MOV,//进入移动力,如一级道路的此值为-1，则计算此地进入移动力时要减去1
    STK,//堆叠
    HP//造成伤害
}
enum FixWay//修正方式
{
    ADD,//加减
    MULTY,//乘除
    NOPE,//禁止
    ALL//剩余全部
}