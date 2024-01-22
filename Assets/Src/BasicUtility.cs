using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class BasicUtility
{
    public static void DataInit()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlNodeList child;
        //初始化固定数据
        FixSystemData.InitPath();
        //读取地形、设施等地图信息信息
        foreach(string file in Directory.GetFiles(FixSystemData.TerrainDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            child = XmlDoc.DocumentElement.ChildNodes;

            foreach (XmlNode node in child)
            {
                if (node.Attributes == null) continue;

                switch (node.Attributes["Type"].Value)
                {
                    case "BasicTerrain":
                        FixSystemData.GlobalBasicTerrainList.Add(
                            node.Attributes["id"].Value,
                            new BasicLandShape(node)
                            );
                        break;
                    case "TempFacility":
                    case "FixFacility":
                        FixSystemData.GlobalFacilityList.Add(
                            node.Attributes["id"].Value,
                            new Facility(node)
                            );
                        break;
                    case "SpecialTerrain":
                        FixSystemData.GlobalSpecialTerrainList.Add(
                            node.Attributes["id"].Value,
                            new Facility(node)
                            );
                        break;
                    case "SpecialFacility":
                        FixSystemData.GlobalSpFacilityList.Add(
                            node.Attributes["id"].Value,
                            new SpecialFacility(node)
                            );
                        break;
                    case "Zone":
                        FixSystemData.GlobalZoneList.Add(
                            node.Attributes["id"].Value,
                            new Zone(node)
                            );
                        break;
                    default:
                        UnityEngine.Debug.Log(node.Attributes["id"].Value+"— SKIP");
                        break;
                }
            }
        }
        //读取棋子信息
        foreach (string file in Directory.GetFiles(FixSystemData.PieceDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            child = XmlDoc.DocumentElement.ChildNodes;

            if (XmlDoc.DocumentElement.Name != "PieceInfo") continue;
            foreach(XmlNode node in child)
            {
                if (node.Name != "Piece") continue;
                FixSystemData.GlobalPieceDataList.Add(node.Attributes["id"].Value,node);
            }
            
        }
        //读取部队编制
        foreach (string file in Directory.GetFiles(FixSystemData.PieceDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            child = XmlDoc.DocumentElement.ChildNodes;
            if (XmlDoc.DocumentElement.Name != "TroopInfo") continue;
            foreach (XmlNode node in child)
            {
                if (node.Name != "Troop") continue;
                if (node.Attributes["Belong"].Value == "Human") FixSystemData.HumanOrganizationList.Add(node.Attributes["designation"].Value, node);
                else if (node.Attributes["Belong"].Value == "ModCrash") FixSystemData.CrashOrganizationList.Add(node.Attributes["designation"].Value, node);
            }

        }
        //读取棋子美术数据(已在棋子生成里实现)
        //读取四大裁定表
        foreach (string file in Directory.GetFiles(FixSystemData.FormDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            XmlNode tmp = XmlDoc.DocumentElement.FirstChild;
            switch (tmp.Attributes["id"].Value)
            {
                case "BattleJudgeForm":
                    FixSystemData.battleJudgeForm = new BattleJudgeForm(tmp);
                    Debug.Log("加载 战斗裁定表");
                    break;
                case "AirBattleJudgeForm":
                    FixSystemData.airBattleJudgeForm = new AirBattleJudgeForm(tmp);
                    Debug.Log("加载 空战裁定表");
                    break;
                case "FireRankForm":
                    FixSystemData.fireRankForm = new FireRankForm(tmp);
                    Debug.Log("加载 火力表");
                    break;
                case "FireStrikeJudgeForm":
                    FixSystemData.fireStrikeJudgeForm = new FireStrikeJudgeForm(tmp);
                    Debug.Log("加载 火力打击裁定表");
                    break;
                default:
                    Debug.Log(file + " 处的裁定表被忽略");
                    break;
            }
        }
    }

    public static void SpawnPiece(string TroopName,Vector3 Pos)//以部队番号为名，生成一个棋子
    {
        Transform parent;
        Piece PData;
        if (FixSystemData.HumanOrganizationList.ContainsKey(TroopName))
        {
            PData = new Piece(FixSystemData.HumanOrganizationList[TroopName]);
        }
        else
        {
            PData = new Piece(FixSystemData.CrashOrganizationList[TroopName]);
        }
        
        if (PData.Belong == ArmyBelong.Human)
        {
            parent = FixGameData.FGD.HumanPieceParent;

        }
        else
        {
            parent = FixGameData.FGD.CrashPieceParent;
        }
        GameObject newPiece = Object.Instantiate(FixGameData.FGD.PiecePrefab, parent);
        newPiece.transform.position = Pos;
        newPiece.name = TroopName;
        OB_Piece ps = newPiece.GetComponent<OB_Piece>();
        ps.setPieceData(PData);
    }

    public static Sprite getPieceIcon( string name )
    {
        string path = FixSystemData.PieceDirectory + "/img";
        string[] files = Directory.GetFiles(path, name + ".png");

        Debug.Log(name);

        if (files.Length != 0)
        {
            byte[] data = File.ReadAllBytes(files[0]);

            Texture2D texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        }
        return null;
    }

    public static void saveMap(string path)
    {
        FixGameData gamedata = FixGameData.FGD;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        xmlDoc.AppendChild(xmlDec);
        XmlNode root = xmlDoc.CreateElement("MapData");
        xmlDoc.AppendChild(root);

        XmlNode tmp = xmlDoc.CreateElement("Size");
        tmp.InnerText = "42*42";
        root.AppendChild(tmp);

        XmlNode Map = xmlDoc.CreateElement("Map");
        root.AppendChild(Map);

        XmlElement Colum;
        XmlElement Row;
        string tmpStr;

        Vector3Int pos;
        for(int y = 0; y < 42; y++)
        {
            Colum = xmlDoc.CreateElement("Colum");
            Colum.SetAttribute("CNo", y.ToString());

            for (int x = 0; x < 42; x++)
            {
                Row = xmlDoc.CreateElement("Row");
                Row.SetAttribute("RNo", x.ToString());
                pos = FixGameData.MapToWorld(y, x, 42, 42);
                
                goto ed;

                //编写基础地形
                tmp = xmlDoc.CreateElement("basicTerrain");
                tmp.InnerText = gamedata.MapList[0].GetTile(pos).name;
                Row.AppendChild(tmp);
                //编写河流
                tmpStr = "";
                if (gamedata.MapList[1].GetTile(pos) != null) tmpStr += "1-"; else tmpStr += "0-";
                if (gamedata.MapList[2].GetTile(pos) != null) tmpStr += "1-"; else tmpStr += "0-";
                if (gamedata.MapList[3].GetTile(pos) != null) tmpStr += "1"; else tmpStr += "0";
                if(tmpStr != "0-0-0")
                {
                    tmp = xmlDoc.CreateElement("river");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //编写道路
                tmpStr = "";
                if (gamedata.MapList[4].GetTile(pos) != null) 
                {
                    if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road_1st")) tmpStr += "1-";
                    else if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road_2nd")) tmpStr += "2-";
                    else if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road_3rd")) tmpStr += "3-";
                }
                else tmpStr += "0-";
                if (gamedata.MapList[5].GetTile(pos) != null)
                {
                    if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road_1st")) tmpStr += "1-";
                    else if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road_2nd")) tmpStr += "2-";
                    else if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road_3rd")) tmpStr += "3-";
                }
                else tmpStr += "0-";
                if (gamedata.MapList[6].GetTile(pos) != null)
                {
                    if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road_1st")) tmpStr += "1";
                    else if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road_2nd")) tmpStr += "2";
                    else if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road_3rd")) tmpStr += "3";
                }
                else tmpStr += "0";
                if(tmpStr != "0-0-0")
                {
                    tmp = xmlDoc.CreateElement("road");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }

                
ed:
                //编写设施(格内)
                if (gamedata.MapList[7].GetTile(pos) != null)
                {
                    tmpStr = "";
                    tmpStr += gamedata.MapList[7].GetTile(pos).name;
                    tmp = xmlDoc.CreateElement("facilityC");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //编写设施(格边)
                tmpStr = "";
                if (gamedata.MapList[8].GetTile(pos) != null) tmpStr += gamedata.MapList[8].GetTile(pos).name + "-"; else tmpStr += "X-";
                if (gamedata.MapList[9].GetTile(pos) != null) tmpStr += gamedata.MapList[9].GetTile(pos).name + "-"; else tmpStr += "X-";
                if (gamedata.MapList[10].GetTile(pos) != null) tmpStr += gamedata.MapList[10].GetTile(pos).name; else tmpStr += "X";
                if(tmpStr != "X-X-X")
                {
                    tmp = xmlDoc.CreateElement("facilityS");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //特殊地形(格边)
                tmpStr = "";
                if (gamedata.MapList[11].GetTile(pos) != null) tmpStr += gamedata.MapList[11].GetTile(pos).name + "-"; else tmpStr += "X-";
                if (gamedata.MapList[12].GetTile(pos) != null) tmpStr += gamedata.MapList[12].GetTile(pos).name + "-"; else tmpStr += "X-";
                if (gamedata.MapList[13].GetTile(pos) != null) tmpStr += gamedata.MapList[13].GetTile(pos).name; else tmpStr += "X";
                if(tmpStr != "X-X-X")
                {
                    tmp = xmlDoc.CreateElement("specialTerrainS");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //特殊地形(格内)
                if (gamedata.MapList[14].GetTile(pos) != null) 
                {
                    tmpStr = "";
                    tmpStr += gamedata.MapList[14].GetTile(pos).name;
                    tmp = xmlDoc.CreateElement("specialTerrain");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }


                //列加入行
                if(Row.ChildNodes.Count != 0) Colum.AppendChild(Row);

            }
            if(Colum.ChildNodes.Count != 0) Map.AppendChild(Colum);

        }

        xmlDoc.Save(path);

    }
}

public enum ArmyBelong
{
    Human,
    ModCrash,
    Nutral
}

public enum TerrainType
{
    BasicTerrain,
    FixFacility,
    TempFacility,
    SpecialTerrain
}

public enum FixData//可提供修正的数值项
{
    ATK,//进攻方战力
    DEF,//防守方战力
    RRK,//战果评级，即裁定表的列
    MOV,//进入移动力,如一级道路的此值为-1，则计算此地进入移动力时要减去1
    STK,//堆叠
    HP//造成伤害
}
public enum FixWay//修正方式
{
    ADD,//加减
    MULTY,//乘除
    NOPE,//禁止
    ALL//剩余全部
}