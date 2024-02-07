using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        //��ֹ��UI����ʱ�󴥵�ͼ
        if (EventSystem.current.IsPointerOverGameObject()) return;

        FixGameData.FGD.ZoneMap.ClearAllTiles();
        List<CellInfo> Path = Map.AStarPathSerch(Vector3Int.zero, MousePos, 1000);
        if (Path == null) Debug.Log("Ѱ·ʧ��");
        else
        {
            for (int i = 0; i < Path.Count; i++)
            {
                FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
            }
            Debug.Log("�����ƶ�����" + Path[Path.Count - 1].usedCost);
        }
    }
}
