using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class UIModBattle : MonoBehaviour , IUIHandler,IAirStrick
{
    [SerializeField]
    GameObject PieceSet;
    [SerializeField]
    GameObject TerrainSet;

    [SerializeField]
    GameObject EaseException;
    [SerializeField]
    GameObject AnnihilateException;
    [SerializeField]
    GameObject Hacker;
    [SerializeField]
    GameObject TextHint;

    [SerializeField]
    GameObject PosSelectView;

    GameObject nowActive = null;
    string currentAttack = "";

    bool needListenPieceData = false;

    private void OnEnable()
    {
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        FixGameData.FGD.uiManager.actUI = this;
        PieceSet.SetActive(false);
        TerrainSet.SetActive(false);
        TextHint.SetActive(true);

        if (nowActive != null)
        {
            nowActive.SetActive(false);
            nowActive = null;
        }
    }

    private void Update()
    {
        if (needListenPieceData)
        {
            PosSelectView.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "当前可用攻击次数:" + GameManager.GM.currentPiece.SpecialActPoint;
        }
    }

    public void OnPieceSelect(bool isFriendly)
    {
        TextHint.SetActive(false);
        UpdateShow();

        TerrainSet.SetActive(false);
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();

        if (isFriendly && GameManager.GM.currentPiece.getPieceData().canFixMod)
        {
            PieceSet.SetActive(true);
        }else PieceSet.SetActive(false);
    }

    public void OnTerrainSelect(bool isFac)
    {
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        TextHint.SetActive(false);
        PieceSet.SetActive(false);
        TerrainSet.SetActive(true);
    }

    public void OnPositionSelect(Vector3Int Pos)
    {
        if (FixGameData.FGD.MoveAreaMap.GetTile(Pos) == null &&
            FixGameData.FGD.AttackAreaMap.GetTile(Pos) != null)
        {
            ModBattle.CommitAttack(currentAttack, Pos);
            GameManager.GM.currentPiece.SpecialActPoint--;
            if (GameManager.GM.currentPiece.SpecialActPoint == 0) StopPosSelect();
        }
    }

    public void UpdateShow()
    {
        if(GameManager.GM.currentPiece != null)
        {
            PieceSet.transform.GetChild(0).gameObject.SetActive(GameManager.GM.currentPiece.canMove);
            PieceSet.transform.GetChild(1).gameObject.SetActive(GameManager.GM.currentPiece.ActionPoint > 0);
        }
        else
        {
            PieceSet.transform.GetChild(0).gameObject.SetActive(false);
            PieceSet.transform.GetChild(1).gameObject.SetActive(false);
        }
        
        if(nowActive != null)
        {
            nowActive.SetActive(false);
            nowActive = null;
        }
    }

    public string WhatShouldIDo()
    {
        string str = "本阶段仅双方模组战单位允许移动与发动模组战。模组战是针对棋子安定度发起的打击，能够大幅度影响敌方单位的战斗力。";

        return str;
    }

    //棋子移动
    public void OnMoveClick()
    {
        GameManager.GM.currentPiece.PrepareMove();
        if (GameManager.GM.MoveArea != null && GameManager.GM.MoveArea.Count > 1) GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
    }

    public void PrepareModAttack()
    {
        PieceSet.SetActive(false);
        Hacker.SetActive(false);
        AnnihilateException.SetActive(false);
        EaseException.SetActive(false);


        string TroopFunc = GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2];

        switch (TroopFunc)
        {
            case "Ease-Exception":
                EaseException.SetActive(true);
                nowActive = EaseException;
                break;
            case "Annihilate-Exception":
                AnnihilateException.SetActive(true);
                nowActive = AnnihilateException;
                break;
            case "Hacker":
                Hacker.SetActive(true);
                nowActive = Hacker;
                break;
            default:
                Debug.Log("未知兵种：" + TroopFunc);
                break;
        }

        //设置不可选择区域
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        for (int i = 0; i < GameManager.GM.EnemyPool.transform.childCount; i++)
        {
            OB_Piece tarP = GameManager.GM.EnemyPool.transform.GetChild(i).GetComponent<OB_Piece>();
            
            if (tarP.getPieceData().canFixMod)
            {
                Map.SetArea(tarP.piecePosition, tarP.getPieceData().passiveArea,FixGameData.FGD.MoveAreaMap, FixSystemData.GlobalZoneList["ZOC"].Top,false);
                //Map.SetArea(tarP.piecePosition, tarP.getPieceData().passiveArea);
            }
        }
    }

    public void DeSelect()
    {
        nowActive.SetActive(false);
        nowActive = null;
        PieceSet.SetActive(true);
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
    }

    public void DoModAttck(string name)
    {
        if (GameManager.GM.currentPiece.SpecialActPoint <= 0)
        {
            FixGameData.FGD.uiIndex.HintUI.SetText("行动点不足");
            FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
            return;
        }

        currentAttack = name;

        //选定坐标
        nowActive.SetActive(false);
        PosSelectView.SetActive(true);
        FixGameData.FGD.uiIndex.HintUI.SetText("请选择打击位置");
        FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
        needListenPieceData = true;

        ModBattle.PrepareAttack(name);

        GameManager.GM.SetMachineState(MachineState.SelectEventPosition);
        GameManager.GM.CanMachineStateChange = false;
    }

    public void PrepareAirStrick()
    {
        FixGameData.FGD.uiIndex.AirStrickWindow.GetComponent<UIAirStrickSelect>().SetData(this, true);
        FixGameData.FGD.uiIndex.AirStrickWindow.SetActive(true);
    }

    public void AirStrickCall(Dictionary<Piece, int> FriendList, Dictionary<Piece, int> EnemyList, int GroundDefence)
    {
        ModBattle.ModBattleAirStrick(FriendList, EnemyList, GroundDefence);
    }

    public void StopPosSelect()
    {
        nowActive.SetActive(true);
        PosSelectView.SetActive(false);
        needListenPieceData = false;
        FixGameData.FGD.uiIndex.HintUI.gameObject.SetActive(false);
        FixGameData.FGD.AttackAreaMap.ClearAllTiles();

        GameManager.GM.CanMachineStateChange = true;
        GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
    }
}
