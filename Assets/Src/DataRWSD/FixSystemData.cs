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
        tmp += "��Ϸ·����" + rootDirectory + "\n";
        tmp += "����·����" + dataDirectory + "\n";
        tmp += "����·����" + pieceDirectory + "\n";
        tmp += "�ؿ�·����" + TerrainDirectory + "\n";

        return tmp;
    }
}
