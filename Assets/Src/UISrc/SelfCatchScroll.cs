using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelfCatchScroll : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform ContentPannel;
    [SerializeField]
    RectTransform Item;

    [SerializeField]
    VerticalLayoutGroup ItemGroup;
    [SerializeField]
    GameObject PieceDataPannel;
    [SerializeField]
    GameObject TerrainDataPannel;

    Dictionary<string,GameObject> pieceList;

    bool catching = false;
    float snapingSpeed;
    float snapingForce = 200;
    Vector2 lastPos;

    int currentItem;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < ContentPannel.childCount; i++)
        {
            GameObject.Destroy(ContentPannel.GetChild(i).gameObject);
        }
        TerrainDataPannel.SetActive(false);
        PieceDataPannel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        currentItem = Mathf.RoundToInt(ContentPannel.localPosition.y / (Item.rect.height + ItemGroup.spacing));
        float Velo = Mathf.Min(Mathf.Abs((ContentPannel.localPosition.y - lastPos.y) / Time.deltaTime), 300);
        lastPos = ContentPannel.localPosition;

        

        //if (scrollRect.velocity.magnitude < 200 && catching)
        if (Velo < 200 && catching)
        {
            float tarY = currentItem * (Item.rect.height + ItemGroup.spacing);
            scrollRect.velocity = Vector2.zero;
            snapingSpeed += snapingForce * Time.deltaTime;
            ContentPannel.localPosition = new Vector3(
                ContentPannel.localPosition.x,
                Mathf.MoveTowards(ContentPannel.localPosition.y, tarY, snapingSpeed),
                ContentPannel.localPosition.z);
            
            lastPos = ContentPannel.localPosition;
            if (ContentPannel.localPosition.y == tarY)
            {
                catching = false;
                SelectPiece();
            }
            
        }

        //if(scrollRect.velocity.magnitude >= 200)
        if (Velo >= 200)
        {
            catching = true;
            snapingSpeed = 0;
        }

    }

    public void UpdateCellChilds(List<GameObject> Datas,List<LandShape> basicTerrain)
    {
        //清空记录
        ClearCells();
        ContentPannel.localPosition = new Vector3(
                ContentPannel.localPosition.x,
                Mathf.MoveTowards(ContentPannel.localPosition.y, 0, snapingSpeed),
                ContentPannel.localPosition.z);
        currentItem = 0;

        //清空选择的棋子
        GameManager.GM.currentPiece = null;

        //放置地块
        GameObject newPiece = GameObject.Instantiate(Item.gameObject, ContentPannel);
        newPiece.name = "基础地形信息";
        UIPieceDataCell ps = newPiece.GetComponent<UIPieceDataCell>();
        ps.SetData(null, basicTerrain[0], null);

        FacilityDataCell dataCell = null;
        //放置设施
        if (basicTerrain[3] != null)
        {
            List<FacilityDataCell> tmpl = FixGameData.FGD.FacilityList.Where(x=>x.Positian == GameManager.GM.currentPosition).ToList();
            if (tmpl.Count != 0) dataCell = tmpl[0];
            else
            {
                dataCell = FixGameData.FGD.SpecialFacilityList.Where(x => x.Positian == GameManager.GM.currentPosition).First();
            }

            if(dataCell != null)
            {
                newPiece = GameObject.Instantiate(Item.gameObject, ContentPannel);
                newPiece.name = "芝士设施";
                ps = newPiece.GetComponent<UIPieceDataCell>();
                ps.SetData(null, basicTerrain[3], dataCell);
                dataCell = null;
            }
            
        }
        //放置特殊地形



        //放置棋子
        Datas.Reverse();
        pieceList = Datas.ToDictionary(x => x.name, x=>x);
        foreach (GameObject Data in Datas)
        {
            newPiece = GameObject.Instantiate(Item.gameObject, ContentPannel);
            newPiece.name = Data.name;
            ps = newPiece.GetComponent<UIPieceDataCell>();
            ps.SetData(Data.GetComponent<OB_Piece>().getPieceData(), null,null);
        }
        
        SelectPiece();
    }

    public void UpdateCellChilds()
    {
        List<GameObject> PieceLst = FixGameData.FGD.HumanPiecePool.getChildByPos(GameManager.GM.currentPosition);
        if (PieceLst.Count == 0) PieceLst = FixGameData.FGD.CrashPiecePool.getChildByPos(GameManager.GM.currentPosition);

        List<LandShape> LandLst = Map.GetPLaceInfo(GameManager.GM.currentPosition, 0);
        UpdateCellChilds(PieceLst, LandLst);
    }

    public void ClearCells()
    {
        for (int i = 0; i < ContentPannel.childCount; i++)
        {
            GameObject.Destroy(ContentPannel.GetChild(i).gameObject);
        }
    }

    void SelectPiece()
    {
        //GameManager.GM.currentPiece = null;
        //Debug.Log(ContentPannel.childCount - currentItem - 1 + " — " + ContentPannel.childCount);
        UIPieceDataCell tar = ContentPannel.GetChild(ContentPannel.childCount - currentItem - 1).gameObject.GetComponent<UIPieceDataCell>();
        //UIPieceDataCell tar = ContentPannel.GetChild(0).gameObject.GetComponent<UIPieceDataCell>();
        //return;

        if (tar.Data != null && tar.Data.LoyalTo == GameManager.GM.ActionSide)
        {
            if (GameManager.GM.currentPiece != null) GameManager.GM.currentPiece.gameObject.SetActive(false);
            //foreach (KeyValuePair<string, GameObject> par in pieceList) par.Value.SetActive(false);

            GameManager.GM.currentPiece = pieceList[tar.name].GetComponent<OB_Piece>();
            GameManager.GM.currentPiece.gameObject.SetActive(true);

            SetPieceTextShow(GameManager.GM.currentPiece.getPieceData());
            (FixGameData.FGD.uiManager.actUI as IUIHandler).OnPieceSelect(true);

            //刷新选择
            GameManager.GM.MoveArea = null;
            FixGameData.FGD.MoveAreaMap.ClearAllTiles();


            GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
        }
        else if(tar.Data != null && tar.Data.LoyalTo != GameManager.GM.ActionSide)
        {
            SetPieceTextShow(pieceList[tar.name].GetComponent<OB_Piece>().getPieceData());

            GameManager.GM.currentPiece = null;

            (FixGameData.FGD.uiManager.actUI as IUIHandler).OnPieceSelect(false);
        }
        else
        {
            TerrainDataPannel.gameObject.SetActive(true);
            PieceDataPannel.gameObject.SetActive(false);

            GameManager.GM.currentPiece = null;

            (FixGameData.FGD.uiManager.actUI as IUIHandler).OnTerrainSelect(SetTerrainTextShow(tar));

            
            GameManager.GM.SetMachineState(MachineState.FocusOnTerrain);
        }
    }

    public void SelectPieceWithoutFocus()
    {
        int i = 0;
        List<GameObject> lst = GameManager.GM.ActionPool.getChildByPos(GameManager.GM.currentPosition);

        foreach(var piece in pieceList)
        {
            Debug.Log(piece.Key + " - " + lst[i].name);
            i++;
        }
    }

    void SetPieceTextShow(Piece cDt)
    {
        //设置显示内容
        TerrainDataPannel.SetActive(false);
        PieceDataPannel.SetActive(true);
        string[] nams = cDt.PName.Split("/");
        PieceDataPannel.GetComponent<RectTransform>().GetChild(0).GetComponent<TMP_Text>().text = nams[1] ;
        PieceDataPannel.GetComponent<RectTransform>().GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = (cDt.LoyalTo == cDt.Belong ? nams[0] : "叛军");

        string str = "补给状态：" + cDt.SupplyStr
            + "\n</color>连接状态：" + cDt.ConnectStr
            + "\n</color>设备状态：" + cDt.StabilityStr
            + "\n</color>部队状态：" + cDt.HealthStr;

        PieceDataPannel.GetComponent<RectTransform>().GetChild(2).GetComponent<TMP_Text>().text = str;
    }
    bool SetTerrainTextShow(UIPieceDataCell terrID)
    {
        TerrainDataPannel.SetActive(true);
        PieceDataPannel.SetActive(false);
        string str = "";
        bool isFac = false;
        if(terrID.FacData != null)
        {
            //写设施信息
            //Debug.Log(terrID.FacData.Data.Item2.GetType());
            if(terrID.FacData.Data.Item1 == typeof(Facility))
            {
                str += "从属：" + ((terrID.FacData.Data.Item2 as Facility).Belone == ArmyBelong.Human ? "人类联军" : "崩坏意志");

            }
            else
            {
                str += "从属：" + ((terrID.FacData.Data.Item2 as SpecialFacility).Belone == ArmyBelong.Human ? "人类联军" : "崩坏意志");
                if((terrID.FacData.Data.Item2 as SpecialFacility).Belone == ArmyBelong.Human) str += "\n状态：" + (terrID.FacData.active ? "运行" : "待机");
                else str += "\n状态：" + (terrID.FacData.active ? "激活态" : "先兆态");
            }
            isFac = true;
        }
        else
        {
            //写地形信息
            str += "堆叠 " + Map.GetHereStack(GameManager.GM.currentPosition, GameManager.GM.ActionSide);
            str += "\n净移动力消耗 " + Map.GetNearMov(GameManager.GM.currentPosition, 0, GameManager.GM.ActionSide);
            str += "\n本格高度 " + Map.GetInCellHeight(GameManager.GM.currentPosition)+"\n高度影响火炮的打击范围，请谨慎选择火炮的打击地点";
            str += "\n方向 \t1  \t2  \t3  \t4  \t5  \t6";
            str += "\n移动力 \t";
            for(int i= 1; i < 7; i++)
            {
                float tmpf = Map.GetNearMov(Map.GetRoundSlotPos(GameManager.GM.currentPosition, i), (i + 2) % 6 + 1, GameManager.GM.ActionSide);
                str += (tmpf == -1 ? "禁入" : tmpf == -2 ? "全部" : tmpf) + "\t";
            }
            str += "\nATK修正 \t";
            for (int i = 1; i < 7; i++)
            {
                float tmpf = Map.GetTargetATK(Map.GetRoundSlotPos(GameManager.GM.currentPosition, i), (i + 2) % 6 + 1, GameManager.GM.ActionSide,4);
                str += tmpf + "\t";
            }
            str += "\nATK修正结果是基础攻击为4时的结果\nDEF修正 \t"+ Map.GetTargetDEFK(GameManager.GM.currentPosition, GameManager.GM.ActionSide, 1)+
                " *DEF修正与边设施、地形无关";
            
            str += "\n战果修正 \t";
            for (int i = 1; i < 7; i++)
            {
                float tmpf = Map.GetBattleRRK(Map.GetRoundSlotPos(GameManager.GM.currentPosition, i), (i + 2) % 6 + 1, GameManager.GM.ActionSide, 0);
                str += tmpf + "\t";
            }
            str += "\n战果以进攻方角度描述，修正值大于0表示优势，小于0表示劣势";

            str += "\n格边高度 \t";
            for (int i = 1; i < 7; i++)
            {
                float tmpf = Map.GetCellSideHeight(GameManager.GM.currentPosition, i);
                str += tmpf + "\t";
            }
            str += "\n高度会影响火炮的打击范围，请谨慎选择打击地点";

        }
        TerrainDataPannel.GetComponent<RectTransform>().GetChild(1).GetChild(0).GetChild(0).transform.localPosition -= new Vector3(-500, 50, 0);
        TerrainDataPannel.GetComponent<RectTransform>().GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = str;
        TerrainDataPannel.GetComponent<RectTransform>().GetChild(0).GetComponent<TMP_Text>().text = terrID.LandData.name;

        return isFac;
    }

    
}
