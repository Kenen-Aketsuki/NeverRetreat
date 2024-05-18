using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    public static List<Vector3Int> needChenkVisibility = new List<Vector3Int>();

    Piece Data;//��������
    PieseTextShow PieceText;
    //�ж��㣬�����ڽ���
    public int ActionPoint = 0;
    //�����ж��㣬�����ڻ���֧Ԯ��ʩ������Ϊ��
    public int SpecialActPoint = 0;

    public Vector3Int piecePosition{ get { return FixGameData.FGD.InteractMap.WorldToCell(transform.position); } }
    //�ܷ��ƶ�
    public bool canMove { get { return Data.MOV > 0; } }
    //�Ƿ�Ϊ��Ӣ��λ
    public bool isSpecialPiece { get {
            return Data.canSupport ||
                Data.canDoMagic ||
                Data.canBild ||
                Data.canFixMod ||
                Data.canMental;
        } }

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
    bool needCheckGround = true; // �Ƿ������
    public bool needMove { get; private set; }//�Ƿ���Ҫ�ƶ�
    float timer;
    [SerializeField]
    int invisiableDmg = 0;

    //��ȡ��ͼ����
    public Vector3Int PosInMap { get
        {
            return FixGameData.FGD.InteractMap.WorldToCell((transform.position));
        }
    }

    private void Awake()
    {
        PieceText = Instantiate(FixGameData.FGD.PieceInfoPrefab).GetComponent<PieseTextShow>();
        needMove = false;
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
                if (PathCount < Path?.Count)
                {
                    transform.position = FixGameData.FGD.InteractMap.CellToWorld(Path[PathCount].Positian);
                    //if(needCheckGround) CheckGround(Path[PathCount], Path[PathCount].fromDir);
                    if (needCheckGround) CheckGround(Path[PathCount]);
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
        //GameObject TextData = Instantiate(FixGameData.FGD.PieceInfoPrefab, parent);
        PieceText.gameObject.name = Data.PDesignation;
        PieceText.transform.parent = parent;
        //PieceText = TextData.GetComponent<PieseTextShow>();
        PieceText.setParPice(transform);
        PieceText.InitText(gameObject.name, Data);
        
        //������ʾ
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);
        //���ñ���
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
    }
    //�Լ죬������������ʾ����
    void UpdateData()
    {
        //�Լ�
        //��鲹����ͨ�����
        //CheckSupplyConnect();

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
        Debug.Log(Pse.name + "����");

        if(Data.LoyalTo == ArmyBelong.Human)
        {
            FixGameData.FGD.HumanPiecePool.DelChildByID(Pse.name);
        }
        else
        {
            FixGameData.FGD.CrashPiecePool.DelChildByID(Pse.name);
        }

        if (Data.PDesignation == "Human.Betray") GameManager.GM.MobilizationRate--;
        else if (Data.PieceID.Contains("Infantry")) FixGameData.FGD.HumanDeathList.Add(Data.PDesignation);
        else if (Data.PieceID == "GovermentVIP") FixGameData.FGD.resultMem.KillGOV();

        needChenkVisibility.Add(Pse.GetComponent<OB_Piece>().piecePosition);
        Destroy(Pse);
        CheckVisibility();
        

        Map.UpdateCrashBindwith();

        //����ZOC
        Map.UpdateZOC();
    }
    //��Ѫ
    public bool Recover()
    {
        bool tmp = Data.Recover();
        UpdateData();
        return tmp;
    }
    //���غ�
    public void OverTurn()
    {
        Data.OverTurn();//�����ƶ���������Ӱ��
        ResetAction();//�����ж������Դ�����
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
        //����ZOC
        Map.UpdateZOC();
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
        needCheckGround = true;
        if(Path != null) Path.Clear();
        if (invisiableDmg != 0)
        {
            TakeDemage(-invisiableDmg);
            invisiableDmg = 0;
        }

        //���²������Ӷѵ���־
        Map.UpdatePieceStackSign();
        //�����˫���Ĳ���������
        UpdateData();
        //�����ж�Ŀ��
        if (Data.PieceID == "Govermant") GameManager.GM.ActionTargetPos = piecePosition;

    }
    //������
    public void CheckGround(CellInfo cell)
    {
        Tuple<int, Vector3Int> sidePos = Map.GetSideAddr(cell.Positian, cell.fromDir);

        //�����ʩ
        int addr = FixGameData.FGD.FacilityList.FindIndex(x => x.Positian == sidePos.Item2 && x.dir == sidePos.Item1 + 1);
        FacilityDataCell fac = addr == -1 ? null : FixGameData.FGD.FacilityList[addr];
        if (fac != null && (fac.Id == "Landmine" || fac.Id == "IFFLandmine"))
        {
            invisiableDmg += (int)fac.Data.Item2.HP_All.Item2;
            Tuple<FixWay, float> fix = fac.Data.Item2.HP_IFF(Data.LoyalTo);
            invisiableDmg += (int)(fix != null ? fix.Item2 : 0);
            fac.RemoveSelf();
            FixGameData.FGD.FacilityList.RemoveAt(addr);
        }
        //������
        addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == cell.Positian);
        fac = addr == -1 ? null : FixGameData.FGD.SpecialTerrainList[addr];
        if (fac != null && fac.Id == "PosDisorderZone" && Data.LoyalTo == ArmyBelong.Human)
        {
            //�ȵ�����������
            EndMove();

            FixGameData.FGD.CameraNow.transform.position = transform.position + new Vector3Int(0, 0, -10);
            //��������յ�
            List<CellInfo> area = Map.PowerfulBrickAreaSearch(piecePosition, 10);
            Vector3Int target = area[UnityEngine.Random.Range(0, area.Count)].Positian;

            if (TPMoveTo(target)) Death(gameObject, Data);
        }else if (fac != null && fac.Id == "DataDisorderZone" && Data.LoyalTo == ArmyBelong.Human)
        {
            CulateSupplyConnection(true, false);
        }

        

    }

    public bool CheckStand()
    {
        //���˴��޷�վ����������
        return Map.GetNearMov(piecePosition, 0, Data.LoyalTo) == -1;
    }
    //��������벹�����ڻغ�ĩ����
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
                countZoc += FixGameData.FGD.ZOCMap().HasTile(Map.GetRoundSlotPos(piecePosition, i)) ? 1 : 0;
            }
            if (countZoc == 6) Connect = false;
        }

        //�ж�����
        string tarId;
        if (Data.Belong == ArmyBelong.Human) tarId = "HunterGuild";
        else tarId = "DimensionFissure";

        List<FacilityDataCell> facList = FixGameData.FGD.FacilityList.Where(x => x.Id == tarId).ToList();
        facList.AddRange(FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == tarId));

        //�ӽ���Զ���򹤻�
        foreach (FacilityDataCell dta in facList.OrderBy(x => Map.HexDistence(piecePosition, x.Positian)))
        {
            //����Ѱ·��������ֱ��������������Ϊû�С�
            if (Map.AStarPathSerch(piecePosition, dta.Positian, 20) != null)
            {
                unSupply = false;
                break;
            }
        }
        Data.CulateSupplyConnection(unSupply, Connect);

    }

    public void CulateSupplyConnection(bool isUnsupply, bool isConnected)
    {
        Data.CulateSupplyConnection(isUnsupply, isConnected);
        UpdateData();
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

        pool = GameManager.GM.EnemyPool;

        foreach (Vector3Int pie in needChenkVisibility)
        {
            List<GameObject> tmpl = pool.getChildByPos(pie);

            if (tmpl.Count() > 0)
            {
                tmpl[0].SetActive(true);
            }
            for (int i = 1; i < tmpl.Count; i++)
            {
                tmpl[i].SetActive(false);
            }
        }

        needChenkVisibility.Clear();
    }

    //ǿ���ƶ�
    public bool ForceMoveTo(Vector3Int Target)
    {
        PiecePool pool;
        if (Data.LoyalTo == ArmyBelong.Human) pool = FixGameData.FGD.HumanPiecePool;
        else pool = FixGameData.FGD.CrashPiecePool;

        Path = null;
        //Ѱ·
        //Path = Map.AStarPathSerch(piecePosition, Target, 500);
        Path = Map.LineSerch(piecePosition, Target);
        if (Path != null)
        {
            PathCount = 0;
            timer = -1;
            needMove = true;
            //���������ƶ�
            pool.UpdateChildPos(name, Target);

        }
        else return false;
        return true;
    }
    //˲��
    public bool TPMoveTo(Vector3Int Target)
    {
        PiecePool pool;
        if (Data.LoyalTo == ArmyBelong.Human) pool = FixGameData.FGD.HumanPiecePool;
        else pool = FixGameData.FGD.CrashPiecePool;

        needChenkVisibility.Add(piecePosition);

        pool.UpdateChildPos(name, Target);
        transform.position = FixGameData.FGD.InteractMap.CellToWorld(Target);
        PieceText.transform.position = FixGameData.FGD.InteractMap.CellToWorld(Target);

        needChenkVisibility.Add(Target);

        CheckVisibility();

        return true;
    }

    //�����ƶ���
    public void ResetMov()
    {
        Data.ResetMov();
        UpdateData();
    }
    //�����ж���
    public void ResetAction()
    {
        ActionPoint = 1;
        SpecialActPoint = 3;
        UpdateData();
    }
    
    //ǿ��ɱ��
    public static void Kill(GameObject Pse, Piece Data)
    {
        if (Data.LoyalTo == ArmyBelong.Human)
        {
            FixGameData.FGD.HumanPiecePool.DelChildByID(Pse.name);
        }
        else
        {
            FixGameData.FGD.CrashPiecePool.DelChildByID(Pse.name);
        }

        needChenkVisibility.Add(Pse.GetComponent<OB_Piece>().piecePosition);
        Destroy(Pse);
        CheckVisibility();


        Map.UpdateCrashBindwith();

        //����ZOC
        Map.UpdateZOC();
    }
}
