using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    //数据区
    //行动方棋子池
    public PiecePool ActionPool;
    //敌方棋子池
    public PiecePool EnemyPool;

    //棋子可移动区域
    public Dictionary<Vector3Int, CellInfo> MoveArea;
    //当前行动的棋子
    public OB_Piece currentPiece;
    //当前关注的位置
    public Vector3Int currentPosition;

    //游戏管理
    //有限状态机当前状态
    [SerializeField]
    MachineState machineState;
    //能否更改机器状态
    public bool CanMachineStateChange = true;
    //当前行动方
    public ArmyBelong ActionSide;
    //当前回合数
    public int CurrentTurnCount = 0;
    //当前回合阶段
    public TurnStage Stage;


    //人类方--最大动员率
    [SerializeField]
    public float MaxMobilizationRate;
    //人类方--动员率
    [SerializeField]
    public int MobilizationRate;
    //人类方—预备役数量
    [SerializeField]
    public int PreTrainTroop;
    //人类方—可激活安定节点数量
    [SerializeField]
    public int MaxActiveStableNodeCount;
    //崩坏意志 — 事件列表
    public List<Tuple<SpecialEvent, Vector3Int>> HumanEventList = new List<Tuple<SpecialEvent, Vector3Int>>();


    //崩坏意志--带宽上限
    [SerializeField]
    public int MaxCrashBandwidth;
    //崩坏意志--带宽
    [SerializeField]
    public int CrashBandwidth;
    //崩坏意志—可激活裂隙数
    [SerializeField]
    public int MaxActiveFissureCount;
    //崩坏意志 — 事件列表
    public List<Tuple<SpecialEvent, Vector3Int>> CrashEventList = new List<Tuple<SpecialEvent, Vector3Int>>();

    //行动目标地点
    public Vector3Int ActionTargetPos;

    void Start()
    {
        GM = this;
        ActionTargetPos = Vector3Int.zero;
    }
    

    //设置机器状态
    public void SetMachineState(MachineState state){if(CanMachineStateChange) machineState = state;}
    //获取机器状态
    public MachineState GetMachineState(){return machineState;}

    int stageMode = 5;
    public void NextStage()//进入下一阶段
    {
        bool NextTurn = false;
        if (ActionSide == ArmyBelong.Human) NextTurn = true;

        StageEnd(NextTurn);
        if (NextTurn)
        {
            //跳回合
            Stage = (TurnStage)(((int)Stage + 1) % stageMode);
            ActionSide = ArmyBelong.ModCrash;
            ActionPool = FixGameData.FGD.CrashPiecePool;
            EnemyPool = FixGameData.FGD.HumanPiecePool;
        }
        else
        {
            //跳玩家
            ActionSide = ArmyBelong.Human;
            ActionPool = FixGameData.FGD.HumanPiecePool;
            EnemyPool = FixGameData.FGD.CrashPiecePool;
        }
        
        StageStart(NextTurn);
    }

    public void StageStart(bool NextTurn )
    {
        Map.UpdateCrashBindwith();
        Map.UpdateZOC();
        FixGameData.FGD.uiIndex.turnData.UpdateInfo();
        FixGameData.FGD.AttackAreaMap.ClearAllTiles();
        FixGameData.FGD.MoveAreaMap.ClearAllTiles();

        switch (Stage)
        {
            case 0:
                stageMode = 5;
                if (NextTurn) TurnSwitch();
                StrategyStageStart();
                break;
            case TurnStage.ZeroTurn:
                stageMode = 6;
                ZeroTurnStageStart();
                break;
            case TurnStage.ModBattle:
                ModeBattleStageStart();
                break;
            case TurnStage.Action:
                ActionStageStart();
                break;
        }

        currentPiece = null;
        currentPosition = new Vector3Int(114, 514);
        machineState = MachineState.Idel;
        FixGameData.FGD.uiIndex.scrollView.ClearCells();
    }

    public void StageEnd(bool NextTurn)
    {
        switch (Stage)
        {
            case TurnStage.Strategy:
                StrategyStageEnd(NextTurn);
                break;
            case TurnStage.ZeroTurn:
                ZeroTurnStageEnd();
                break;
            case TurnStage.ModBattle:
                ModeBattleStageEnd(NextTurn);
                break;
            case TurnStage.Action:
                ActionStageEnd();
                break;

        }
    }
    //加载回合信息
    public void LoadTurnData(TurnData data)
    {
        MaxMobilizationRate = 50;
        if(data == null)
        {
            //为空，按照默认从第零回合开始
            MaxActiveFissureCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveBarrierAmmount;
            MaxCrashBandwidth = FixGameData.FGD.TurnDatas[CurrentTurnCount].CrashBundith;
            Stage = FixGameData.FGD.TurnDatas[CurrentTurnCount].StartStage;
            if (FixGameData.FGD.TurnDatas[CurrentTurnCount].PreTrainedAmount > 0) PreTrainTroop = FixGameData.FGD.TurnDatas[CurrentTurnCount].PreTrainedAmount;
            if (FixGameData.FGD.TurnDatas[CurrentTurnCount].MobilizationRate > 0) MobilizationRate = FixGameData.FGD.TurnDatas[CurrentTurnCount].MobilizationRate;
        }
        else
        {
            //非空，同步存档
            MobilizationRate = data.MobilizationRate;
            MaxActiveFissureCount = data.MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = data.MaxActiveBarrierAmmount;
            MaxCrashBandwidth = data.CrashBundith;
            Stage = data.StartStage;
            PreTrainTroop = data.PreTrainedAmount;

            CurrentTurnCount = data.TurnNo;
        }
    }
    //切换回合
    void TurnSwitch()
    {
        CurrentTurnCount++;
        //加载对应回合的信息
        LoadTurnData(null);
        //重置所有棋子
        for(int i = 0; i < FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().OverTurn();
        }
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>().OverTurn();
        }
    }

    //各个阶段起止
    #region//第零阶段
    void ZeroTurnStageStart()
    {
        FixGameData.FGD.uiIndex.TurnZeroUISet.SetActive(true);
        for(int i=0;i< FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().ResetMov();
        }
    }

    void ZeroTurnStageEnd()
    {
        FixGameData.FGD.uiIndex.TurnZeroUISet.SetActive(false);
    }
    #endregion

    #region//策略阶段
    void StrategyStageStart()
    {
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(true);
        Streagy.GetAirForce();
    }
    void StrategyStageEnd(bool isTurnChange)
    {
        if (isTurnChange)
        {
            //展开结界
            Map.UpdateStaticBarrier();
            //造成伤害
            for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
            {
                var tmp = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
                if (FixGameData.FGD.MapList[14].GetTile(tmp.piecePosition) != null)
                {
                    tmp.TakeDemage(1);
                }
            }
            //结算事件
            Streagy.CulEvent();
            //更改裂隙状态
            foreach (FacilityDataCell fac in FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure"))
            {
                if (FixGameData.FGD.MapList[14].GetTile(fac.Positian) != null)
                {
                    fac.SetActive(false);
                }
            }

        }
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(false);
        Map.UpdateCrashBindwith();
    }
    #endregion

    #region //模组战阶段
    void ModeBattleStageStart()
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(true);
    }

    void ModeBattleStageEnd(bool isTurnChange)
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(false);
    }

    #endregion

    #region//行动阶段
    void ActionStageStart()
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(true);
    }

    void ActionStageEnd()
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(false);
    }

    #endregion
}

//有限状态机状态
public enum MachineState
{
    TestOnly,
    NotReadyYet,
    JustReady,
    Idel,
    WaitForcuse,
    FocusOnPiece,
    FocusOnTerrain,
    WaitMoveTarget,
    WaitForceMoveTarget,
    ActiveSpecialFac,
    RecoverTroop,
    SelectEventPosition,
    SelectEnemyPiece
}
//回合阶段
public enum TurnStage
{
    Strategy,//策略阶段
    ModBattle,//模组战阶段
    Action,//行动阶段
    Support,//增援阶段
    Settle,//结算阶段
    ZeroTurn//第一回合特殊行动：人类方全体一次移动机会
}

public enum SpecialEvent
{
    //崩坏
    DataStrom,
    SpaceSplit,
    SpaceFix,
    PosConfuse,
    //人类
    MentalAD,
    TrainTroop,
    RetreatCiv
}