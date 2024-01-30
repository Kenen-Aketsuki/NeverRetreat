using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    Piece Data;//��������
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
        
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);

        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        
    }
    //����������ʾ����
    void UpdateData()
    {
        //�����ı�
        PieceText.InitText(gameObject.name, Data);
        //��ʾ��������
        if (Data.LoyalTo == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        //���ĵ�ɫ
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;

        //���ĸ�Ԫ��(�����޸��ڱ��λ��)
        //if (Data.LoyalTo == ArmyBelong.Human) transform.parent = FixGameData.FGD.HumanPieceParent;
        //else transform.parent = FixGameData.FGD.CrashPieceParent;

        

    }
    //������������
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
        PiecePool.ChangeSide(gameObject.name, GetMapPos(), Data.LoyalTo);
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

    //�ƶ�
    public bool MoveTo(Vector3Int Target)
    {
        //������
        Data.TryMove(1);

        //�ж��ܷ��ƶ�
        //Ѱ·
        //���ʣ���ƶ���
        //�ƶ�������
        //�����ƶ���

        UpdateData();
        return true;
    }
    //��������벹��
    public void CheckSupplyConnect()
    {
        bool Connect = false;
        bool unSupply = true ;
        //�ж�ʧ��

        //�ӽ���Զ���򹤻�
        //����Ѱ·��������ֱ��������������Ϊû�С�
        Data.UpdateSupplyConnection(unSupply, Connect);
        UpdateData();
    }


    //��ȡ��ͼ����
    public Vector3Int GetMapPos()
    {
        return FixGameData.FGD.InteractMap.WorldToCell(transform.position);
    }
}
