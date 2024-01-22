using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    //��ͼ���
    //��������-����-��·-��ʩ(����)-��ʩ(���)-�������(���)-�������(����)
    //    0   -  1 -  4 -    7     -    8     -      11      -    14 ����ʼ��ַ��������-��-������
    public List<Tilemap> MapList;

    private void Start()
    {
        FGD = this;
    }

    public static Vector3Int MapToWorld(int x,int y,int xSize,int ySize)
    {
        x = x - (int)Math.Floor((double)(xSize / 2));
        y = y - (int)Math.Floor((double)(ySize / 2));
        return new Vector3Int(x, y, 0);
    }
}
