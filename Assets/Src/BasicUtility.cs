using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class BasicUtility
{
    public static void DataInit()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlNodeList child;
        //初始化固定数据fsdfasdf ad as 
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
                    break;
                case "AirBattleJudgeForm":
                    FixSystemData.airBattleJudgeForm = new AirBattleJudgeForm(tmp);
                    break;
                case "FireRankForm":
                    FixSystemData.fireRankForm = new FireRankForm(tmp);
                    break;
                case "FireStrikeJudgeForm":
                    FixSystemData.fireStrikeJudgeForm = new FireStrikeJudgeForm(tmp);
                    break;
                default:
                    Debug.Log(file + " 处的裁定表被忽略");
                    break;
            }
        }
    }

    public static string SpawnPiece(string TroopName,Vector3Int Pos,XmlNode SaveData, bool needSort)//以部队番号为名，生成一个棋子,SaveData为null说明这是新棋子,needSort代表是否按照顺序加入
    {
        Transform parent;
        Piece PData;
        PiecePool pool;

        if (FixSystemData.HumanOrganizationList.ContainsKey(TroopName))
        {
            PData = new Piece(FixSystemData.HumanOrganizationList[TroopName],SaveData);
        }
        else
        {
            PData = new Piece(FixSystemData.CrashOrganizationList[TroopName],SaveData);
        }
        
        if (PData.Belong == ArmyBelong.Human)
        {
            parent = FixGameData.FGD.HumanPieceParent;

        }
        else
        {
            parent = FixGameData.FGD.CrashPieceParent;
        }

        GameObject newPiece = UnityEngine.Object.Instantiate(FixGameData.FGD.PiecePrefab, parent);
        newPiece.transform.position = FixGameData.FGD.InteractMap.CellToWorld(Pos);
        newPiece.name = parent.gameObject.GetComponent<PiecePool>().getRedomNo() + TroopName;
        OB_Piece ps = newPiece.GetComponent<OB_Piece>();
        ps.setPieceData(PData);

        pool = parent.gameObject.GetComponent<PiecePool>();
        if (needSort)
        {
            pool.AddChildInOrder(newPiece.name, Pos);
        }
        else
        {
            pool.AddChildNoOrder(newPiece.name, Pos);
        }

        return newPiece.name;
    }

    public static Sprite getPieceIcon( string name )
    {
        string path = FixSystemData.PieceDirectory + "/img";
        string[] files = Directory.GetFiles(path, name + ".png");

        if (files.Length != 0)
        {
            byte[] data = File.ReadAllBytes(files[0]);

            Texture2D texture = new Texture2D(FixSystemData.ImagSize, FixSystemData.ImagSize, TextureFormat.ARGB32, false);
            texture.LoadImage(data);
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), FixSystemData.ImagSize);
        }
        return null;
    }

    public static void saveTerrain(string path)//保存道路、基本地形、河流
    {
        FixGameData gamedata = FixGameData.FGD;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        xmlDoc.AppendChild(xmlDec);
        XmlNode root = xmlDoc.CreateElement("MapData");
        xmlDoc.AppendChild(root);

        XmlNode tmp = xmlDoc.CreateElement("Size");
        tmp.InnerText = GameUtility.mapSize.x + "*" + GameUtility.mapSize.y;
        root.AppendChild(tmp);

        XmlNode Map = xmlDoc.CreateElement("Map");
        root.AppendChild(Map);

        XmlElement Colum;
        XmlElement Row;
        string tmpStr;

        Vector3Int pos;
        for (int y = 0; y <= GameUtility.mapSize.y; y++)
        {
            Colum = xmlDoc.CreateElement("Colum");
            Colum.SetAttribute("CNo", y.ToString());

            for (int x = 0; x <= GameUtility.mapSize.x; x++)
            {
                Row = xmlDoc.CreateElement("Row");
                Row.SetAttribute("RNo", x.ToString());
                pos = FixGameData.MapToWorld(y, x);

                //编写基础地形
                tmp = xmlDoc.CreateElement("basicTerrain");
                tmp.InnerText = gamedata.MapList[0].GetTile(pos).name;
                Row.AppendChild(tmp);
                //编写河流
                tmpStr = "";
                if (gamedata.MapList[1].GetTile(pos) != null) tmpStr += "1-"; else tmpStr += "0-";
                if (gamedata.MapList[2].GetTile(pos) != null) tmpStr += "1-"; else tmpStr += "0-";
                if (gamedata.MapList[3].GetTile(pos) != null) tmpStr += "1"; else tmpStr += "0";
                if (tmpStr != "0-0-0")
                {
                    tmp = xmlDoc.CreateElement("river");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //编写道路
                tmpStr = "";
                if (gamedata.MapList[4].GetTile(pos) != null)
                {
                    if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road1st")) tmpStr += "1-";
                    else if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road2nd")) tmpStr += "2-";
                    else if (gamedata.MapList[4].GetTile(pos).name.StartsWith("Road3rd")) tmpStr += "3-";
                }
                else tmpStr += "0-";
                if (gamedata.MapList[5].GetTile(pos) != null)
                {
                    if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road1st")) tmpStr += "1-";
                    else if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road2nd")) tmpStr += "2-";
                    else if (gamedata.MapList[5].GetTile(pos).name.StartsWith("Road3rd")) tmpStr += "3-";
                }
                else tmpStr += "0-";
                if (gamedata.MapList[6].GetTile(pos) != null)
                {
                    if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road1st")) tmpStr += "1";
                    else if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road2nd")) tmpStr += "2";
                    else if (gamedata.MapList[6].GetTile(pos).name.StartsWith("Road3rd")) tmpStr += "3";
                }
                else tmpStr += "0";
                if (tmpStr != "0-0-0")
                {
                    tmp = xmlDoc.CreateElement("road");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }

                //列加入行
                Colum.AppendChild(Row);

            }
            Map.AppendChild(Colum);

        }

        xmlDoc.Save(path);
    }

    public static void saveFacillitys(string path)//保存设施、特殊设施和特殊地形
    {
        FixGameData gamedata = FixGameData.FGD;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        xmlDoc.AppendChild(xmlDec);
        XmlNode root = xmlDoc.CreateElement("MapData");
        xmlDoc.AppendChild(root);

        XmlElement tmp = xmlDoc.CreateElement("Size");
        tmp.InnerText = GameUtility.mapSize.x + "*" + GameUtility.mapSize.y;
        root.AppendChild(tmp);

        XmlNode Map = xmlDoc.CreateElement("Map");
        root.AppendChild(Map);

        XmlElement Colum;
        XmlElement Row;
        string tmpStr;
        TileBase tmpTile;
        
        List<FacilityDataCell> tmpFacLst;

        Vector3Int pos;
        for (int y = 0; y <= GameUtility.mapSize.y; y++)
        {
            Colum = xmlDoc.CreateElement("Colum");
            Colum.SetAttribute("CNo", y.ToString());

            for (int x = 0; x <= GameUtility.mapSize.x; x++)
            {
                Row = xmlDoc.CreateElement("Row");
                Row.SetAttribute("RNo", x.ToString());
                pos = FixGameData.MapToWorld(y, x);

                //编写设施(格内)
                tmpTile = gamedata.MapList[7].GetTile(pos);
                if (tmpTile != null)
                {
                    tmpStr = "";
                    tmpStr += tmpTile.name;
                    tmp = xmlDoc.CreateElement("facilityC");
                    tmp.InnerText = tmpStr;
                    
                    tmpFacLst = FixGameData.FGD.FacilityList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if(tmpFacLst.Count == 0) tmpFacLst = FixGameData.FGD.SpecialFacilityList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if(tmpFacLst.Count != 0)
                    {
                        tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                        tmp.SetAttribute("active", tmpFacLst[0].active.ToString());
                    }

                    Row.AppendChild(tmp);
                }
                //编写设施(格边)
                tmpStr = "";
                tmpTile = gamedata.MapList[8].GetTile(pos);
                if (tmpTile != null) 
                {
                    tmpStr += tmpTile.name + "-";
                    tmpFacLst = FixGameData.FGD.FacilityList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if (tmpFacLst.Count == 0) tmpFacLst = FixGameData.FGD.SpecialFacilityList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());

                }  else tmpStr += "X-";

                tmpTile = gamedata.MapList[9].GetTile(pos);
                if (tmpTile != null)
                {
                    tmpStr += tmpTile.name + "-";
                    tmpFacLst = FixGameData.FGD.FacilityList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if (tmpFacLst.Count == 0) tmpFacLst = FixGameData.FGD.SpecialFacilityList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                } else tmpStr += "X-";
                
                tmpTile = gamedata.MapList[10].GetTile(pos);
                if (tmpTile != null) 
                {
                    tmpStr += tmpTile.name;
                    tmpFacLst = FixGameData.FGD.FacilityList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if (tmpFacLst.Count == 0) tmpFacLst = FixGameData.FGD.SpecialFacilityList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                } else tmpStr += "X";
                if (tmpStr != "X-X-X")
                {
                    tmp = xmlDoc.CreateElement("facilityS");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //特殊地形(格边)
                tmpStr = "";
                tmpTile = gamedata.MapList[11].GetTile(pos);
                if (tmpTile != null) 
                {
                    tmpStr += tmpTile.name + "-";
                    tmpFacLst = FixGameData.FGD.SpecialTerrainList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                } else tmpStr += "X-";

                tmpTile = gamedata.MapList[12].GetTile(pos);
                if (tmpTile != null) 
                {
                    tmpStr += tmpTile.name + "-";
                    tmpFacLst = FixGameData.FGD.SpecialTerrainList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                } else tmpStr += "X-";

                tmpTile = gamedata.MapList[13].GetTile(pos);
                if (tmpTile != null) 
                {
                    tmpStr += tmpTile.name;
                    tmpFacLst = FixGameData.FGD.SpecialTerrainList.Where(x => x.Positian == pos && x.Id.StartsWith(tmpTile.name)).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());
                } else tmpStr += "X";
                if (tmpStr != "X-X-X")
                {
                    tmp = xmlDoc.CreateElement("specialTerrainS");
                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }
                //特殊地形(格内)
                tmpTile = gamedata.MapList[14].GetTile(pos);
                if (tmpTile != null)
                {
                    tmpStr = "";
                    tmpStr += tmpTile.name;
                    tmp = xmlDoc.CreateElement("specialTerrain");
                    tmpFacLst = FixGameData.FGD.SpecialTerrainList.Where(x => x.Positian == pos && x.Id == tmpTile.name).ToList();
                    if (tmpFacLst.Count != 0) tmp.SetAttribute("stayTime", tmpFacLst[0].LastTime.ToString());

                    tmp.InnerText = tmpStr;
                    Row.AppendChild(tmp);
                }


                //列加入行
                if (Row.ChildNodes.Count != 0) Colum.AppendChild(Row);

            }
            if (Colum.ChildNodes.Count != 0) Map.AppendChild(Colum);

        }

        xmlDoc.Save(path);
    }

    public static void savePiece(string path)//保存场上棋子
    {
        FixGameData gamedata = FixGameData.FGD;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration Dec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        xmlDoc.AppendChild(Dec);
        XmlNode root = xmlDoc.CreateElement("PieceData");
        xmlDoc.AppendChild(root);

        XmlElement Humans = xmlDoc.CreateElement("Human");
        XmlElement Crash = xmlDoc.CreateElement("Crash");
        XmlElement tmp;
        Tuple<string, Vector2Int, string, int, int, bool> pData;
        //保存人类方棋子
        for (int i= 0; i < gamedata.HumanPieceParent.childCount; i++)
        {
            //pData = gamedata.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().getPieceData();
            pData = gamedata.HumanPieceParent.GetChild(i).GetComponent<rua>().getData();

            tmp = xmlDoc.CreateElement("Piece");
            tmp.SetAttribute("troopName", pData.Item1);
            tmp.SetAttribute("xPos", pData.Item2.x.ToString());
            tmp.SetAttribute("yPos", pData.Item2.y.ToString());
            
            tmp.SetAttribute("LoyalTo", pData.Item3);
            tmp.SetAttribute("stability", pData.Item4.ToString());
            tmp.SetAttribute("connectState", pData.Item5.ToString());
            tmp.SetAttribute("inCasualty", pData.Item6.ToString());
            
            Humans.AppendChild(tmp);

        }
        //保存崩坏方棋子
        for (int i = 0; i < gamedata.CrashPieceParent.childCount; i++)
        {
            //pData = gamedata.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>().getPieceData();
            pData = gamedata.CrashPieceParent.GetChild(i).GetComponent<rua>().getData();

            tmp = xmlDoc.CreateElement("Piece");
            tmp.SetAttribute("troopName", pData.Item1);
            tmp.SetAttribute("xPos", pData.Item2.x.ToString());
            tmp.SetAttribute("yPos", pData.Item2.y.ToString());
            
            tmp.SetAttribute("LoyalTo", pData.Item3);
            tmp.SetAttribute("stability", pData.Item4.ToString());
            tmp.SetAttribute("connectState", pData.Item5.ToString());
            tmp.SetAttribute("inCasualty", pData.Item6.ToString());
            
            Crash.AppendChild(tmp);

        }

        root.AppendChild(Humans);
        root.AppendChild(Crash);
        xmlDoc.Save(path);
    }

    public static void savePieceAsDefault(string path)//保存场上棋子为预设
    {
        FixGameData gamedata = FixGameData.FGD;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration Dec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        xmlDoc.AppendChild(Dec);
        XmlNode root = xmlDoc.CreateElement("PieceData");
        xmlDoc.AppendChild(root);

        XmlElement Humans = xmlDoc.CreateElement("Human");
        XmlElement Crash = xmlDoc.CreateElement("Crash");
        XmlElement tmp;
        Tuple<string, Vector2Int, string, int, int, bool> pData;
        //保存人类方棋子
        for (int i = 0; i < gamedata.HumanPieceParent.childCount; i++)
        {
            //pData = gamedata.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().getPieceData();
            pData = gamedata.HumanPieceParent.GetChild(i).GetComponent<rua>().getData();

            tmp = xmlDoc.CreateElement("Piece");
            tmp.SetAttribute("troopName", pData.Item1);
            tmp.SetAttribute("xPos", pData.Item2.x.ToString());
            tmp.SetAttribute("yPos", pData.Item2.y.ToString());

            Humans.AppendChild(tmp);

        }
        //保存崩坏方棋子
        for (int i = 0; i < gamedata.CrashPieceParent.childCount; i++)
        {
            //pData = gamedata.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>().getPieceData();
            pData = gamedata.CrashPieceParent.GetChild(i).GetComponent<rua>().getData();

            tmp = xmlDoc.CreateElement("Piece");
            tmp.SetAttribute("troopName", pData.Item1);
            tmp.SetAttribute("xPos", pData.Item2.x.ToString());
            tmp.SetAttribute("yPos", pData.Item2.y.ToString());

            Crash.AppendChild(tmp);

        }

        root.AppendChild(Humans);
        root.AppendChild(Crash);
        xmlDoc.Save(path);
    }


}

public enum ArmyBelong
{
    Human = 0,
    ModCrash = 1,
    Nutral = 2
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