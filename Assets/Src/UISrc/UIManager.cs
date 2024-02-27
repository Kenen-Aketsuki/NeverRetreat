using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region//测试用UI
    public void AddPiece()
    {
        //BasicUtility.SpawnPiece("Crash.Incite",
        //    new Vector3Int(0, 0, 0), 
        //    null, 
        //    true);
        //Debug.Log("增加了一个棋子");

        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().UpdateChildPos("1\\DawIII.101", new Vector3Int(0, 0, 0));
        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().getChildByID("1\\DawIII.101").transform.position = new Vector3Int(0, 0, 0);
        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().DelChildByID("1\\DawIII.101");
        Debug.Log("移动了一个棋子");

        List<Tuple<string, int, int>> clist = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList;
        Dictionary<int, Tuple<int, int>> listIndex = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex;
        Debug.Log(clist.Count);
        foreach (KeyValuePair<int, Tuple<int, int>> piece in listIndex)
        {
            Debug.Log(piece.Key + "的长度为: " + piece.Value.Item2 + "起始坐标为: " + piece.Value.Item1);
        }

        List<GameObject> lst = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().getChildByPos(new Vector3Int(0, 0, 0));
        if (lst.Count != 0)
        {
            Debug.Log("在该处找到棋子,其名为：" + lst[0].name);
        }
        else
        {
            Debug.Log("我那么大一个棋子呢？");
        }
        
    }

    public void MovePiece()
    {
        if (GameManager.GM.ActionSide == ArmyBelong.Human) GameManager.GM.currentPiece = FixGameData.FGD.HumanPiecePool.getChildByID("1\\DawIII.101").GetComponent<OB_Piece>();
        else GameManager.GM.currentPiece = FixGameData.FGD.HumanPiecePool.getChildByID("1\\Crash.Blade").GetComponent<OB_Piece>();

        Map.UpdateZOC();
        GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
    }

    public void TestFunc1()
    {
        GameManager.GM.currentPiece.OverTurn();

    }

    public void TestFunc2()
    {
        
    }

    public void TestFunc3()
    {
        
    }
    #endregion
    //UI管理
    //public UIIndex index;
    //当前阶段激活的UI
    public MonoBehaviour actUI;

    public void UI_SwitchStage()
    {
        if (GameManager.GM.GetMachineState() == MachineState.FocusOnPiece ||
            GameManager.GM.GetMachineState() == MachineState.FocusOnTerrain ||
            GameManager.GM.GetMachineState() == MachineState.WaitForcuse ||
            GameManager.GM.GetMachineState() == MachineState.Idel)
        {
            GameManager.GM.NextStage();
            FixGameData.FGD.uiIndex.TurnShowText.text = GameManager.GM.CurrentTurnCount + " / " + FixGameData.FGD.MaxRoundCount;
        }
        
    }

    public void WhatToDo(Transform trs)
    {
        trs.gameObject.SetActive(!trs.gameObject.activeInHierarchy);
        if (trs.gameObject.activeInHierarchy)
        {
            trs.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = (FixGameData.FGD.uiManager.actUI as IUIHandler).WhatShouldIDo();
        }
    }

    public void GameStart(Transform trs)
    {
        if(GameManager.GM.GetMachineState() == MachineState.JustReady)
        {
            GameManager.GM.StageStart(false);
            trs.gameObject.SetActive(false);
        }
    }
}
