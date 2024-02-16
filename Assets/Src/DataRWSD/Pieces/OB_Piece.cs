using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Playables;

public class OB_Piece : MonoBehaviour
{
    public static List<Vector3Int> needChenkVisibility = new List<Vector3Int>();

    Piece Data;//��������
    PieseTextShow PieceText;
    //�ж��㣬�����ڽ���
    public int ActionPoint = 0;
    //�����ж��㣬�����ڻ���֧Ԯ��ʩ������Ϊ��
    public int SpecialActPoint = 0;

    public bool isVisiable { get; private set; }
    public Vector3Int piecePosition{ get { return FixGameData.FGD.InteractMap.WorldToCell(transform.position); } }

    [SerializeField]
    SpriteRenderer BaseColor;
    [SerializeField]
    GameObject CrashCover;
    [SerializeField]
    SpriteRenderer TroopType;
    [SerializeField]
    GameObject AreaSlash;
    [SerializeField]
    SpriteMask VisibaleMask;

    //���ӵ��ƶ�·��
    List<CellInfo> Path;
    int PathCount = 0;

    //����״̬
    bool needMove = false;//�Ƿ���Ҫ�ƶ�
    float timer;

    //��ȡ��ͼ����
    public Vector3Int PosInMap { get
        {
            return FixGameData.FGD.InteractMap.WorldToCell((transform.position));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitData();
    }

    private void OnDestroy()
    {
        if(Data.Belong == ArmyBelong.Human)
        {
            Destroy(FixGameData.FGD.DataHumanPieceParent.Find(gameObject.name).gameObject);
        }
        else
        {
            Destroy(FixGameData.FGD.DataCrashPieceParent.Find(gameObject.name).gameObject);
        }
    }

    private void Update()
    {
        if (needMove)
        {
            //�ƶ�������
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                if (PathCount < Path.Count)
                {
                    transform.position = FixGameData.FGD.InteractMap.CellToWorld(Path[PathCount].Positian);
                    PathCount++;
                    timer = 0.1f;
                }
                else
                {
                    needMove = false;
                    EndMove();
                }
            }

        }
    }

    private void OnEnable()
    {
        if (PieceText != null)
            PieceText.gameObject.SetActive(isActiveAndEnabled);
    }
    private void OnDisable()
    {
        PieceText.gameObject.SetActive(isActiveAndEnabled);
    }
    //��ʼ��������ʾ����
    void InitData()
    {
        Transform parent;
        if(Data.Belong == ArmyBelong.Human)
        {
            parent = FixGameData.FGD.DataHumanPieceParent;
            
        }
        else
        {
            parent = FixGameData.FGD.DataCrashPieceParent;
        }
        //�����ı���ʾ��
        GameObject TextData = Instantiate(FixGameData.FGD.PieceInfoPrefab, parent);
        TextData.name = Data.PDesignation;
        PieceText = TextData.GetComponent<PieseTextShow>();
        PieceText.setParPice(transform);
        PieceText.InitText(gameObject.name, Data);
        
        //������ʾ
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);
        //���ñ���
        Color bakC;
        UnityEngine.ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        
        isVisiable = true;
    }
    //�Լ죬������������ʾ����
    void UpdateData()
    {
        //�Լ�
        //��鲹����ͨ�����
        CheckSupplyConnect();

        //�����ı�
        PieceText.InitText(gameObject.name, Data);
        //��ʾ��������
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        //���ĵ�ɫ
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
    }
    //������������
    public void setPieceData(Piece P)
    {
        Data = P;
    }
    //��ȡ��������
    public Piece getPieceData()
    {
        return Data;
    }
    //���ÿɼ���
    public void setVisibility(bool visible)
    {
        if (visible ^ isVisiable)//����ͬ��
        {
            isVisiable = visible;
            PieceText.gameObject.SetActive(isVisiable);
            //VisibaleMask.enabled = !visible;
            //gameObject.SetActive(visible);
        }
    }

    public Tuple<string, Vector2Int, string, int, int, bool> getPieceDataPack()
    {
        Vector3Int pos = FixGameData.FGD.InteractMap.WorldToCell(gameObject.transform.position);
        Vector2Int pos2 = FixGameData.WorldToMap(pos);
        return Data.getDataPack(pos2);
    }

    //���ӵĶ���
    //����
    public void TakeDemage(int Dmg)
    {
        if (!Data.TakeDemage(Dmg))
        {
            Death(gameObject,Data);
        }
        UpdateData();
    }
    //����
    public static void Death(GameObject Pse,Piece Data)
    {
        if(Data.LoyalTo == ArmyBelong.Human)
        {
            FixGameData.FGD.HumanPiecePool.DelChildByID(Pse.name);
        }
        else
        {
            FixGameData.FGD.CrashPiecePool.DelChildByID(Pse.name);
        }

        Destroy(Pse);
        Map.UpdateCrashBindwith();
    }
    //��Ѫ
    public void Recover()
    {
        Data.Recover();
        UpdateData();
    }
    //���غ�
    public void OverTurn()
    {
        Data.OverTurn();
        ActionPoint = 0;
        SpecialActPoint = 3;
        UpdateData();
    }

    //�ָ��ȶ���
    public void RecoverStable(int pt)
    {
        Data.RecoverStable(pt);
        UpdateData();
    }
    //�ܵ�����
    public void TakeUnstable(int Dmg)
    {
        Data.TakeUnstable(Dmg);
        UpdateData();
    }
    //���߷�
    public void Betray()
    {
        //��������
        PiecePool.ChangeSide(gameObject.name, PosInMap, Data.LoyalTo);
        Data.Betray();
        //��������
        if (Data.LoyalTo == ArmyBelong.Human)
        {
            transform.SetParent(FixGameData.FGD.HumanPieceParent);
        }
        else
        {
            transform.SetParent(FixGameData.FGD.CrashPieceParent);
        }
        UpdateData();
    }

    //׼���ƶ�
    public bool PrepareMove()
    {
        if (Data.MOV <= 0) return false;
        GameManager.GM.MoveArea = Map.DijkstraPathSerch(piecePosition, Data.MOV);
        Map.UpdateMoveArea();
        return true;
    }
    //�ƶ�
    public bool MoveTo(Vector3Int Target)
    {
        PiecePool pool;
        if (Data.LoyalTo == ArmyBelong.Human) pool = FixGameData.FGD.HumanPiecePool;
        else pool = FixGameData.FGD.CrashPiecePool;

        Path = null;
        //Ѱ·
        if (FixGameData.FGD.MoveAreaMap.HasTile(Target))
        {
            Path = Map.DijkstraPathReverse(GameManager.GM.MoveArea, Target);
            //����ƶ���Χ
            GameManager.GM.MoveArea.Clear();
            Map.UpdateMoveArea();
        }
        if(Path != null)
        {
            //�����ƶ���
            Data.DecreaseMov(Path[Path.Count - 1].usedCost);
            PathCount = 0;
            timer = -1;
            needMove = true;
            //���������ƶ�
            pool.UpdateChildPos(name, Target);

        }else return false;
        return true;
    }
    //�ƶ�����
    public void EndMove()
    {
        Path.Clear();
        //���²������Ӷѵ���־
        Map.UpdatePieceStackSign();
        //�����˫���Ĳ���������
        UpdateData();

    }
    //��������벹��
    public void CheckSupplyConnect()
    {
        bool Connect = true;//Ĭ��δʧ��
        bool unSupply = true ;//Ĭ�϶ϲ�
        //�ж�ʧ��
        int countZoc = 0;
        List<LandShape> shaplst = Map.GetPLaceInfo(piecePosition, 0);
        //shaplst = shaplst.Where(x => x.id == "DefenceArea").ToList();
        if (!(shaplst[3] != null && shaplst[3].id == "DefenceArea"))//���Ӳ����ڷ�������
        {
            for (int i = 1; i < 7; i++)
            {
                countZoc += FixGameData.FGD.ZOCMap.HasTile(Map.GetRoundSlotPos(piecePosition, i)) ? 1 : 0;
            }
            if (countZoc == 6) Connect = false;
        }

        //�ж�����
        string tarId;
        if (Data.Belong == ArmyBelong.Human) tarId = "HunterGuild";
        else tarId = "DimensionFissure";

        //�ӽ���Զ���򹤻�
        foreach (FacilityDataCell dta in FixGameData.FGD.FacilityList.Where(x=>x.Id == tarId).OrderBy(x=>Map.HexDistence(piecePosition,x.Positian)).ToList())
        {
            //����Ѱ·��������ֱ��������������Ϊû�С�
            if (Map.AStarPathSerch(piecePosition, dta.Positian, 20) != null)
            {
                unSupply = false;
                break;
            }
        }
        Data.UpdateSupplyConnection(unSupply, Connect);

    }
    //����Ƿ���Ҫ����
    public static void CheckVisibility()
    {
        PiecePool pool = GameManager.GM.ActionPool;

        foreach(Vector3Int pie in needChenkVisibility)
        {
            List<GameObject> tmpl = pool.getChildByPos(pie);

            if (tmpl.Count() > 0)
            {
                tmpl[0].SetActive(true);
            }
            for (int i = 1;i< tmpl.Count;i++)
            {
                tmpl[i].SetActive(false);
            }
        }

        needChenkVisibility.Clear();
    }
}
