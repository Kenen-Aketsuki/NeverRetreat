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

    // Update is called once per frame
    void Update()
    {

    }

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
        GameObject TextData = Instantiate(FixGameData.FGD.PieceInfoPrefab, parent);
        TextData.name = Data.PDesignation;
        PieceText = TextData.GetComponent<PieseTextShow>();
        PieceText.setParPice(transform);
        PieceText.InitText(Data,Data.isTwo);

        if (Data.Belong == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        if (Data.activeArea > 0 || Data.passiveArea > 0) AreaSlash.SetActive(true);
        else AreaSlash.SetActive(false);

        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        TroopType.sprite = BasicUtility.getPieceIcon(Data.PName.Split('/')[2]);
        
    }

    void UpdateData()
    {
        //�����ı�
        PieceText.InitText(Data, Data.isTwo);
        //��ʾ��������
        if (Data.Belong == ArmyBelong.ModCrash) CrashCover.SetActive(true);
        else CrashCover.SetActive(false);
        //���ĵ�ɫ
        Color bakC;
        ColorUtility.TryParseHtmlString("#" + Data.BackColor, out bakC);
        BaseColor.color = bakC;
        //���ĸ�Ԫ��(�����޸��ڱ��λ��)
        if (Data.LoyalTo == ArmyBelong.Human) gameObject.transform.parent = FixGameData.FGD.HumanPieceParent;
        else gameObject.transform.parent = FixGameData.FGD.CrashPieceParent;
    }

    public void setPieceData(Piece P)
    {
        Data = P;
    }
}
