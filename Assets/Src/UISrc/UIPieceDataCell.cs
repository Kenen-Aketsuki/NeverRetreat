using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPieceDataCell : MonoBehaviour
{
    public Piece Data { get; private set; }
    public string TerrainID;
    public FacilityDataCell FacData { get; private set; }
    public LandShape LandData { get; private set; }

    //图像数据
    [SerializeField]
    Image BaseColor;
    [SerializeField]
    GameObject CrashCover;
    [SerializeField]
    Image TroopType;
    [SerializeField]
    GameObject AreaSlash;
    [SerializeField]
    GameObject DataBack;
    [SerializeField]
    Image GroundIcon;
    //文字数据
    [SerializeField]
    RectTransform BaseData_Three;
    [SerializeField]
    RectTransform BaseData_Two;
    [SerializeField]
    TMP_Text TroopName;
    [SerializeField]
    TMP_Text ActiveArea;
    [SerializeField]
    TMP_Text PassiveArea;

    void DataInit()
    {
        if (Data == null) return;
        //设置显示
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);
        //设置背景
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        //文字显示
        TroopName.text = Data.PDesignation;

        if (Data.isTwo)
        {
            string ATK;
            if (Data.ATK < 0) ATK = "X"; else ATK = Data.ATK.ToString();

            BaseData_Two.GetChild(2).GetComponent<TMP_Text>().text = Data.MOV.ToString();
            BaseData_Two.GetChild(0).GetComponent<TMP_Text>().text = ATK;
            BaseData_Two.gameObject.SetActive(true);
        }
        else
        {
            string ATK;
            string DEF;
            string MOV;
            if (Data.ATK < 0) ATK = "X"; else ATK = Data.ATK.ToString();
            if (Data.DEF < 0) DEF = "X"; else DEF = Data.DEF.ToString();
            if (Data.MOV < 0) MOV = "X"; else MOV = Data.MOV.ToString();
            BaseData_Three.GetChild(4).GetComponent<TMP_Text>().text = MOV;
            BaseData_Three.GetChild(2).GetComponent<TMP_Text>().text = DEF;
            BaseData_Three.GetChild(0).GetComponent<TMP_Text>().text = ATK;
            BaseData_Three.gameObject.SetActive(true);

            if (!Data.canAirBattle && Data.isAir) BaseData_Three.GetChild(2).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
            if (Data.canAirBattle && !Data.isAir) BaseData_Three.GetChild(4).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
        }
        if (Data.activeArea > 0) ActiveArea.text = Data.activeArea.ToString();
        else ActiveArea.gameObject.SetActive(false);
        if (Data.passiveArea > 0) PassiveArea.text = Data.passiveArea.ToString();
        else PassiveArea.gameObject.SetActive(false);
    }

    public void SetData(Piece Data, LandShape Land,FacilityDataCell CellData)
    {
        this.Data = Data;
        FacData = CellData;
        LandData = Land;

        if (Data == null)
        {
            GroundIcon.gameObject.SetActive(true);

            if (Land.Top != null) GroundIcon.sprite = Land.Top.sprite;
            else if(CellData.active) GroundIcon.sprite = Land.Active.sprite;
            else GroundIcon.sprite = Land.Close.sprite;


            BaseData_Three.gameObject.SetActive(false);
            BaseData_Two.gameObject.SetActive(false);
            TroopName.gameObject.SetActive(false);
            ActiveArea.gameObject.SetActive(false); ;
            PassiveArea.gameObject.SetActive(false);

            BaseColor.gameObject.SetActive(false);
            CrashCover.SetActive(false);
            TroopType.gameObject.SetActive(false);
            AreaSlash.SetActive(false);
            DataBack.SetActive(false);
        }

        DataInit();
    }

    public string getPieceInfo()
    {
        bool isHuman = Data.Belong == ArmyBelong.Human;
        string str = "";
        //所属
        str += "所属：" + (isHuman ? "人类势力" : "崩坏意志");
        //部队名
        string[] tmpArr = Data.PName.Split("/");
        str += "\n番号：" + tmpArr[0] + " — " + tmpArr[1] + " \\ " +(Data.isAir ? "空中单位" : "地面单位");
        //是否拥有带宽
        if (!isHuman)
        {
            str += "\n占用带宽：" + Data.crashLoad;
        }
        //部队能力
        str += "\n部队功能：";

        //public bool canBattle//能否参与普通战斗
        if (Data.canBattle) str += "常规作战\t";
        //public bool canSupport = false;//能否提供火力支援
        if (Data.canSupport) str += "火力支援\t";
        //public bool canAirBattle = false;//能否参加空战
        if (Data.canAirBattle) str += "对空打击\t";
        //public bool canDoMagic = false;//能否施法
        if (Data.canDoMagic) str += "施法\t";
        //public bool canFixMod = false;//能否进行模组战
        if (Data.canFixMod) str += "模组战\t";
        //public bool canBild = false;//能否进行建造
        if (Data.canBild) str += "建造\t";
        //public bool canMental = false;//能否进行心理战
        if (Data.canMental) str += "蛊惑人心\t";
        //public bool canStrike = false;//能否进行火力打击
        if (Data.canStrike) str += "对地打击";





        return str;
    }
}
