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
        //��ʼ���̶�����
        FixSystemData.InitPath();
        //��ȡ���Ρ���ʩ�ȵ�ͼ��Ϣ��Ϣ
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
                        UnityEngine.Debug.Log(node.Attributes["id"].Value+"�� SKIP");
                        break;
                }
            }
        }
        //��ȡ������Ϣ
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
        //��ȡ���ӱ���
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
        //��ȡ������������(��������������ʵ��)
        //��ȡ�Ĵ�ö���
        foreach (string file in Directory.GetFiles(FixSystemData.FormDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            XmlNode tmp = XmlDoc.DocumentElement.FirstChild;
            switch (tmp.Attributes["id"].Value)
            {
                case "BattleJudgeForm":
                    FixSystemData.battleJudgeForm = new BattleJudgeForm(tmp);
                    Debug.Log("���� ս���ö���");
                    break;
                case "AirBattleJudgeForm":
                    FixSystemData.airBattleJudgeForm = new AirBattleJudgeForm(tmp);
                    Debug.Log("���� ��ս�ö���");
                    break;
                case "FireRankForm":
                    FixSystemData.fireRankForm = new FireRankForm(tmp);
                    Debug.Log("���� ������");
                    break;
                case "FireStrikeJudgeForm":
                    FixSystemData.fireStrikeJudgeForm = new FireStrikeJudgeForm(tmp);
                    Debug.Log("���� ��������ö���");
                    break;
                default:
                    Debug.Log(file + " ���Ĳö�������");
                    break;
            }
        }
    }

    public static void SpawnPiece(string TroopName,Vector3 Pos)//�Բ��ӷ���Ϊ��������һ������
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

    public static void saveMap()
    {

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

public enum FixData//���ṩ��������ֵ��
{
    ATK,//������ս��
    DEF,//���ط�ս��
    RRK,//ս�����������ö������
    MOV,//�����ƶ���,��һ����·�Ĵ�ֵΪ-1�������˵ؽ����ƶ���ʱҪ��ȥ1
    STK,//�ѵ�
    HP//����˺�
}
public enum FixWay//������ʽ
{
    ADD,//�Ӽ�
    MULTY,//�˳�
    NOPE,//��ֹ
    ALL//ʣ��ȫ��
}