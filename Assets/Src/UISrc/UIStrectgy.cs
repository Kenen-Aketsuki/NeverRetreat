using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStrectgy : MonoBehaviour , IUIHandler
{
    [SerializeField]
    GameObject HumanPannel;
    [SerializeField]
    GameObject CrashPannel;
    [SerializeField]
    GameObject RenforceWindow;

    bool ReinforceCheck = false; //确定增援
    bool RecoverCheck = false; //部队恢复
    bool ActiveCheck = false; //激活特殊设施
    bool EventCheck = false; //确定特殊事件

    void OnEnable()
    {
        FixGameData.FGD.uiManager.actUI = this;
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
        return "你需要在本回合";
    }

    public void CrashRenforcs()
    {
        RenforceWindow.SetActive(true);
    }
}
