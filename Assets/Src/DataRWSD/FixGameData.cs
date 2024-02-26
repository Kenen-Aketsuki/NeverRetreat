using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FixGameData : MonoBehaviour
{
    public static FixGameData FGD;
    public Camera CameraNow;
    public UIIndex uiIndex;
    public UIManager uiManager;

    //���ӽṹ
    public GameObject PiecePrefab;
    public GameObject PieceInfoPrefab;
    public GameObject UIPieceCell;

    //���ӱ�������
    public Transform HumanPieceParent;
    public Transform CrashPieceParent;

    //������Ϣ����
    public Transform DataHumanPieceParent;
    public Transform DataCrashPieceParent;

    //���ӳ�
    public PiecePool HumanPiecePool;
    public PiecePool CrashPiecePool;

    //��ͼ���
    //��������-����-��·-��ʩ(����)-��ʩ(���)-�������(���)-�������(����)
    //    0   -  1 -  4 -    7     -    8     -      11      -    14 ����ʼ��ַ��������-��-������
    public List<Tilemap> MapList;
    //�����õ�ͼ
    public Tilemap InteractMap;
    //�����ͼ 
    public Tilemap ZoneMap;
    //�ѵ���־
    public Tilemap MultiPieceMap;
    //������
    public Tilemap ZOCMap;
    //�ƶ���Χ��ͼ
    public Tilemap MoveAreaMap;

    //������Ƭ
    public Tile InteractFill;//��佻����
    public Tile MultiPieceIcon;//�ѵ����
    public Tile MoveArea;//�ƶ�����
    public Tile MoveZocArea;//��ZOC���ƶ�����

    //��Ϸ������
    //�㶨��ʩ�б�
    public List<FacilityDataCell> SpecialFacilityList = new List<FacilityDataCell>();
    //��ʱ��ʩ
    public List<FacilityDataCell> FacilityList = new List<FacilityDataCell>();
    //�������
    public List<FacilityDataCell> SpecialTerrainList = new List<FacilityDataCell>();
    //���غ���
    public int MaxRoundCount;
    //�غ���Ϣ
    public List<TurnData> TurnDatas;

    //���෽����׼���б� ��Ϣ�� ��������Ϣ-�����볡��ʽ-�����볡����ʱ��
    public List<Tuple<string, string, int>> HumanLoadList = new List<Tuple<string, string, int>>();
    public List<Tuple<string, string, int>> HumanFixLoadList = new List<Tuple<string, string, int>>();
    //���෽���������б����ڻغϳ��������Ϊ���ӷ���
    public List<string> HumanDeathList = new List<string>();
    //���෽֧Ԯǩ �� �Բ��ӷ���ΪKey�����ô���ΪValue
    public Dictionary<string, int> HumanSupportDic = new Dictionary<string, int>();
    //���෽�����¼�
    public List<SpecialEvent> HumanSpecialEventList = new List<SpecialEvent> { SpecialEvent.MentalAD,SpecialEvent.TrainTroop,SpecialEvent.RetreatCiv};

    //����������׼���б�
    public List<Tuple<string, string,int>> CrashLoadList=new List<Tuple<string, string, int>>();
    //����������֧Ԯ���ã�����Ϊ���ӷ���
    public List<string> CrashDeathList = new List<string>();
    //������֧Ԯǩ
    public Dictionary<string, int> CrashSupportDic=new Dictionary<string, int>();
    //�����������¼�
    public List<SpecialEvent> CrashSpecialEventList = new List<SpecialEvent> { SpecialEvent.DataStrom, SpecialEvent.SpaceSplit, SpecialEvent.SpaceFix,SpecialEvent.PosConfuse };

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

public class FacilityDataCell
{
    public Tuple<Type,LandShape> Data
    {
        get
        {
            LandShape tmp = null;
            Type type = null;
            if (FixSystemData.GlobalFacilityList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalFacilityList[Id];
                type = typeof(Facility);
            }
            else if (FixSystemData.GlobalSpFacilityList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalSpFacilityList[Id];
                type = typeof(SpecialFacility);
            }
            else if (FixSystemData.GlobalSpecialTerrainList.ContainsKey(Id))
            {
                tmp = FixSystemData.GlobalSpecialTerrainList[Id];
                type = typeof(Facility);
            }
            return new Tuple<Type, LandShape>(type, tmp);
        }
    }

    public string Id { get; private set; }
    public Vector3Int Positian { get; private set; }
    public int dir { get; private set; }
    public int LastTime { get; private set; }//����ʱ��
    //�Ƿ񼤻�
    public bool active { get; private set; }

    public FacilityDataCell(string id, Vector3Int Pos,int Dir,int lastTime,bool atSide,bool active)
    {
        LastTime = lastTime;
        Id = id;
        if (atSide)
        {
            Tuple<int, Vector3Int> tmpP = Map.GetSideAddr(Pos, Dir);
            Positian = tmpP.Item2;
            dir = tmpP.Item1 + 1;
        }
        else
        {
            Positian = Pos;
            dir = 0;
        }
        this.active = active;
    }

    public bool PassTime()
    {
        LastTime--;
        return LastTime > 0;
    }

    public void RemoveSelf()
    {
        int startAddr = 0;
        if(Data.Item1 == typeof(Facility) && dir == 0)
        {
            Facility tmpf = (Facility)Data.Item2;
            //������ʩ
            startAddr = tmpf.isSpecialLandShape ? 14 : 7;
        }
        else if((Data.Item1 == typeof(Facility) && dir != 0))
        {
            Facility tmpf = (Facility)Data.Item2;
            //������ʩ
            startAddr = tmpf.isSpecialLandShape ? 11 : 8;
            startAddr += dir - 1;
        }

        FixGameData.FGD.MapList[startAddr].SetTile(Positian, null);

    }

    public void ChangeActive()
    {
        active = !active;
        FixGameData.FGD.MapList[7].SetTile(Positian, active ? (Data.Item2 as SpecialFacility).Active : (Data.Item2 as SpecialFacility).Close);
    }

    public void SetActive(bool act)
    {
        active = act;
        FixGameData.FGD.MapList[7].SetTile(Positian, active ? (Data.Item2 as SpecialFacility).Active : (Data.Item2 as SpecialFacility).Close);
    }
}

