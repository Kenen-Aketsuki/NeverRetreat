using System.Collections;
using System.Collections.Generic;

public static class BasicUtility
{
    public static void DataInit()
    {
        //��ȡ������Ϣ
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