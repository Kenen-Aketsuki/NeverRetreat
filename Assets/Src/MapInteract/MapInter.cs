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
        //防止与UI交互时误触地图
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Debug.Log(MousePos);
        //FixGameData.FGD.ZoneMap.ClearAllTiles();
        //FixGameData.FGD.ZoneMap.SetTile(MousePos, FixSystemData.GlobalZoneList["ZOC"].Top);
        //for (int i = 1; i < 7; i++)
        //{
        //    Debug.Log("方向：" + i + " — 移动力：" + Map.GetNearMov(MousePos, i, GameManager.GM.ActionSide));
        //}

        //return;

        FixGameData.FGD.ZoneMap.ClearAllTiles();

        List<CellInfo> Path = Map.AStarPathSerch(Vector3Int.zero, MousePos, 1000);
        if (Path == null) Debug.Log("寻路失败");
        else
        {
            Debug.Log("—————以下为路径—————");
            for (int i = 0; i < Path.Count; i++)
            {
                Debug.Log(i+" -> " + Path[i].Positian);
                FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
            }
            Debug.Log("花费移动力：" + Path[Path.Count - 1].usedCost);
        }
    }
}
