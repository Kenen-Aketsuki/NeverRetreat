using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    //������
    //�ж������ӳ�
    public PiecePool ActionPool;
    //�з����ӳ�
    public PiecePool EnemyPool;

    //���ӿ��ƶ�����
    public Dictionary<Vector3Int, CellInfo> MoveArea;
    //��ǰ�ж�������
    public OB_Piece currentPiece;
    //��ǰ��ע��λ��
    public Vector3Int currentPosition;

    //��Ϸ����
    //����״̬����ǰ״̬
    [SerializeField]
    MachineState machineState;
    //��ǰ�ж���
    public ArmyBelong ActionSide;
    //��ǰ�غ���
    public int CurrentTurnCount = 0;
    //��ǰ�غϽ׶�
    public TurnStage Stage;

    //���෽--���Ա��
    [SerializeField]
    public float MaxMobilizationRate;
    //���෽--��Ա��
    [SerializeField]
    public float MobilizationRate;
    //���෽��Ԥ��������
    [SerializeField]
    public int PreTrainTroop;
    //���෽���ɼ�����ڵ�����
    [SerializeField]
    public int MaxActiveStableNodeCount;
    //������־ �� �¼��б�
    [SerializeField]
    public List<SpecialEvent> HumanEventList = new List<SpecialEvent>();

    //������־--��������
    [SerializeField]
    public int MaxCrashBandwidth;
    //������־--����
    [SerializeField]
    public int CrashBandwidth;
    //������־���ɼ�����϶��
    [SerializeField]
    public int MaxActiveFissureCount;
    //������־ �� �¼��б�
    [SerializeField]
    public List<SpecialEvent> CrashEventList = new List<SpecialEvent>();

    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }
    

    //���û���״̬
    public void SetMachineState(MachineState state){machineState = state;}
    //��ȡ����״̬
    public MachineState GetMachineState(){return machineState;}

    public int stageMode = 5;
    public void NextStage()//������һ�׶�
    {
        bool NextTurn = false;
        StageEnd();
        if (ActionSide == ArmyBelong.Human)
        {
            Stage = (TurnStage)(((int)Stage + 1) % stageMode);
            ActionSide = ArmyBelong.ModCrash;
            ActionPool = FixGameData.FGD.CrashPiecePool;
            EnemyPool = FixGameData.FGD.HumanPiecePool;
            NextTurn = true;
        }
        else
        {
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
        }

        currentPiece = null;
        currentPosition = new Vector3Int(114, 514);
        machineState = MachineState.Idel;
        FixGameData.FGD.uiIndex.scrollView.ClearCells();
    }

    public void StageEnd()
    {
        switch (Stage)
        {
            case TurnStage.Strategy:
                StrategyStageEnd();
                break;
            case TurnStage.ZeroTurn:
                ZeroTurnStageEnd();
                break;

        }
    }
    //���ػغ���Ϣ
    public void LoadTurnData(TurnData data)
    {
        MaxMobilizationRate = 50;
        if(data == null)
        {
            //Ϊ�գ�����Ĭ�ϴӵ���غϿ�ʼ
            MaxActiveFissureCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveBarrierAmmount;
            MaxCrashBandwidth = FixGameData.FGD.TurnDatas[CurrentTurnCount].CrashBundith;
            Stage = FixGameData.FGD.TurnDatas[CurrentTurnCount].StartStage;
            if (FixGameData.FGD.TurnDatas[CurrentTurnCount].PreTrainedAmount > 0) PreTrainTroop = FixGameData.FGD.TurnDatas[CurrentTurnCount].PreTrainedAmount;
            if (FixGameData.FGD.TurnDatas[CurrentTurnCount].MobilizationRate > 0) MobilizationRate = FixGameData.FGD.TurnDatas[CurrentTurnCount].MobilizationRate;
        }
        else
        {
            //�ǿգ�ͬ���浵
            MobilizationRate = data.MobilizationRate;
            MaxActiveFissureCount = data.MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = data.MaxActiveBarrierAmmount;
            MaxCrashBandwidth = data.CrashBundith;
            Stage = data.StartStage;
            PreTrainTroop = data.PreTrainedAmount;

            CurrentTurnCount = data.TurnNo;
        }
    }
    //�л��غ�
    void TurnSwitch()
    {
        CurrentTurnCount++;
        //���ض�Ӧ�غϵ���Ϣ
        LoadTurnData(null);
        //������������
        for(int i = 0; i < FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().OverTurn();
        }
        for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
        {
            FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>().OverTurn();
        }
    }

    //�����׶���ֹ
    #region//����׶�
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

    #region//���Խ׶�
    void StrategyStageStart()
    {
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(true);


    }
    void StrategyStageEnd()
    {
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(false);
    }
    #endregion
}

//����״̬��״̬
public enum MachineState
{
    NotReadyYet,
    JustReady,
    Idel,
    WaitForcuse,
    FocusOnPiece,
    FocusOnTerrain,
    WaitMoveTarget,
    ActiveSpecialFac
}
//�غϽ׶�
public enum TurnStage
{
    Strategy,//���Խ׶�
    ModBattle,//ģ��ս�׶�
    Action,//�ж��׶�
    Support,//��Ԯ�׶�
    Settle,//����׶�
    ZeroTurn//��һ�غ������ж������෽ȫ��һ���ƶ�����
}

public enum SpecialEvent
{
    //����
    DataStrom,
    SpaceSplit,
    SpaceFix,
    PosConfuse,
    //����
    MentalAD,
    TrainTroop,
    RetreatCiv
}