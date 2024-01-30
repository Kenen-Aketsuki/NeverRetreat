using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    Piece Data;//棋子数据
    PieseTextShow PieceText;

    [SerializeField]
    SpriteRenderer BaseColor;
    [SerializeField]
    GameObject CrashCover;
    [SerializeField]
    SpriteRenderer TroopType;
    [SerializeField]
    GameObject AreaSlash;
    

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
        
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);

        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        
    }
    //更新棋子显示数据
    void UpdateData()
    {
        //更新文本
        PieceText.InitText(gameObject.name, Data);
        //显示崩坏遮罩
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        //更改底色
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;

        //更改父元素(其它修改在别的位置)
        //if (Data.LoyalTo == ArmyBelong.Human) transform.parent = FixGameData.FGD.HumanPieceParent;
        //else transform.parent = FixGameData.FGD.CrashPieceParent;

        

    }
    //设置棋子数据
    public void setPieceData(Piece P)
    {
        Data = P;
    }

    public Tuple<string, Vector2Int, string, int, int, bool> getPieceData()
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
        PiecePool.ChangeSide(gameObject.name, GetMapPos(), Data.LoyalTo);
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

    //移动
    public bool MoveTo(Vector3Int Target)
    {
        //测试用
        Data.TryMove(1);

        //判断能否移动
        //寻路
        //检查剩余移动点
        //移动到坐标
        //减少移动点

        UpdateData();
        return true;
    }
    //检查联络与补给
    public void CheckSupplyConnect()
    {
        bool Connect = false;
        bool unSupply = true ;
        //判定失联

        //从近到远排序工会
        //依次寻路，若有则直接跳出，否则认为没有。
        Data.UpdateSupplyConnection(unSupply, Connect);
        UpdateData();
    }


    //获取地图坐标
    public Vector3Int GetMapPos()
    {
        return FixGameData.FGD.InteractMap.WorldToCell(transform.position);
    }
}
