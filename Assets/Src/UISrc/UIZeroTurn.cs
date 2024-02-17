using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIZeroTurn : MonoBehaviour , IUIHandler
{
    private void OnEnable()
    {
        FixGameData.FGD.uiManager.actUI = this;
    }

    public void OnPieceSelect(bool isFriendly)
    {
        //����
        if(isFriendly && GameManager.GM.currentPiece != null) transform.GetChild(0).gameObject.SetActive(true);
        else transform.GetChild(0).gameObject.SetActive(false);
    }

    public void OnTerrainSelect(bool isFac)
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void UpdateShow()
    {
        if(GameManager.GM.GetMachineState() == MachineState.FocusOnPiece)
        {
            if (GameManager.GM.currentPiece.getPieceData().MOV <= 0) transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public string WhatShouldIDo()
    {
        return "���׶����෽ȫ����1���ƶ��Ļ��ᣬ�����ܽ���ս����";
    }

    public void OnMoveClick()
    {
        Debug.Log(GameManager.GM.currentPiece.name + "��Ҫ��̣���̣��壡");
        GameManager.GM.currentPiece.PrepareMove();
        GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
    }
}
