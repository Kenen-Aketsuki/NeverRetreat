using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;

    //���ӽṹ
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
    //�����õ�ͼ
    public Tilemap InteractMap;

    private void Start()
    {
        FGD = this;
    }

    public static Vector3Int MapToWorld(int x,int y)//�浵����ת��Ϸ��ͼ����
    {
        x = x - (int)Math.Floor((double)(GameUtility.mapSize.x / 2));
        y = y - (int)Math.Floor((double)(GameUtility.mapSize.y / 2));
        return new Vector3Int(x, y, 0);
    }

    public static Vector2Int WorldToMap(Vector3Int pos)//��Ϸ��ͼ����ת�浵����
    {
        int x = pos.x + (int)Math.Floor((double)(GameUtility.mapSize.x / 2));
        int y = pos.y + (int)Math.Floor((double)(GameUtility.mapSize.y / 2));
        return new Vector2Int(x, y);
    }
}
