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

    //��Ϸ����
    //����״̬����ǰ״̬
    [SerializeField]
    MachineState machineState;
    //��ǰ�ж���
    public ArmyBelong ActionSide;
    //��ǰ�غ���
    public int CurrentCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }

    public void SetMachineState(MachineState state){machineState = state;}

    public MachineState GetMachineState(){return machineState;}
    
}

//����״̬��״̬
public enum MachineState
{
    Idel,
    FocusOnPiece,
    WaitMoveTarget
}
//�غϽ׶�
public enum TurnStage
{

}