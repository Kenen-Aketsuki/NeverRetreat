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
    public Vector3Int currentPosition = new Vector3Int(114514, 1919810);

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
    //���෽���ɼ�����ڵ�����
    [SerializeField]
    public int MaxActiveStableNodeCount;
    //������־--��������
    [SerializeField]
    public int MaxCrashBandwidth;
    //������־--����
    [SerializeField]
    public int CrashBandwidth;
    //������־���ɼ�����϶��
    [SerializeField]
    public int MaxActiveFissureCount;


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
        StageEnd();
        Stage = (TurnStage)(((int)Stage + 1) % stageMode);
        StageStart();
    }

    public void StageStart()
    {
        Map.UpdateCrashBindwith();
        Map.UpdateZOC();
        FixGameData.FGD.uiIndex.turnData.UpdateInfo();
        switch (Stage)
        {
            case 0:
                stageMode = 5;
                TurnSwitch();
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
            MobilizationRate = 50;
            MaxActiveFissureCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = FixGameData.FGD.TurnDatas[CurrentTurnCount].MaxActiveBarrierAmmount;
            MaxCrashBandwidth = FixGameData.FGD.TurnDatas[CurrentTurnCount].CrashBundith;
            Stage = FixGameData.FGD.TurnDatas[CurrentTurnCount].StartStage;
        }
        else
        {
            //�ǿգ�ͬ���浵
            MobilizationRate = data.MobilizationRate;
            MaxActiveFissureCount = data.MaxActiveFissureAmmount;
            MaxActiveStableNodeCount = data.MaxActiveBarrierAmmount;
            MaxCrashBandwidth = data.CrashBundith;
            Stage = data.StartStage;

            CurrentTurnCount = data.TurnNo;
        }
    }
    //�л��غ�
    void TurnSwitch()
    {
        CurrentTurnCount++;
        //���ض�Ӧ�غϵ���Ϣ
        LoadTurnData(null);
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

    }
    void StrategyStageEnd()
    {

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
    WaitMoveTarget
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