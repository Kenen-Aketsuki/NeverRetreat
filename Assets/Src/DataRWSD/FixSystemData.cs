using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine.XR;

public static class FixSystemData
{
    public static int ImagSize = 256;

    public static string rootDirectory;
    public static string dataDirectory;
    public static string PieceDirectory;
    public static string TerrainDirectory;
    public static string FormDirectory;

    //���α�
    public static Dictionary<string, BasicLandShape> GlobalBasicTerrainList = new Dictionary<string, BasicLandShape>();
    //��ʩ��
    public static Dictionary<string, Facility> GlobalFacilityList = new Dictionary<string, Facility>();
    public static Dictionary<string, SpecialFacility> GlobalSpFacilityList = new Dictionary<string, SpecialFacility>();
    //������α�
    public static Dictionary<string, Facility> GlobalSpecialTerrainList = new Dictionary<string, Facility>();
    //�����
    public static Dictionary<string, Zone> GlobalZoneList = new Dictionary<string, Zone>();

    //���ֱ�
    public static Dictionary<string, XmlNode> GlobalPieceDataList = new Dictionary<string, XmlNode>();
    //���Ʊ�
    public static Dictionary <string, XmlNode> HumanOrganizationList = new Dictionary<string, XmlNode>();//���෽
    public static Dictionary<string, XmlNode> CrashOrganizationList = new Dictionary<string, XmlNode>();//ģ�鷽

    //ս���ö���
    public static BattleJudgeForm battleJudgeForm;
    public static AirBattleJudgeForm airBattleJudgeForm;
    public static FireRankForm fireRankForm;
    public static FireStrikeJudgeForm fireStrikeJudgeForm;

    public static void InitPath()
    {
        rootDirectory = Environment.CurrentDirectory;

        dataDirectory = rootDirectory + "\\GameData";

        PieceDirectory = dataDirectory + "\\Piece";
        if (!Directory.Exists(PieceDirectory)) PieceDirectory = "MISS";

        TerrainDirectory = dataDirectory + "\\Terrain";
        if (!Directory.Exists(TerrainDirectory)) TerrainDirectory = "MISS";

        FormDirectory = dataDirectory + "\\Form";
        if (!Directory.Exists(FormDirectory)) FormDirectory = "MISS";
    }

    public static string toString()
    {
        string tmp = "";
        tmp += "��Ϸ·����" + rootDirectory + "\n";
        tmp += "����·����" + dataDirectory + "\n";
        tmp += "����·����" + PieceDirectory + "\n";
        tmp += "�ؿ�·����" + TerrainDirectory + "\n";

        return tmp;
    }
}
