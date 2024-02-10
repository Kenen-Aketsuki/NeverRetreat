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
        //防止与UI交互时误触地图
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //Debug.Log(MousePos * new Vector3Int(-1, 1, 0) + new Vector3Int(21, 21));
        Debug.Log(MousePos);

        //一坨测试代码
        #region
        //if (!FixGameData.FGD.ZoneMap.HasTile(MousePos)) goto DigTest;
        //else goto PathRev;

        //for (int i = 1; i < 7; i++)
        //{
        //    Debug.Log(Map.GetNearMov(MousePos, i,GameManager.GM.ActionSide) +" —> "+i);
        //}
        //FixGameData.FGD.ZoneMap.ClearAllTiles();
        //FixGameData.FGD.ZoneMap.SetTile(MousePos, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        //return;

        //测试A* 算法
        //List<CellInfo> Path = Map.AStarPathSerch(Vector3Int.zero, MousePos, 20);
        //if (Path == null) Debug.Log("寻路失败");
        //else
        //{
        //    Debug.Log("—————以下为路径—————");
        //    for (int i = 0; i < Path.Count; i++)
        //    {
        //        Debug.Log(i + " -> " + Path[i].Positian);
        //        FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        //    }
        //    Debug.Log("花费移动力：" + Path[Path.Count - 1].usedCost);
        //}

        //    DigTest:
        //    FixGameData.FGD.ZoneMap.ClearAllTiles();
        //    //测试Dijkstra算法
        //    area = Map.DijkstraPathSerch(MousePos, 10);
        //    Debug.Log(area.Count);
        //    foreach(KeyValuePair<Vector3Int, CellInfo> kvp in area)
        //    {
        //        if(kvp.Value.moveCost != float.PositiveInfinity) FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["ZOC"].Top);
        //        else FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        //    }
        //    return;
        //PathRev:
        //    //Dijkstra路径回溯
        //    List<CellInfo> Path = Map.DijkstraPathReverse(area, MousePos);
        //    FixGameData.FGD.ZoneMap.ClearAllTiles();
        //    for (int i = 0; i < Path.Count; i++)
        //    {
        //        Debug.Log(i + " -> " + Path[i].Positian);
        //        FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        //    }

        //测试区域搜索算法
        //List<CellInfo> Area = Map.PowerfulBrickAreaSearch(MousePos, 20);
        //foreach(CellInfo Cell in Area)
        //{
        //    FixGameData.FGD.ZoneMap.SetTile(Cell.Positian, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        //}
        //FixGameData.FGD.ZoneMap.SetTile(MousePos, FixSystemData.GlobalZoneList["ZOC"].Top);
        #endregion
        switch (GameManager.GM.GetMachineState())
        {
            case MachineState.WaitMoveTarget:
                if( GameManager.GM.currentPiece.MoveTo(MousePos)) 
                    GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
                break;
            case MachineState.FocusOnPiece:
                if( GameManager.GM.currentPiece.PrepareMove()) 
                    GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
                break;
            case MachineState.Idel:
                area = Map.DijkstraPathSerch(MousePos, 10);
                Debug.Log(area.Count);
                foreach (KeyValuePair<Vector3Int, CellInfo> kvp in area)
                {
                    if (kvp.Value.moveCost != float.PositiveInfinity) FixGameData.FGD.MoveAreaMap.SetTile(kvp.Key, FixGameData.FGD.MoveArea);
                    else FixGameData.FGD.MoveAreaMap.SetTile(kvp.Key, FixGameData.FGD.MoveZocArea);
                }
                break;
            default:
                Debug.Log("未知的状态:" + GameManager.GM.GetMachineState());
                break;
        }

    }
}
