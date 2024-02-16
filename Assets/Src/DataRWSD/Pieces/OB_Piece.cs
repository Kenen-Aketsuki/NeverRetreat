using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Playables;

public class OB_Piece : MonoBehaviour
{
    public static List<Vector3Int> needChenkVisibility = new List<Vector3Int>();

    Piece Data;//棋子数据
    PieseTextShow PieceText;
    //行动点，可用于进攻
    public int ActionPoint = 0;
    //特殊行动点，可用于火力支援、施法等行为。
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

    //棋子的移动路径
    List<CellInfo> Path;
    int PathCount = 0;

    //棋子状态
    bool needMove = false;//是否需要移动
    float timer;

    //获取地图坐标
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
            //移动到坐标
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
        GameObject TextData = Instantiate(FixGameData.FGD.PieceInfoPrefab, parent);
        TextData.name = Data.PDesignation;
        PieceText = TextData.GetComponent<PieseTextShow>();
        PieceText.setParPice(transform);
        PieceText.InitText(gameObject.name, Data);
        
        //设置显示
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);
        //设置背景
        Color bakC;
        UnityEngine.ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        
        isVisiable = true;
    }
    //自检，并更新棋子显示数据
    void UpdateData()
    {
        //自检
        //检查补给与通信情况
        CheckSupplyConnect();

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
    //设置可见性
    public void setVisibility(bool visible)
    {
        if (visible ^ isVisiable)//二者同或
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
    //回血
    public void Recover()
    {
        Data.Recover();
        UpdateData();
    }
    //过回合
    public void OverTurn()
    {
        Data.OverTurn();
        ActionPoint = 0;
        SpecialActPoint = 3;
        UpdateData();
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
        Path.Clear();
        //重新布设棋子堆叠标志
        Map.UpdatePieceStackSign();
        //检查我双方的补给与联络
        UpdateData();

    }
    //检查联络与补给
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
                countZoc += FixGameData.FGD.ZOCMap.HasTile(Map.GetRoundSlotPos(piecePosition, i)) ? 1 : 0;
            }
            if (countZoc == 6) Connect = false;
        }

        //判定补给
        string tarId;
        if (Data.Belong == ArmyBelong.Human) tarId = "HunterGuild";
        else tarId = "DimensionFissure";

        //从近到远排序工会
        foreach (FacilityDataCell dta in FixGameData.FGD.FacilityList.Where(x=>x.Id == tarId).OrderBy(x=>Map.HexDistence(piecePosition,x.Positian)).ToList())
        {
            //依次寻路，若有则直接跳出，否则认为没有。
            if (Map.AStarPathSerch(piecePosition, dta.Positian, 20) != null)
            {
                unSupply = false;
                break;
            }
        }
        Data.UpdateSupplyConnection(unSupply, Connect);

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

        needChenkVisibility.Clear();
    }
}
