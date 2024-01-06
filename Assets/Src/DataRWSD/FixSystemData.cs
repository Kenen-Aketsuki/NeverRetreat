using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class FixSystemData
{
    public static int ImagSize = 256;

    public static string rootDirectory;
    public static string dataDirectory;
    public static string pieceDirectory;
    public static string TerrainDirectory;

    public static Dictionary<string, BasicLandShape> GlobalBasicTerrainList = new Dictionary<string, BasicLandShape>();
    public static Dictionary<string, Facility> GlobalFacilityList = new Dictionary<string, Facility>();
    public static Dictionary<string, Facility> GlobalSpecialTerrainList = new Dictionary<string, Facility>();

    public static void InitPath()
    {
        rootDirectory = Environment.CurrentDirectory;

        dataDirectory = rootDirectory + "\\GameData";

        pieceDirectory = dataDirectory + "\\Piece";
        if (!Directory.Exists(pieceDirectory)) pieceDirectory = "MISS";

        TerrainDirectory = dataDirectory + "\\Terrain";
        if (!Directory.Exists(TerrainDirectory)) TerrainDirectory = "MISS";
    }

    public static string toString()
    {
        string tmp = "";
        tmp += "游戏路径：" + rootDirectory + "\n";
        tmp += "数据路径：" + dataDirectory + "\n";
        tmp += "棋子路径：" + pieceDirectory + "\n";
        tmp += "地块路径：" + TerrainDirectory + "\n";

        return tmp;
    }
}
