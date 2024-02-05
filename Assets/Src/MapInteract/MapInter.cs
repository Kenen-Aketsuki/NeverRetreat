using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInter : MonoBehaviour
{
    Vector3Int MousePos
    {
        get
        {
            return FixGameData.FGD.InteractMap.WorldToCell(FixGameData.FGD.CameraNow.gameObject.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition)) + new Vector3Int(0, 0, 10);
        }
    }

    private void OnMouseDown()
    {
        Map.UpdateZOC();

        Debug.Log(MousePos);
        for (int i = 1; i < 7; i++)
        {
            Debug.Log("���������������� " + i + " ��������������\n�˴��ѵ�Ϊ" + Map.GetHereStack(MousePos,GameManager.GM.ActionSide));

        }
    }
}
