using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class UIActionStage : MonoBehaviour , IUIHandler , IAirStrick
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
    [SerializeField]
    GameObject AttackSelectView;

    [SerializeField]
    List<Vector3Int> AttackPosList = new List<Vector3Int>();
    Vector3Int AttackTarPos;
    [SerializeField]
    float ATK;
    [SerializeField]
    float DEF;
    [SerializeField]
    int RRKMend;

    GameObject currentActive;

    string waitPosTar;
    string currentSpell;

    bool needListen = false;
    bool needCloseSpellList = false;

    // Update is called once per frame
    public void Update()
    {
        if (needListen && waitPosTar != "Attack")
        {
            PosSelectView.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "可用攻击次数: " + GameManager.GM.currentPiece.SpecialActPoint;
        }else if(needListen && waitPosTar == "Attack")
        {
            AttackSelectView.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = 
                "攻守战力比: " + ATK +"/" + DEF + 
                " 战果评级: " + FixSystemData.battleJudgeForm.GetRRK(ATK,DEF) + RRKMend.ToString();
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
                else
                {
                    FixGameData.FGD.uiIndex.HintUI.SetText("弹药用尽");
                    FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
                }
                if (GameManager.GM.currentPiece.SpecialActPoint <= 0) StopPosSelect();
                break;
            case "Spell":
                if (FixGameData.FGD.AttackAreaMap.GetTile(Pos) != null)
                {
                    ActionStage.CastSpell(currentSpell, Pos);
                    GameManager.GM.currentPiece.SpecialActPoint--;
                }
                if (GameManager.GM.currentPiece.SpecialActPoint <= 0) StopPosSelect();
                break;
            case "Attack":
                int addr = AttackPosList.FindIndex(x => x == Pos);
                if(addr != -1)
                {
                    AttackPosList.RemoveAt(addr);
                }
                else if(Map.HexDistence(Pos,AttackTarPos) == 1 && GameManager.GM.ActionPool.getChildByPos(Pos).Count != 0)
                {
                    AttackPosList.Add(Pos);
                }

                ActionStage.CulATK(ref ATK, ref DEF, AttackPosList, AttackTarPos);
                Map.SetArea(Map.PowerfulBrickAreaSearch(AttackTarPos, 1), FixGameData.FGD.AttackAreaMap, FixGameData.FGD.MoveArea, true);
                Map.SetArea(AttackPosList, FixGameData.FGD.AttackAreaMap, FixSystemData.GlobalZoneList["ZOC"].Top, false);

                break;
        }
        //waitPosTar = "";
        

    }

    public void UpdateShow()
    {
        if (waitPosTar == "Attack") return;

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
                currentActive.transform.GetChild(0).gameObject.SetActive(true);
                if (GameManager.GM.ActionSide == ArmyBelong.ModCrash) currentActive.transform.GetChild(1).gameObject.SetActive(FixGameData.FGD.SupportDic().Where(x => x.Value.Item2 != 0).Count() != 0 && FixGameData.FGD.ZoneMap.GetTile(GameManager.GM.currentPosition) == null);
                else currentActive.transform.GetChild(1).gameObject.SetActive(FixGameData.FGD.SupportDic().Where(x => x.Value.Item2 != 0).Count() != 0);

                break;
            case "Terrain":

                break;
        }
    }

    public string WhatShouldIDo()
    {
        return "这是最重要的阶段，你需要穷尽各种手段组织有效的进攻或防御来达到你的战术目标。";
    }

    public void AirStrickCall(Dictionary<Piece, int> FriendList, Dictionary<Piece, int> EnemyList, int GroundDefence)
    {
        ActionStage.CommitAirStrick(FriendList, EnemyList, GroundDefence);
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
        //Map.SetArea(GameManager.GM.currentPosition,
        //    GameManager.GM.currentPiece.getPieceData().activeArea,
        //    FixGameData.FGD.AttackAreaMap,
        //    FixGameData.FGD.MoveArea,
        //    true);
        List<CellInfo> area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, GameManager.GM.currentPiece.getPieceData().activeArea).Where(x => ActionStage.canHit(x.Positian,Vector3Int.zero)).ToList();
        Map.SetArea(area, FixGameData.FGD.AttackAreaMap, FixGameData.FGD.MoveZocArea, true);

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

    public void PrepareAttack()
    {
        waitPosTar = "Attack";
        needListen = true;

        currentActive.SetActive(false);
        AttackSelectView.SetActive(true);

        GameManager.GM.SetMachineState(MachineState.SelectEventPosition);
        GameManager.GM.CanMachineStateChange = false;

        AttackTarPos = GameManager.GM.currentPosition;
        AttackPosList.Clear();
        
        Map.SetArea(Map.PowerfulBrickAreaSearch(AttackTarPos, 1), FixGameData.FGD.AttackAreaMap, FixGameData.FGD.MoveArea, true);
        //Map.SetArea(AttackPosList, FixGameData.FGD.AttackAreaMap, FixSystemData.GlobalZoneList["ZOC"].Top, false);
        Regex reg = new Regex(".*正常.*");
        RRKMend = 0;
        foreach (Vector3Int pos in Map.PowerfulBrickAreaSearch(AttackTarPos, FixGameData.FGD.maxFireSupportDic).Select(x => x.Positian))
        {
            int supListC = GameManager.GM.ActionPool.getChildByPos(pos)
                .Select(x => x.GetComponent<OB_Piece>().getPieceData())
                .Where(x => x.canSupport &&
                    Map.HexDistence(pos, AttackTarPos) <= x.activeArea &&
                    ActionStage.canHit(AttackTarPos, pos, x.PieceID, true)
                ).Count();
            if (supListC > 0)
            {
                RRKMend += supListC;
                continue;
            }

            supListC = GameManager.GM.EnemyPool.getChildByPos(pos)
                .Select(x => x.GetComponent<OB_Piece>().getPieceData())
                .Where(x => x.canSupport &&
                    reg.IsMatch(x.ConnectStr) &&
                    reg.IsMatch(x.SupplyStr) &&
                    reg.IsMatch(x.HealthStr) &&
                    Map.HexDistence(pos, AttackTarPos) <= x.activeArea &&
                    ActionStage.canHit(AttackTarPos, pos, x.PieceID, true)
                ).Count();
            if (supListC > 0)
            {
                RRKMend -= supListC;
            }
        }
    }

    public void StartAngriff()
    {
        if(AttackPosList.Count == 0)
        {
            StopPosSelect();
            return;
        }

        ActionStage.CommitAttack(ATK, DEF, AttackPosList, AttackTarPos,RRKMend);
        
        foreach(Vector3Int pos in AttackPosList)
        {
            foreach(OB_Piece pic in GameManager.GM.ActionPool.getChildByPos(pos).Select(x => x.GetComponent<OB_Piece>()))
            {
                pic.ActionPoint--;
            }
        }

        StopPosSelect();
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
        AttackSelectView.SetActive(false);

        FixGameData.FGD.MoveAreaMap.ClearAllTiles();
        FixGameData.FGD.AttackAreaMap.ClearAllTiles();

        
        GameManager.GM.SetMachineState(MachineState.Idel);
        waitPosTar = "";
        needListen = false;
    }

    public void PrepareAirStrick()
    {
        FixGameData.FGD.uiIndex.AirStrickWindow.GetComponent<UIAirStrickSelect>().SetData(this, false);
        FixGameData.FGD.uiIndex.AirStrickWindow.SetActive(true);
    }
}
