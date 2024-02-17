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
        //开关
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
        return "本阶段人类方全体获得1次移动的机会，但不能进行战斗。";
    }

    public void OnMoveClick()
    {
        Debug.Log(GameManager.GM.currentPiece.name + "想要冲刺，冲刺，冲！");
        GameManager.GM.currentPiece.PrepareMove();
        GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
    }
}
