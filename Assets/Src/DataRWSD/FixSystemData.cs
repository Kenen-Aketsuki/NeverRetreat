using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public static class FixSystemData
{
    public static int ImagSize = 256;

    public static string rootDirectory;
    public static string dataDirectory;
    public static string PieceDirectory;
    public static string TerrainDirectory;

    //地形表
    public static Dictionary<string, BasicLandShape> GlobalBasicTerrainList = new Dictionary<string, BasicLandShape>();
    //设施表
    public static Dictionary<string, Facility> GlobalFacilityList = new Dictionary<string, Facility>();
    public static Dictionary<string, SpecialFacility> GlobalSpFacilityList = new Dictionary<string, SpecialFacility>();
    //特殊地形表
    public static Dictionary<string, Facility> GlobalSpecialTerrainList = new Dictionary<string, Facility>();
    
    //兵种表
    public static Dictionary<string, XmlNode> GlobalPieceDataList = new Dictionary<string, XmlNode>();
    //编制表
    public static Dictionary <string, XmlNode> HumanOrganizationList = new Dictionary<string, XmlNode>();//人类方
    public static Dictionary<string, XmlNode> CrashOrganizationList = new Dictionary<string, XmlNode>();//模组方

    public static void InitPath()
    {
        rootDirectory = Environment.CurrentDirectory;

        dataDirectory = rootDirectory + "\\GameData";

        PieceDirectory = dataDirectory + "\\Piece";
        if (!Directory.Exists(PieceDirectory)) PieceDirectory = "MISS";

        TerrainDirectory = dataDirectory + "\\Terrain";
        if (!Directory.Exists(TerrainDirectory)) TerrainDirectory = "MISS";
    }

    public static string toString()
    {
        string tmp = "";
        tmp += "游戏路径：" + rootDirectory + "\n";
        tmp += "数据路径：" + dataDirectory + "\n";
        tmp += "棋子路径：" + PieceDirectory + "\n";
        tmp += "地块路径：" + TerrainDirectory + "\n";

        return tmp;
    }
}
