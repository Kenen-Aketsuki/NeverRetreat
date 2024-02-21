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

    bool ReinforceCheck = false; //ȷ����Ԯ
    bool RecoverCheck = false; //���ӻָ�
    bool ActiveCheck = false; //����������ʩ
    bool EventCheck = false; //ȷ�������¼�

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
        return "����Ҫ�ڱ��غ�";
    }

    public void CrashRenforcs()
    {
        RenforceWindow.SetActive(true);
    }
}
