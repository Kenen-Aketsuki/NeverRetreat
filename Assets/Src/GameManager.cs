using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    //�ܷ���Ļ���״̬
    public bool CanMachineStateChange = true;
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
    public int MobilizationRate;
    //���෽��Ԥ��������
    [SerializeField]
    public int PreTrainTroop;
    //���෽���ɼ�����ڵ�����
    [SerializeField]
    public int MaxActiveStableNodeCount;
    //������־ �� �¼��б�
    public List<Tuple<SpecialEvent, Vector3Int>> HumanEventList = new List<Tuple<SpecialEvent, Vector3Int>>();


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
    public List<Tuple<SpecialEvent, Vector3Int>> CrashEventList = new List<Tuple<SpecialEvent, Vector3Int>>();

    //�ж�Ŀ��ص�
    public Vector3Int ActionTargetPos;

    void Start()
    {
        GM = this;
        ActionTargetPos = Vector3Int.zero;
    }
    

    //���û���״̬
    public void SetMachineState(MachineState state){if(CanMachineStateChange) machineState = state;}
    //��ȡ����״̬
    public MachineState GetMachineState(){return machineState;}

    int stageMode = 5;
    public void NextStage()//������һ�׶�
    {
        bool NextTurn = false;
        if (ActionSide == ArmyBelong.Human) NextTurn = true;

        StageEnd(NextTurn);
        if (NextTurn)
        {
            //���غ�
            Stage = (TurnStage)(((int)Stage + 1) % stageMode);
            ActionSide = ArmyBelong.ModCrash;
            ActionPool = FixGameData.FGD.CrashPiecePool;
            EnemyPool = FixGameData.FGD.HumanPiecePool;
        }
        else
        {
            //�����
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
            case TurnStage.Support:
                SupportStageStart();
                break;
        }

        currentPiece = null;
        currentPosition = new Vector3Int(114, 514);
        SetMachineState(MachineState.Idel);
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
            case TurnStage.Support:
                SupportStageEnd();
                break;

        }
    }
    //���ػغ���Ϣ
    public void LoadTurnData(TurnData data)
    {
        MaxMobilizationRate = 50;
        if(data == null)
        {
            data = FixGameData.FGD.TurnDatas[CurrentTurnCount];
            //Ϊ�գ�����Ĭ�ϴӵ���غϿ�ʼ
            MaxActiveFissureCount = data.MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = data.MaxActiveBarrierAmmount;
            MaxCrashBandwidth = data.CrashBundith;
            Stage = data.StartStage;
            if (data.PreTrainedAmount > 0) PreTrainTroop = data.PreTrainedAmount;
            if (data.MobilizationRate > 0) MobilizationRate = data.MobilizationRate;

            //��ȡ��Ԯ
            FixGameData.FGD.HumanLoadList.AddRange(data.HumanReinforceList);
            FixGameData.FGD.CrashLoadList.AddRange(data.CrashReinforceList);
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
            //��ȡ��Ԯ
            //��ȡ��Ԯ
            FixGameData.FGD.HumanLoadList.AddRange(data.HumanReinforceList);
            FixGameData.FGD.CrashLoadList.AddRange(data.CrashReinforceList);

            //���õ�ǰ�غ���
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
        Streagy.GetAirForce();
    }
    void StrategyStageEnd(bool isTurnChange)
    {
        if (isTurnChange)
        {
            //չ�����
            Map.UpdateStaticBarrier();
            //����˺�
            for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
            {
                var tmp = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
                if (FixGameData.FGD.MapList[14].GetTile(tmp.piecePosition) != null)
                {
                    tmp.TakeDemage(1);
                }
            }
            //�����¼�
            Streagy.CulEvent();
            //������϶״̬
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

    #region //ģ��ս�׶�
    void ModeBattleStageStart()
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(true);
    }

    void ModeBattleStageEnd(bool isTurnChange)
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(false);
    }

    #endregion

    #region//�ж��׶�
    void ActionStageStart()
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(true);
    }

    void ActionStageEnd()
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(false);
    }

    #endregion

    #region //��Ԯ�׶�

    void SupportStageStart()
    {
        List<Tuple<string, string, int>> temp;
        if (ActionSide == ArmyBelong.Human) temp = FixGameData.FGD.HumanLoadList;
        else temp = FixGameData.FGD.CrashLoadList;

        if(temp.Count > 0)
        {
            FixGameData.FGD.uiIndex.SupportUISet.SetActive(true);
            SetMachineState(MachineState.Supporting);
            CanMachineStateChange = false;
        }
        else
        {
            NextStage();
        }

        
    }

    void SupportStageEnd()
    {
        FixGameData.FGD.uiIndex.SupportUISet.SetActive(false);

        if (ActionSide == ArmyBelong.Human) FixGameData.FGD.HumanLoadList = FixGameData.FGD.HumanLoadList.Where(x => x.Item3 != 0).ToList();
        else FixGameData.FGD.CrashLoadList = FixGameData.FGD.CrashLoadList.Where(x => x.Item3 != 0).ToList();

        CanMachineStateChange = true;
        SetMachineState(MachineState.Idel);
    }

    #endregion
}

//����״̬��״̬
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
    SelectEnemyPiece,
    Supporting
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