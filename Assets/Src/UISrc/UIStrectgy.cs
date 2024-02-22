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
    public void OnPieceSelect(bool isFriendly) { }

    public void OnTerrainSelect(bool isFac) { }

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
}
