using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
    
    public string PName { get { return TroopName + "/" + PieceName + "/" + PieceID; } }
    public string PDesignation { get { return Designation; } }
    public string BackColor { get {
            if (Belong == ArmyBelong.Human && !inCasualty) return "ff5ec4";
            else if (Belong == ArmyBelong.Human && inCasualty) return "ffb0e2";
            else if (Belong == ArmyBelong.ModCrash && !inCasualty) return "c30cc0";
            else return "d573d5";} }//背景色

    string TroopName;//部队名称
    string PieceName;//兵种名称
    string Designation;//番号
    string PieceID;//兵种id
    public ArmyBelong Belong;//部队从属
    public ArmyBelong LoyalTo;//部队效忠对象

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

    bool canCasualty { get
        {
            if(cATK < 0 && cDEF < 0 && cMaxMOV < 0) return false;
            else return true;
        } }//能否减员
    bool canLossConnect = true;//能否失联
    bool canDoubleCross = false;//能否被策反
    public bool isAir = false;//是否为空中单位(支援签)

    public bool canBattle {get{
            if (nATK > 0 && !canAirBattle) return true;
            else return false;
        }}//能否参与普通战斗
    public bool canSupport = false;//能否提供火力支援
    public bool canAirBattle = false;//能否参加空战
    public bool canDoMagic = false;//能否施法
    public bool canFixMod = false;//能否进行模组战
    public bool canBild = false;//能否进行建造
    public bool canMental = false;//能否进行心理战
    public bool canStrike = false;//能否进行火力打击
    
    public int activeArea = 0;//主动距离
    public int passiveArea = 0;//被动距离

    public bool isTwo { get {return cATK == cDEF && nATK == nDEF;} }
    public Piece(XmlNode root)
    {
        TroopName = root.Attributes["name"].Value;
        Designation = root.Attributes["designation"].Value;
        PieceID = root.Attributes["type"].Value;
        XmlNode data = FixSystemData.GlobalPieceDataList[PieceID];
        XmlNode tmp;
        //开始录入数据
        PieceName = data.SelectSingleNode("name").InnerText;
        tmp = data.SelectSingleNode("Datas");
        if (data.Attributes["isAir"] != null) isAir = bool.Parse(data.Attributes["isAir"].Value);
        else isAir = false;
        //正常数值
        #region
        nATK = int.Parse(tmp.SelectSingleNode("ATK").InnerText);
        nDEF = int.Parse(tmp.SelectSingleNode("DEF").InnerText);
        nMaxMOV = int.Parse(tmp.SelectSingleNode("MOV").InnerText);
        if (tmp.SelectSingleNode("cATK") != null) cATK = int.Parse(tmp.SelectSingleNode("cATK").InnerText);
        else cATK = -1;
        if (tmp.SelectSingleNode("cDEF") != null) cDEF = int.Parse(tmp.SelectSingleNode("cDEF").InnerText);
        else cDEF = -1;
        if (tmp.SelectSingleNode("cMOV") != null) cMaxMOV = int.Parse(tmp.SelectSingleNode("cMOV").InnerText);
        else cMaxMOV = -1;
        if (tmp.SelectSingleNode("activeArea") != null) activeArea = int.Parse(tmp.SelectSingleNode("activeArea").InnerText);
        else activeArea = -1;
        if (tmp.SelectSingleNode("passiveArea") != null) passiveArea = int.Parse(tmp.SelectSingleNode("passiveArea").InnerText);
        else passiveArea = -1;
        #endregion
        //读取能力
        #region
        tmp = data.SelectSingleNode("ability");
        if (tmp.Attributes["canSupport"] != null) canSupport = bool.Parse(tmp.Attributes["canSupport"].Value);
        if (tmp.Attributes["canAirBattle"] != null) canAirBattle = bool.Parse(tmp.Attributes["canAirBattle"].Value);
        if (tmp.Attributes["canDoMagic"] != null) canDoMagic = bool.Parse(tmp.Attributes["canDoMagic"].Value);
        if (tmp.Attributes["canFixMod"] != null) canFixMod = bool.Parse(tmp.Attributes["canFixMod"].Value);
        if (tmp.Attributes["canBild"] != null) canBild = bool.Parse(tmp.Attributes["canBild"].Value);
        if (tmp.Attributes["canMental"] != null) canMental = bool.Parse(tmp.Attributes["canMental"].Value);
        #endregion
        //衍生数值
        Belong = (ArmyBelong)Enum.Parse(typeof(ArmyBelong), root.Attributes["Belong"].Value);
        LoyalTo = Belong;
        //可策反、可失联
        if (Belong == ArmyBelong.Human)
        {
            canLossConnect = true;
            canDoubleCross = true;
        }
        else
        {
            canLossConnect = false;
            canDoubleCross = false;
        }
        //可进行间瞄火力打击
        if (canSupport && !isAir || isAir && nATK > 0) canStrike = true;
        else canStrike = false;
        OverTurn();
    }

    public void RecoverHP(int pt) //恢复HP
    {

    }

    public void RecoverStable(int pt)//恢复稳定性
    {

    }

    public void OverTurn() //过回合恢复
    {
        if (inCasualty) restMOV = cMaxMOV;
        else restMOV = nMaxMOV;
    }

    public void TakeDemage(int Dmg)//受伤
    {

    }

    public void TakeUnstable(int Dmg)//受到干扰
    {

    }

    public static void Dead( Piece deadPiece )//死亡
    {

    }

    public static void Move(Piece movePiece,Object Path)//移动
    {

    }

}


