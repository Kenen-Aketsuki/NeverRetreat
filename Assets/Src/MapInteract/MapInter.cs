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

    Dictionary<Vector3Int, CellInfo> area;

    private void OnMouseDown()
    {
        //��ֹ��UI����ʱ�󴥵�ͼ
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Debug.Log(MousePos);
        if (!FixGameData.FGD.ZoneMap.HasTile(MousePos)) goto DigTest;
        else goto PathRev;

        //for(int i = 1; i < 7; i++)
        //{
        //    Debug.Log(Map.GetNearMov(MousePos, i, ArmyBelong.Human));
        //}
        //return;

        //����A*�㷨
        //List<CellInfo> Path = Map.AStarPathSerch(Vector3Int.zero, MousePos, 1000);
        //if (Path == null) Debug.Log("Ѱ·ʧ��");
        //else
        //{
        //    Debug.Log("��������������Ϊ·������������");
        //    for (int i = 0; i < Path.Count; i++)
        //    {
        //        Debug.Log(i+" -> " + Path[i].Positian);
        //        FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        //    }
        //    Debug.Log("�����ƶ�����" + Path[Path.Count - 1].usedCost);
        //}
    DigTest:
        FixGameData.FGD.ZoneMap.ClearAllTiles();
        //����Dijkstra�㷨
        area = Map.DijkstraPathSerch(MousePos, 5);
        Debug.Log(area.Count);
        foreach(KeyValuePair<Vector3Int, CellInfo> kvp in area)
        {
            if(kvp.Value.moveCost != float.PositiveInfinity) FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["ZOC"].Top);
            else FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        }
        return;
    PathRev:
        //Dijkstra·������
        List<CellInfo> Path = Map.DijkstraPathReverse(area, MousePos);
        FixGameData.FGD.ZoneMap.ClearAllTiles();
        for (int i = 0; i < Path.Count; i++)
        {
            Debug.Log(i + " -> " + Path[i].Positian);
            FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        }
    }
}
