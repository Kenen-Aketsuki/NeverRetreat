using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;

    public GameObject PiecePrefab;
    public GameObject PieceInfoPrefab;

    //���ӱ�������
    public Transform HumanPieceParent;
    public Transform CrashPieceParent;

    //������Ϣ����
    public Transform DataHumanPieceParent;
    public Transform DataCrashPieceParent;


    private void Start()
    {
        FGD = this;
    }

}
