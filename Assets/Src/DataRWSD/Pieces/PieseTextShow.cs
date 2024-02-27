using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PieseTextShow : MonoBehaviour
{
    Transform ParPiece;

    [SerializeField]
    Transform BaseData_Three;
    [SerializeField]
    Transform BaseData_Two;
    [SerializeField]
    TMP_Text TroopName;
    [SerializeField]
    TMP_Text ActiveArea;
    [SerializeField]
    TMP_Text PassiveArea;

    // Update is called once per frame
    void Update()
    {
        transform.position = ParPiece.position;

        //UpdateText(ParPiece.gameObject.GetComponent<OB_Piece>().getPieceData());

    }

    public void setParPice(Transform pp)
    {
        ParPiece = pp;
    }

    public void InitText(string ID, Piece data)//初始化棋子信息
    {
        TroopName.text = data.PDesignation;
        gameObject.name = ID;

        if (data.isTwo)
        {
            string ATK;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();
            
            BaseData_Two.GetChild(0).GetComponent<TMP_Text>().text = data.MOV.ToString();
            BaseData_Two.GetChild(2).GetComponent<TMP_Text>().text = ATK;
            BaseData_Two.gameObject.SetActive(true);
        }
        else
        {
            string ATK;
            string DEF;
            string MOV;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();
            if (data.DEF < 0) DEF = "X"; else DEF = data.DEF.ToString();
            if (data.MOV < 0) MOV = "X"; else MOV = data.MOV.ToString();
            BaseData_Three.GetChild(0).GetComponent<TMP_Text>().text = MOV;
            BaseData_Three.GetChild(2).GetComponent<TMP_Text>().text = DEF;
            BaseData_Three.GetChild(4).GetComponent<TMP_Text>().text = ATK;
            BaseData_Three.gameObject.SetActive(true);

            if (!data.canAirBattle && data.isAir) BaseData_Three.GetChild(2).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
            if(data.canAirBattle && !data.isAir) BaseData_Three.GetChild(4).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
        }
        if (data.activeArea > 0) ActiveArea.text = data.activeArea.ToString();
        else ActiveArea.gameObject.SetActive(false);
        if (data.passiveArea > 0) PassiveArea.text = data.passiveArea.ToString();
        else PassiveArea.gameObject.SetActive(false);
    }

    public void UpdateText(Piece data)
    {
        if (data.isTwo)
        {
            string ATK;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();

            BaseData_Two.GetChild(0).GetComponent<TMP_Text>().text = data.MOV.ToString();
            BaseData_Two.GetChild(2).GetComponent<TMP_Text>().text = ATK;
            BaseData_Two.gameObject.SetActive(true);
        }
        else
        {
            string ATK;
            string DEF;
            string MOV;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();
            if (data.DEF < 0) DEF = "X"; else DEF = data.DEF.ToString();
            if (data.MOV < 0) MOV = "X"; else MOV = data.MOV.ToString();
            BaseData_Three.GetChild(0).GetComponent<TMP_Text>().text = MOV;
            BaseData_Three.GetChild(2).GetComponent<TMP_Text>().text = DEF;
            BaseData_Three.GetChild(4).GetComponent<TMP_Text>().text = ATK;
            BaseData_Three.gameObject.SetActive(true);

            if (!data.canAirBattle && data.isAir) BaseData_Three.GetChild(2).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
            if (data.canAirBattle && !data.isAir) BaseData_Three.GetChild(4).GetComponent<TMP_Text>().fontStyle = FontStyles.Underline;
        }
        if (data.activeArea > 0) ActiveArea.text = data.activeArea.ToString();
        else ActiveArea.gameObject.SetActive(false);
        if (data.passiveArea > 0) PassiveArea.text = data.passiveArea.ToString();
        else PassiveArea.gameObject.SetActive(false);
    }
}
