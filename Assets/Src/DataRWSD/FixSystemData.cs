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
    public static string pieceDirectory;
    public static string TerrainDirectory;

    //���α�
    public static Dictionary<string, BasicLandShape> GlobalBasicTerrainList = new Dictionary<string, BasicLandShape>();
    //��ʩ��
    public static Dictionary<string, Facility> GlobalFacilityList = new Dictionary<string, Facility>();
    //������α�
    public static Dictionary<string, Facility> GlobalSpecialTerrainList = new Dictionary<string, Facility>();
    
    //���ֱ�
    public static Dictionary<string, XmlNode> GlobalPieceDataList = new Dictionary<string, XmlNode>();
    //���Ʊ�
    public static Dictionary <string, XmlNode> HumanOrganizationList = new Dictionary<string, XmlNode>();//���෽
    public static Dictionary<string, XmlNode> ModeOrganizationList = new Dictionary<string, XmlNode>();//ģ�鷽

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
