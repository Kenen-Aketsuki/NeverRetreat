using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIReforenceWindow : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform ContentPannel;
    [SerializeField]
    RectTransform Item;
    [SerializeField]
    HorizontalLayoutGroup ItemGroup;

    [SerializeField]
    TMP_Text MountShow;

    [SerializeField]
    TMP_Text Text;

    [SerializeField]
    TMP_Text turnShow;

    bool catching = false;
    float snapingSpeed;
    float snapingForce = 20;
    Vector2 lastPos;
    int currentItem;
    int halfFachInt;

    int TmpBindWide;
    int avabileWide;

    UIPieceDataCell SelectedPiece;
    Dictionary<string, Tuple<int, int>> SelectList = new Dictionary<string, Tuple<int, int>>();


    private void OnEnable()
    {
        SelectList.Clear();
        UpdatePieceList();

        avabileWide = GameManager.GM.MaxCrashBandwidth - GameManager.GM.CrashBandwidth;
        TmpBindWide = 0;
        

        halfFachInt = Mathf.FloorToInt(ContentPannel.childCount / 2);
        if(ContentPannel.childCount %2 == 0)
        {
            ItemGroup.padding.left = Mathf.RoundToInt((Item.rect.width + ItemGroup.spacing));
        }

        UpdateTextShow();
    }

    private void OnDisable()
    {
        ClearCells();
    }

    void Update()
    {
        #region //自捕捉滚动视图
        //currentItem = Mathf.RoundToInt(ContentPannel.localPosition.x / (Item.rect.width + ItemGroup.spacing));
        float Velo = Mathf.Abs((ContentPannel.localPosition.x - lastPos.x) / Time.deltaTime);
        lastPos = ContentPannel.localPosition;

        //if (scrollRect.velocity.magnitude < 200 && catching)
        if (Velo < 2 && catching)
        {
            float tarX = currentItem * (Item.rect.width + ItemGroup.spacing);
            scrollRect.velocity = Vector2.zero;
            snapingSpeed += snapingForce * Time.deltaTime;
            ContentPannel.localPosition = new Vector3(
                Mathf.MoveTowards(ContentPannel.localPosition.x, tarX, snapingSpeed),
                ContentPannel.localPosition.y,
                ContentPannel.localPosition.z);

            lastPos = ContentPannel.localPosition;
            if (ContentPannel.localPosition.x == tarX)
            {
                //ContentPannel.localPosition = new Vector3(
                //    tarX,
                //    ContentPannel.localPosition.y,
                //    ContentPannel.localPosition.z);
                catching = false;
                SelectPiece();
            }

        }

        //if(scrollRect.velocity.magnitude >= 200)
        if (Velo >= 2)
        {
            currentItem = Mathf.RoundToInt(ContentPannel.localPosition.x / (Item.rect.width + ItemGroup.spacing));
            if (currentItem + 1 + halfFachInt > ContentPannel.childCount) currentItem = ContentPannel.childCount - 1 - halfFachInt;
            else if (currentItem + halfFachInt < 0) currentItem = -halfFachInt;
            catching = true;
            snapingSpeed = 0;
        }

        #endregion
    }

    void SelectPiece()
    {
        int addr = currentItem + halfFachInt;
        SelectedPiece = ContentPannel.GetChild(addr).GetComponent<UIPieceDataCell>();
        Text.text = SelectedPiece.getPieceInfo();
        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].Item2.ToString();
    }

    void UpdatePieceList()
    {
        ClearCells();

        List<string> pieceLst;
        Dictionary<string, XmlNode> OrgDic;

        if(GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            pieceLst = FixGameData.FGD.HumanDeathList;
            OrgDic = FixSystemData.HumanOrganizationList;
        }
        else
        {
            pieceLst = FixGameData.FGD.CrashDeathList;
            OrgDic = FixSystemData.CrashOrganizationList;
        }

        for(int i = 0; i < pieceLst.Count; i++)
        {
            string troopNam = pieceLst[i];
            Piece PData = new Piece(OrgDic[troopNam], null);
            GameObject newPiece = GameObject.Instantiate(Item.gameObject, ContentPannel);
            newPiece.name = PData.PDesignation;
            UIPieceDataCell ps = newPiece.GetComponent<UIPieceDataCell>();
            ps.SetData(PData, null, null);
            if (!SelectList.ContainsKey(troopNam)) SelectList.Add(troopNam, new Tuple<int, int>(PData.crashLoad, 0));
        }

        currentItem = 0;

        SelectPiece();
    }

    public void closeWindow()
    {
        gameObject.SetActive(false);
    }

    public void conformChoice()
    {
        if(GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            FixGameData.FGD.HumanLoadList.Clear();
            foreach (KeyValuePair<string,Tuple<int,int>> pare in SelectList)
            {
                if (pare.Value.Item2 != 0 && FixGameData.FGD.HumanLoadList.Where(x=>x.Item1 == pare.Key).ToList().Count == 0) 
                    FixGameData.FGD.HumanLoadList.Add(new Tuple<string, string, int>(
                    pare.Key,
                    "HunterGuild",
                    0
                    ));
            }
            GameManager.GM.PreTrainTroop += TmpBindWide;
        }
        else
        {
            FixGameData.FGD.CrashLoadList.Clear();
            foreach (KeyValuePair<string, Tuple<int, int>> pare in SelectList)
            {
                if (pare.Value.Item2 != 0)
                    FixGameData.FGD.CrashLoadList.Add(new Tuple<string, string, int>(
                    pare.Key,
                    "DimensionFissure",
                    UnityEngine.Random.Range(0, 1)
                    ));
            }
            GameManager.GM.CrashBandwidth += TmpBindWide;
        }

        gameObject.SetActive(false);
    }

    public void PAdd()
    {
        Tuple<int, int> tmp = SelectList[SelectedPiece.Data.PDesignation];

        if(GameManager.GM.ActionSide == ArmyBelong.ModCrash && TmpBindWide + tmp.Item1 <= avabileWide)
        {
            SelectList[SelectedPiece.Data.PDesignation] = new Tuple<int, int>(tmp.Item1, tmp.Item2 + 1);
            TmpBindWide += tmp.Item1;
        }
        else if(GameManager.GM.ActionSide == ArmyBelong.Human && tmp.Item2 == 0 && GameManager.GM.PreTrainTroop > 0)
        {
            SelectList[SelectedPiece.Data.PDesignation] = new Tuple<int, int>(tmp.Item1, tmp.Item2 + 1);
            TmpBindWide += 2;
        }
        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].Item2.ToString();
        UpdateTextShow();

    }
    public void PSubtract()
    {
        Tuple<int, int> tmp = SelectList[SelectedPiece.Data.PDesignation];

        if (GameManager.GM.ActionSide == ArmyBelong.ModCrash && tmp.Item2 > 0)
        {
            SelectList[SelectedPiece.Data.PDesignation] = new Tuple<int, int>(tmp.Item1, tmp.Item2 - 1);
            TmpBindWide -= tmp.Item1;
        }
        else if (GameManager.GM.ActionSide == ArmyBelong.Human && tmp.Item2 == 1)
        {
            SelectList[SelectedPiece.Data.PDesignation] = new Tuple<int, int>(tmp.Item1, tmp.Item2 + 1);
            TmpBindWide -= 2;
        }
        MountShow.text = SelectList[SelectedPiece.Data.PDesignation].Item2.ToString();
        UpdateTextShow();
    }

    public void UpdateTextShow()
    {
        string str = "";
        if(GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            str += "预备役储备：" + GameManager.GM.PreTrainTroop;
            str += "\n当前准备序列:";
            foreach (KeyValuePair<string, Tuple<int, int>> par in SelectList)
            {
                str += "\n" + par.Key + " ——\t" + par.Value.Item2;
            }
        }
        else
        {
            str += "当前可用带宽：" + GameManager.GM.MaxCrashBandwidth;
            str += "\n当前已用带宽：" + GameManager.GM.CrashBandwidth;
            str += "\n剩余：" + (avabileWide - TmpBindWide);
            str += "\n当前准备序列:";
            foreach (KeyValuePair<string, Tuple<int, int>> par in SelectList)
            {
                str += "\n" + par.Key + " ——\t" + par.Value.Item2;
            }
        }
        turnShow.text = str;
    }

    public void ClearCells()
    {
        for (int i = 0; i < ContentPannel.childCount; i++)
        {
            GameObject.Destroy(ContentPannel.GetChild(i).gameObject);
        }
    }
}
