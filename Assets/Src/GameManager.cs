using System.Collections;
using System.Collections.Generic;
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

    //游戏管理
    //有限状态机当前状态
    [SerializeField]
    MachineState machineState;
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
    public float MobilizationRate;
    //人类方—可激活安定节点数量
    [SerializeField]
    public int MaxActiveStableNodeCount;
    //崩坏意志--带宽上限
    [SerializeField]
    public int MaxCrashBandwidth;
    //崩坏意志--带宽
    [SerializeField]
    public int CrashBandwidth;
    //崩坏意志—可激活裂隙数
    [SerializeField]
    public int MaxActiveFissureCount;


    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }
    //设置机器状态
    public void SetMachineState(MachineState state){machineState = state;}
    //获取机器状态
    public MachineState GetMachineState(){return machineState;}

    public int stageMode = 5;
    public void NextStage()//进入下一阶段
    {
        StageEnd();
        int tmp = (int)Stage;
        tmp++;
        tmp = tmp % stageMode;
        Stage = (TurnStage)tmp;
        //Stage = (TurnStage)(((int)Stage + 1) % stageMode);
        StageStart();
    }

    public void StageStart()
    {
        Map.UpdateCrashBindwith();
        switch (Stage)
        {
            case 0:
                stageMode = 5;
                TurnSwitch();
                StrategyStageStart();
                break;
            case TurnStage.ZeroTurn:
                stageMode = 6;


                break;
        }
    }

    public void StageEnd()
    {
        switch (Stage)
        {
            case TurnStage.Strategy:
                StrategyStageEnd();
                break;
            case TurnStage.ZeroTurn:

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
            MobilizationRate = 50;
            MaxActiveFissureCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveBarrierAmmount;
            MaxCrashBandwidth = FixGameData.FGD.TurnDatas[CurrentTurnCount].CrashBundith;
            Stage = FixGameData.FGD.TurnDatas[CurrentTurnCount].StartStage;
        }
        else
        {
            //非空，同步存档
            MobilizationRate = data.MobilizationRate;
            MaxActiveFissureCount = data.MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = data.MaxActiveBarrierAmmount;
            MaxCrashBandwidth = data.CrashBundith;
            Stage = data.StartStage;

            CurrentTurnCount = data.TurnNo;
        }
    }
    //切换回合
    void TurnSwitch()
    {
        CurrentTurnCount++;
        //加载对应回合的信息
        LoadTurnData(null);
    }

    //各个阶段起止
    #region//策略阶段
    void StrategyStageStart()
    {

    }
    void StrategyStageEnd()
    {

    }
    #endregion
}

//有限状态机状态
public enum MachineState
{
    Idel,
    FocusOnPiece,
    WaitMoveTarget
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