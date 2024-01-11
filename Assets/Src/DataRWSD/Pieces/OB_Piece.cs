using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    Piece Data;//��������
    PieseTextShow PieceText;

    [SerializeField]
    SpriteRenderer BaseColor_Normal;
    [SerializeField]
    SpriteRenderer BaseColor_Casualty;
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
    }

    void UpdateData()
    {
        PieceText.InitText(Data, Data.isTwo);
    }

    public void setPieceData(Piece P)
    {
        Data = P;
    }
}
