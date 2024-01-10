using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PieseTextShow : MonoBehaviour
{
    Transform ParPiece;

    [SerializeField]
    TMP_Text BaseData;
    [SerializeField]
    TMP_Text TroopName;
    [SerializeField]
    TMP_Text ActiveArea;
    [SerializeField]
    TMP_Text PassiveArea;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = ParPiece.position;
    }

    public void setParPice(Transform pp)
    {
        ParPiece = pp;
    }

    public void InitText(Piece data,bool isThree)//棋子信息、是否为三元
    {
        TroopName.text = data.PDesignation;
        
        if (isThree)
        {
            string ATK;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();
                BaseData.text = ATK + " - " + data.MOV;
        }
        else
        {
            string ATK;
            string DEF;
            string MOV;
            if (data.ATK < 0) ATK = "X"; else ATK = data.ATK.ToString();
            if (data.DEF < 0) DEF = "X"; else DEF = data.DEF.ToString();
            if (data.MOV < 0) MOV = "X"; else MOV = data.MOV.ToString();
            BaseData.text = ATK + "-" + DEF + "-" + MOV;
        }
        if (data.activeArea > 0) ActiveArea.text = data.activeArea.ToString();
        else ActiveArea.gameObject.SetActive(false);
        if (data.passiveArea > 0) PassiveArea.text = data.passiveArea.ToString();
        else PassiveArea.gameObject.SetActive(false);
    }
}
