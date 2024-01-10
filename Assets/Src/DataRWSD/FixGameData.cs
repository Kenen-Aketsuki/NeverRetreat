using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;

    public GameObject PiecePrefab;
    public GameObject PieceInfoPrefab;

    //棋子本体所在
    public Transform HumanPieceParent;
    public Transform CrashPieceParent;

    //棋子信息所在
    public Transform DataHumanPieceParent;
    public Transform DataCrashPieceParent;


    private void Start()
    {
        FGD = this;
    }

}
