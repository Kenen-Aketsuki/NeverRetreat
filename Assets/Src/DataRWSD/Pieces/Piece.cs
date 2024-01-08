using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class Piece
{
    public int DEF {get
        {
            double tmp;
            if (inCasualty) tmp = cDEF;
            else tmp = nDEF;

            if (Stability == 2) tmp *= 0.5;//失能

            return (int)Math.Ceiling(tmp);
        }}//真实防御力
    public int ATK
    {
        get
        {
            double tmp;
            if (inCasualty) tmp = cATK;
            else tmp = nATK;

            if (Stability == 1) tmp *= 0.5;//瘫痪
            else if (Stability == 2) tmp = -1;//失能

            if (ConnectState == 1) tmp *= 0.5;//失联
            else if (ConnectState == 2) tmp = -1;//孤立
            else if (isUnsupply && ConnectState ==0) tmp *= 0.75;//断补

            return (int)Math.Floor(tmp);
        }
    }//真实战斗力
    public int MOV
    {
        get
        {
            return restMOV;
        }
    }//真实移动力

    public string PName { get { return Name; } }
    public string PDesignation { get { return Designation; } }

    string Name;//名称
    string Designation;//番号
    ArmyBelong Belong;//部队从属
    ArmyBelong LoyalTo;//部队效忠对象

    int Stability = 0;//判定是否为“正常”-“瘫痪”-“失能”
    int ConnectState = 0;//判定是否为 正常 - 失联 - 孤立
    //数值，C开头的为减员态数值,N开头为正常态数值,为小于零时代表其不能执行此操作
    int nATK;
    int cATK;
    int nDEF;
    int cDEF;
    int restMOV;
    int nMaxMOV;
    int cMaxMOV;

    
    bool inCasualty = false;// 是否减员
    bool isUnsupply = false;//是否断补

    bool canCasualty = false;//能否减员
    bool canLossConnect = true;//能否失联
    bool canDoubleCross = false;//能否被策反
    bool isAir = false;//是否为空中单位(支援签)

    public bool canBattle = true;//能否参与普通战斗
    public bool canSupport = false;//能否提供火力支援
    public bool canAirBattle = false;//能否参加空战
    public bool canDoMagic = false;//能否施法
    public bool canFixMod = false;//能否进行模组战
    public bool canBild = false;//能否进行建造
    public bool canMental = false;//能否进行心理战

    public int activeArea = 0;//主动距离
    public int passivdArea = 0;//被动距离

    public Piece(XmlNode root)
    {

    }

    public void RecoverHP(int pt) //恢复HP
    {

    }

    public void RecoverStable(int pt)//恢复稳定性
    {

    }

    public void OverTurn() //过回合恢复
    {

    }

    public void TakeDemage(int Dmg)//受伤
    {

    }

    public static void Dead( Piece deadPiece )//死亡
    {

    }

    public static void Move(Piece movePiece,Object Path)//移动
    {

    }

}


