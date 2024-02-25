using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIStrectgy : MonoBehaviour , IUIHandler
{
    [SerializeField]
    GameObject HumanPannel;
    [SerializeField]
    GameObject CrashPannel;
    [SerializeField]
    GameObject RenforceWindow;
    [SerializeField]
    GameObject EventSelectWindow;
    [SerializeField]
    GameObject ActiveWindow;
    [SerializeField]
    GameObject ActiveConformBtn;
    [SerializeField]
    GameObject RecoverWindow;
    [SerializeField]
    GameObject RecoverConformBtn;

    int maxActiveAmount = 0;
    int usedActiveAmount = 0;
    List<FacilityDataCell> SpecialFacList;
    string Facname;


    bool startListenAmount = false;


    public void OnEnable()
    {
        FixGameData.FGD.uiManager.actUI = this;
        if (GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            maxActiveAmount = GameManager.GM.MaxActiveStableNodeCount;
            Facname = "StaticBarrierNode";
        }
        else 
        {
            maxActiveAmount = GameManager.GM.MaxActiveFissureCount;
            Facname = "DimensionFissure";
        }

        SpecialFacList = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == Facname).ToList();
        usedActiveAmount = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == Facname && x.active).Count();

        RecoverConformBtn.SetActive(true);
        UpdateShow();
    }

    private void Update()
    {
        if (startListenAmount)
        {
            usedActiveAmount = FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == Facname && x.active).Count();
            ActiveWindow.transform.GetChild(1).GetComponent<TMP_Text>().text = "本回合可用："+maxActiveAmount+
                "\t已用：" + usedActiveAmount;
            if (usedActiveAmount <= maxActiveAmount) ActiveConformBtn.SetActive(true);
            else ActiveConformBtn.SetActive(false);
        }
    }
    public void OnPieceSelect(bool isFriendly)
    {
        if (isFriendly)
        {
            RecoverConformBtn.SetActive(true);
            RecoverWindow.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "剩余预备役：" + GameManager.GM.PreTrainTroop;
        }
        else
        {
            RecoverConformBtn.SetActive(false);
            RecoverWindow.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "请选择一个减员的友方单位";
        }
    }

    public void OnTerrainSelect(bool isFac) 
    {
        RecoverConformBtn.SetActive(false);
        RecoverWindow.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "请选择一个减员的友方单位";
    }

    public void UpdateShow()
    {
        CrashPannel.SetActive(false);
        HumanPannel.SetActive(false);
        if (GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            HumanPannel.SetActive(true);
        }
        else
        {
            CrashPannel.SetActive(true);
        }
    }

    public string WhatShouldIDo()
    {
        return "你需要在本回合确定回合末的援军、激活特殊设施以及回合策略。";
    }

    public void CrashRenforcs()
    {
        RenforceWindow.SetActive(true);
    }

    public void EventSelect()
    {
        EventSelectWindow.SetActive(true);
    }

    public void ActiveSpecialFac()
    {
        GameManager.GM.SetMachineState(MachineState.ActiveSpecialFac);
        CrashPannel.SetActive(false);
        HumanPannel.SetActive(false);
        ActiveWindow.SetActive(true);
        startListenAmount = true;

        foreach(FacilityDataCell dta in SpecialFacList)
        {
            FixGameData.FGD.ZoneMap.SetTile(dta.Positian, FixGameData.FGD.MoveArea);
        }

    }

    public void ConformActive()
    {
        GameManager.GM.SetMachineState(MachineState.Idel);
        ActiveWindow.SetActive(false);
        startListenAmount = false;
        FixGameData.FGD.ZoneMap.ClearAllTiles();
        UpdateShow();
    }

    public void RecoverTroop()
    {
        CrashPannel.SetActive(false);
        HumanPannel.SetActive(false);
        RecoverWindow.SetActive(true);

        OnPieceSelect(false);

        GameManager.GM.SetMachineState(MachineState.RecoverTroop);

    }

    public void RecoverPiece()
    {
        if (GameManager.GM.PreTrainTroop <= 0 || GameManager.GM.currentPiece == null) 
        {
            RecoverConformBtn.SetActive(false);
            return;
        }
        bool tmp = GameManager.GM.currentPiece.Recover();
        if (tmp) GameManager.GM.PreTrainTroop--;
        RecoverWindow.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "剩余预备役：" + GameManager.GM.PreTrainTroop;
        FixGameData.FGD.uiIndex.scrollView.UpdateCellChilds();
    }

    public void FinishRecover()
    {
        GameManager.GM.SetMachineState(MachineState.Idel);
        RecoverWindow.SetActive(false);
        UpdateShow();
    }
}
