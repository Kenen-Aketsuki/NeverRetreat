using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIActionStage : MonoBehaviour , IUIHandler
{
    [SerializeField]
    GameObject TextHint;
    [SerializeField]
    GameObject FriendPiecePannel;
    [SerializeField]
    GameObject EnemyPiecePannel;
    [SerializeField]
    GameObject TerrainPannel;
    [SerializeField]
    GameObject PosSelectView;
    [SerializeField]
    GameObject SpellList;


    GameObject currentActive;

    string waitPosTar;
    string currentSpell;

    bool needListen = false;
    bool needCloseSpellList = false;

    // Update is called once per frame
    public void Update()
    {
        if (needListen)
        {
            PosSelectView.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "可用攻击次数: " + GameManager.GM.currentPiece.SpecialActPoint;
        }
    }

    public void OnEnable()
    {
        FixGameData.FGD.uiManager.actUI = this;

        currentActive = null;
        TextHint.SetActive(true);
        FriendPiecePannel.SetActive(false);
        EnemyPiecePannel.SetActive(false);
        TerrainPannel.SetActive(false);
    }

    public void OnPieceSelect(bool isFriend)
    {
        TextHint.SetActive(false);
        FriendPiecePannel.SetActive(isFriend);
        EnemyPiecePannel.SetActive(!isFriend);
        TerrainPannel.SetActive(false);
        currentActive = isFriend ? FriendPiecePannel : EnemyPiecePannel;

        UpdateShow();
    }

    public void OnTerrainSelect(bool isFac)
    {
        TextHint.SetActive(false);
        FriendPiecePannel.SetActive(false);
        EnemyPiecePannel.SetActive(false);
        TerrainPannel.SetActive(true);
        currentActive = TerrainPannel;

        UpdateShow();
    }

    public void OnPositionSelect(Vector3Int Pos)
    {
        switch (waitPosTar)
        {
            case "Metro":
            case "Move":
                currentActive.SetActive(true);
                PosSelectView.SetActive(false);
                break;
            case "Strick":
                if(ActionStage.CommitFireStrick(Pos)) GameManager.GM.currentPiece.SpecialActPoint -= 2;
                if(GameManager.GM.currentPiece.SpecialActPoint <= 0) StopPosSelect();
                break;
            case "Spell":
                if (FixGameData.FGD.AttackAreaMap.GetTile(Pos) != null) ActionStage.CastSpell(currentSpell, Pos);
                break;
        }
        //waitPosTar = "";
        

    }

    public void UpdateShow()
    {
        switch (currentActive.name)
        {
            case "FriendPiece":
                currentActive.transform.GetChild(0).gameObject.SetActive(GameManager.GM.currentPiece.canMove);
                currentActive.transform.GetChild(1).gameObject.SetActive(FixGameData.FGD.FacilityList.Where(x=>x.Positian == GameManager.GM.currentPosition && x.Id == "MetroStation").Count() != 0 && GameManager.GM.ActionSide == ArmyBelong.Human);
                currentActive.transform.GetChild(2).gameObject.SetActive(GameManager.GM.currentPiece.getPieceData().canStrike);
                currentActive.transform.GetChild(3).gameObject.SetActive(GameManager.GM.currentPiece.getPieceData().canDoMagic);
                currentActive.transform.GetChild(4).gameObject.SetActive(GameManager.GM.currentPiece.getPieceData().canBild);
                break;
            case "EnemyPiece":

                break;
            case "Terrain":

                break;
        }
    }

    public string WhatShouldIDo()
    {
        return "这是最重要的阶段，你需要穷尽各种手段组织有效的进攻或防御来达到你的战术目标。";
    }

    public void ReadyToMove()
    {
        PosSelectView.SetActive(true);
        currentActive.SetActive(false);
        PosSelectView.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "选择移动目的地";

        GameManager.GM.currentPiece.PrepareMove();
        if (GameManager.GM.MoveArea != null && GameManager.GM.MoveArea.Count > 1)
        {
            waitPosTar = "Move";
            GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
        }
    }

    public void TakeSubway()
    {
        //获取可移动区域
        List<FacilityDataCell> Metros = FixGameData.FGD.FacilityList.Where(x => x.Id == "MetroStation" && x.Positian != GameManager.GM.currentPosition).ToList();
        if (Metros.Count == 0) return;
        waitPosTar = "Metro";

        foreach (FacilityDataCell c in Metros)
        {
            FixGameData.FGD.MoveAreaMap.SetTile(c.Positian, FixGameData.FGD.MoveArea);
        }
        PosSelectView.SetActive(true);
        currentActive.SetActive(false);
        PosSelectView.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "选择移动目的地";

        GameManager.GM.SetMachineState(MachineState.WaitForceMoveTarget);
    }

    public void FireStrick()
    {
        if (GameManager.GM.currentPiece.SpecialActPoint <= 0) return;
        Map.SetArea(GameManager.GM.currentPosition,
            GameManager.GM.currentPiece.getPieceData().activeArea,
            FixGameData.FGD.AttackAreaMap,
            FixGameData.FGD.MoveArea,
            true);
        waitPosTar = "Strick";
        needListen = true;

        currentActive.SetActive(false);
        PosSelectView.SetActive(true);

        GameManager.GM.SetMachineState(MachineState.SelectEventPosition);
        GameManager.GM.CanMachineStateChange = false;
    }

    public void CastSpell()
    {
        currentActive.SetActive(false);
        SpellList.SetActive(true);
        needCloseSpellList = true;
        if (GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            SpellList.transform.GetChild(1).gameObject.SetActive(true);
            SpellList.transform.GetChild(2).gameObject.SetActive(true);
            SpellList.transform.GetChild(3).gameObject.SetActive(true);
            SpellList.transform.GetChild(4).gameObject.SetActive(true);
            SpellList.transform.GetChild(5).gameObject.SetActive(false);
            SpellList.transform.GetChild(6).gameObject.SetActive(false);
            SpellList.transform.GetChild(7).gameObject.SetActive(false);
            SpellList.transform.GetChild(8).gameObject.SetActive(false);
        }
        else
        {
            SpellList.transform.GetChild(1).gameObject.SetActive(false);
            SpellList.transform.GetChild(2).gameObject.SetActive(false);
            SpellList.transform.GetChild(3).gameObject.SetActive(false);
            SpellList.transform.GetChild(4).gameObject.SetActive(false);
            SpellList.transform.GetChild(5).gameObject.SetActive(true);
            SpellList.transform.GetChild(6).gameObject.SetActive(true);
            SpellList.transform.GetChild(7).gameObject.SetActive(true);
            SpellList.transform.GetChild(8).gameObject.SetActive(true);
        }
    }

    public void SpellSelect(string SpellNam)
    {
        ActionStage.PrepareSpell(SpellNam);

        currentSpell = SpellNam;
        waitPosTar = "Spell";
        needListen = true;

        SpellList.SetActive(false);
        PosSelectView.SetActive(true);

        GameManager.GM.SetMachineState(MachineState.SelectEventPosition);
        GameManager.GM.CanMachineStateChange = false;
    }

    public void StopPosSelect()
    {
        if (needCloseSpellList)
        {
            SpellList.SetActive(true);
            needCloseSpellList = false;
            GameManager.GM.CanMachineStateChange = false;
        }
        else
        {
            SpellList.SetActive(false);
            currentActive.SetActive(true);
            GameManager.GM.CanMachineStateChange = true;
        }
        PosSelectView.SetActive(false);

        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        FixGameData.FGD.AttackAreaMap.ClearAllTiles();

        
        GameManager.GM.SetMachineState(MachineState.Idel);
        waitPosTar = "";
        needListen = false;
    }
}
