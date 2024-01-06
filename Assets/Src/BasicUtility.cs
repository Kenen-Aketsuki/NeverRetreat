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
        //��ȡ������Ϣ
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

enum FixData//���ṩ��������ֵ��
{
    ATK,//������ս��
    DEF,//���ط�ս��
    RRK,//ս�����������ö������
    MOV,//�����ƶ���,��һ����·�Ĵ�ֵΪ-1�������˵ؽ����ƶ���ʱҪ��ȥ1
    STK,//�ѵ�
    HP//����˺�
}
enum FixWay//������ʽ
{
    ADD,//�Ӽ�
    MULTY,//�˳�
    NOPE,//��ֹ
    ALL//ʣ��ȫ��
}