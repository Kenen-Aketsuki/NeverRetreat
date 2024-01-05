using System.Collections;
using System.Collections.Generic;

public static class BasicUtility
{
    public static void DataInit()
    {
        //读取地形信息
        //读取棋子信息
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

enum FixData//可提供修正的数值项
{
    ATK,//进攻方战力
    DEF,//防守方战力
    RRK,//战果评级，即裁定表的列
    MOV,//进入移动力,如一级道路的此值为-1，则计算此地进入移动力时要减去1
    STK,//堆叠
    HP//造成伤害
}
enum FixWay//修正方式
{
    ADD,//加减
    MULTY,//乘除
    NOPE,//禁止
    ALL//剩余全部
}