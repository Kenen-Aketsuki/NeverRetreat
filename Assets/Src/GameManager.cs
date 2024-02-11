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
    public int CurrentCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }

    public void SetMachineState(MachineState state){machineState = state;}

    public MachineState GetMachineState(){return machineState;}
    
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

}