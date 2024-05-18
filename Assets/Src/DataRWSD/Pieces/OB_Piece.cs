using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    public static List<Vector3Int> needChenkVisibility = new List<Vector3Int>();

    Piece Data;//棋子数据
    PieseTextShow PieceText;
    //行动点，可用于进攻
    public int ActionPoint = 0;
    //特殊行动点，可用于火力支援、施法等行为。
    public int SpecialActPoint = 0;

    public Vector3Int piecePosition{ get { return FixGameData.FGD.InteractMap.WorldToCell(transform.position); } }
    //能否移动
    public bool canMove { get { return Data.MOV > 0; } }
    //是否为精英单位
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

    //棋子的移动路径
    List<CellInfo> Path;
    int PathCount = 0;

    //棋子状态
    bool needCheckGround = true; // 是否检查脚下
    public bool needMove { get; private set; }//是否需要移动
    float timer;
    [SerializeField]
    int invisiableDmg = 0;

    //获取地图坐标
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
            //移动到坐标
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
    //初始化棋子显示数据
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
        //创建文本显示器
        //GameObject TextData = Instantiate(FixGameData.FGD.PieceInfoPrefab, parent);
        PieceText.gameObject.name = Data.PDesignation;
        PieceText.transform.parent = parent;
        //PieceText = TextData.GetComponent<PieseTextShow>();
        PieceText.setParPice(transform);
        PieceText.InitText(gameObject.name, Data);
        
        //设置显示
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);
        //设置背景
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
    }
    //自检，并更新棋子显示数据
    void UpdateData()
    {
        //自检
        //检查补给与通信情况
        //CheckSupplyConnect();

        //更新文本
        PieceText.InitText(gameObject.name, Data);
        //显示崩坏遮罩
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        //更改底色
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
    }
    //设置棋子数据
    public void setPieceData(Piece P)
    {
        Data = P;
    }
    //获取棋子数据
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

    //棋子的动作
    //受伤
    public void TakeDemage(int Dmg)
    {
        if (!Data.TakeDemage(Dmg))
        {
            Death(gameObject,Data);
        }
        UpdateData();
    }
    //死亡
    public static void Death(GameObject Pse,Piece Data)
    {
        Debug.Log(Pse.name + "阵亡");

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

        //更新ZOC
        Map.UpdateZOC();
    }
    //回血
    public bool Recover()
    {
        bool tmp = Data.Recover();
        UpdateData();
        return tmp;
    }
    //过回合
    public void OverTurn()
    {
        Data.OverTurn();//重置移动力，结算影响
        ResetAction();//重置行动力，自带更新
    }

    //恢复稳定性
    public void RecoverStable(int pt)
    {
        Data.RecoverStable(pt);
        UpdateData();
    }
    //受到干扰
    public void TakeUnstable(int Dmg)
    {
        Data.TakeUnstable(Dmg);
        UpdateData();
    }
    //被策反
    public void Betray()
    {
        //数据跳边
        PiecePool.ChangeSide(gameObject.name, PosInMap, Data.LoyalTo);
        Data.Betray();
        //本体跳边
        if (Data.LoyalTo == ArmyBelong.Human)
        {
            transform.SetParent(FixGameData.FGD.HumanPieceParent);
        }
        else
        {
            transform.SetParent(FixGameData.FGD.CrashPieceParent);
        }
        UpdateData();
        //更新ZOC
        Map.UpdateZOC();
    }

    //准备移动
    public bool PrepareMove()
    {
        if (Data.MOV <= 0) return false;
        GameManager.GM.MoveArea = Map.DijkstraPathSerch(piecePosition, Data.MOV);
        Map.UpdateMoveArea();
        return true;
    }
    //移动
    public bool MoveTo(Vector3Int Target)
    {
        PiecePool pool;
        if (Data.LoyalTo == ArmyBelong.Human) pool = FixGameData.FGD.HumanPiecePool;
        else pool = FixGameData.FGD.CrashPiecePool;

        Path = null;
        //寻路
        if (FixGameData.FGD.MoveAreaMap.HasTile(Target))
        {
            Path = Map.DijkstraPathReverse(GameManager.GM.MoveArea, Target);
            //清空移动范围
            GameManager.GM.MoveArea.Clear();
            Map.UpdateMoveArea();
        }
        if(Path != null)
        {
            //减少移动点
            Data.DecreaseMov(Path[Path.Count - 1].usedCost);
            PathCount = 0;
            timer = -1;
            needMove = true;
            //棋子数据移动
            pool.UpdateChildPos(name, Target);

        }else return false;
        return true;
    }

    //移动结束
    public void EndMove()
    {
        needCheckGround = true;
        if(Path != null) Path.Clear();
        if (invisiableDmg != 0)
        {
            TakeDemage(-invisiableDmg);
            invisiableDmg = 0;
        }

        //重新布设棋子堆叠标志
        Map.UpdatePieceStackSign();
        //检查我双方的补给与联络
        UpdateData();
        //更新行动目标
        if (Data.PieceID == "Govermant") GameManager.GM.ActionTargetPos = piecePosition;

    }
    //检查脚下
    public void CheckGround(CellInfo cell)
    {
        Tuple<int, Vector3Int> sidePos = Map.GetSideAddr(cell.Positian, cell.fromDir);

        //检查设施
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
        //检查地形
        addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == cell.Positian);
        fac = addr == -1 ? null : FixGameData.FGD.SpecialTerrainList[addr];
        if (fac != null && fac.Id == "PosDisorderZone" && Data.LoyalTo == ArmyBelong.Human)
        {
            //踩到坐标紊乱区
            EndMove();

            FixGameData.FGD.CameraNow.transform.position = transform.position + new Vector3Int(0, 0, -10);
            //随机生成终点
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
        //若此处无法站立，则死亡
        return Map.GetNearMov(piecePosition, 0, Data.LoyalTo) == -1;
    }
    //检查联络与补给，在回合末进行
    public void CheckSupplyConnect()
    {
        bool Connect = true;//默认未失联
        bool unSupply = true ;//默认断补
        //判定失联
        int countZoc = 0;
        List<LandShape> shaplst = Map.GetPLaceInfo(piecePosition, 0);
        //shaplst = shaplst.Where(x => x.id == "DefenceArea").ToList();
        if (!(shaplst[3] != null && shaplst[3].id == "DefenceArea"))//部队不处于防御区内
        {
            for (int i = 1; i < 7; i++)
            {
                countZoc += FixGameData.FGD.ZOCMap().HasTile(Map.GetRoundSlotPos(piecePosition, i)) ? 1 : 0;
            }
            if (countZoc == 6) Connect = false;
        }

        //判定补给
        string tarId;
        if (Data.Belong == ArmyBelong.Human) tarId = "HunterGuild";
        else tarId = "DimensionFissure";

        List<FacilityDataCell> facList = FixGameData.FGD.FacilityList.Where(x => x.Id == tarId).ToList();
        facList.AddRange(FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == tarId));

        //从近到远排序工会
        foreach (FacilityDataCell dta in facList.OrderBy(x => Map.HexDistence(piecePosition, x.Positian)))
        {
            //依次寻路，若有则直接跳出，否则认为没有。
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

    //检查是否需要隐藏
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

    //强制移动
    public bool ForceMoveTo(Vector3Int Target)
    {
        PiecePool pool;
        if (Data.LoyalTo == ArmyBelong.Human) pool = FixGameData.FGD.HumanPiecePool;
        else pool = FixGameData.FGD.CrashPiecePool;

        Path = null;
        //寻路
        //Path = Map.AStarPathSerch(piecePosition, Target, 500);
        Path = Map.LineSerch(piecePosition, Target);
        if (Path != null)
        {
            PathCount = 0;
            timer = -1;
            needMove = true;
            //棋子数据移动
            pool.UpdateChildPos(name, Target);

        }
        else return false;
        return true;
    }
    //瞬移
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

    //重置移动力
    public void ResetMov()
    {
        Data.ResetMov();
        UpdateData();
    }
    //重置行动点
    public void ResetAction()
    {
        ActionPoint = 1;
        SpecialActPoint = 3;
        UpdateData();
    }
    
    //强制杀死
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

        //更新ZOC
        Map.UpdateZOC();
    }
}
