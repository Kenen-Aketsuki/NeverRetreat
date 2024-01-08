using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public static class BasicUtility
{
    public static void DataInit()
    {
        XmlDocument XmlDoc = new XmlDocument();
        //��ʼ���̶�����
        FixSystemData.InitPath();
        //��ȡ���Ρ���ʩ�ȵ�ͼ��Ϣ��Ϣ
        foreach(string file in Directory.GetFiles(FixSystemData.TerrainDirectory, "*.xml", SearchOption.AllDirectories))
        {
            XmlDoc.Load(file);
            XmlNodeList child = XmlDoc.DocumentElement.ChildNodes;

            foreach (XmlNode node in child)
            {
                if (node.Attributes == null) continue;

                switch (node.Attributes["Type"].Value)
                {
                    case "BasicTerrain":
                        UnityEngine.Debug.Log(node.Attributes["id"].Value + "�� IN");
                        
                        FixSystemData.GlobalBasicTerrainList.Add(
                            node.Attributes["id"].Value,
                            new BasicLandShape(node)
                            );
                        break;
                    case "TempFacility":
                    case "FixFacility":
                        UnityEngine.Debug.Log(node.Attributes["id"].Value + "�� IN");

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
                    default:
                        UnityEngine.Debug.Log(node.Attributes["id"].Value+"�� SKIP");
                        break;
                }
            }
        }
        //��ȡ������Ϣ

    }
}

public enum ArmyBelong
{
    Human,
    ModCrash
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