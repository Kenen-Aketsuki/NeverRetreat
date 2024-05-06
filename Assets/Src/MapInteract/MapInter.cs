using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        if (EventSystem.current.IsPointerOverGameObject() || Input.GetKey(KeyCode.Mouse1)) return;

        //Debug.Log(MousePos * new Vector3Int(-1, 1, 0) + new Vector3Int(21, 21));
        //Debug.Log(MousePos);

        //һ����Դ���
        #region
        //if (!FixGameData.FGD.ZoneMap.HasTile(MousePos)) goto DigTest;
        //else goto PathRev;

        //for (int i = 1; i < 7; i++)
        //{
        //    Debug.Log(Map.GetNearMov(MousePos, i,GameManager.GM.ActionSide) +" ��> "+i);
        //}
        //FixGameData.FGD.ZoneMap.ClearAllTiles();
        //FixGameData.FGD.ZoneMap.SetTile(MousePos, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        //return;

        //����A* �㷨
        //List<CellInfo> Path = Map.AStarPathSerch(Vector3Int.zero, MousePos, 20);
        //if (Path == null) Debug.Log("Ѱ·ʧ��");
        //else
        //{
        //    Debug.Log("��������������Ϊ·������������");
        //    for (int i = 0; i < Path.Count; i++)
        //    {
        //        Debug.Log(i + " -> " + Path[i].Positian);
        //        FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        //    }
        //    Debug.Log("�����ƶ�����" + Path[Path.Count - 1].usedCost);
        //}

        //    DigTest:
        //    FixGameData.FGD.ZoneMap.ClearAllTiles();
        //    //����Dijkstra�㷨
        //    area = Map.DijkstraPathSerch(MousePos, 10);
        //    Debug.Log(area.Count);
        //    foreach(KeyValuePair<Vector3Int, CellInfo> kvp in area)
        //    {
        //        if(kvp.Value.moveCost != float.PositiveInfinity) FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["ZOC"].Top);
        //        else FixGameData.FGD.ZoneMap.SetTile(kvp.Key, FixSystemData.GlobalZoneList["StaticBarrier"].Top);
        //    }
        //    return;
        //PathRev:
        //    //Dijkstra·������
        //    List<CellInfo> Path = Map.DijkstraPathReverse(area, MousePos);
        //    FixGameData.FGD.ZoneMap.ClearAllTiles();
        //    for (int i = 0; i < Path.Count; i++)
        //    {
        //        Debug.Log(i + " -> " + Path[i].Positian);
        //        FixGameData.FGD.ZoneMap.SetTile(Path[i].Positian, FixSystemData.GlobalZoneList["ZOC"].Top);
        //    }

        //�������������㷨
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
                {
                    OB_Piece.needChenkVisibility.Add(MousePos);
                    OB_Piece.needChenkVisibility.Add(GameManager.GM.currentPosition);
                    GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
                    OB_Piece.CheckVisibility();
                    (FixGameData.FGD.uiManager.actUI as IUIHandler).UpdateShow();
                    UpdateForcuse();

                    (FixGameData.FGD.uiManager.actUI as IUIHandler).OnPositionSelect(MousePos);
                }
                break;
            case MachineState.WaitForceMoveTarget:
                if (FixGameData.FGD.MoveAreaMap.GetTile(MousePos) != null && GameManager.GM.currentPiece.ForceMoveTo(MousePos))
                {
                    OB_Piece.needChenkVisibility.Add(MousePos);
                    OB_Piece.needChenkVisibility.Add(GameManager.GM.currentPosition);
                    GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
                    OB_Piece.CheckVisibility();
                    (FixGameData.FGD.uiManager.actUI as IUIHandler).UpdateShow();
                    UpdateForcuse();

                    (FixGameData.FGD.uiManager.actUI as IUIHandler).OnPositionSelect(MousePos);
                }
                break;
            case MachineState.FocusOnPiece:
                //if( GameManager.GM.currentPiece.PrepareMove())
                //{
                //    OB_Piece.needChenkVisibility.Add(GameManager.GM.currentPiece.piecePosition);
                //    GameManager.GM.SetMachineState(MachineState.WaitMoveTarget);
                //}
                //break;
            case MachineState.FocusOnTerrain:
            case MachineState.WaitForcuse:
            case MachineState.Idel:
                UpdateForcuse();

                GameManager.GM.SetMachineState(MachineState.WaitForcuse);
                break;
            case MachineState.ActiveSpecialFac:
                if(FixGameData.FGD.ZoneMap.GetTile(MousePos) != null)
                {
                    FixGameData.FGD.SpecialFacilityList.Find(x => x.Positian == MousePos).ChangeActive();
                }
                
                break;
            case MachineState.RecoverTroop:
                UpdateForcuse();
                break;
            case MachineState.SelectEventPosition:

                (FixGameData.FGD.uiManager.actUI as IUIHandler).OnPositionSelect(MousePos);

                GameManager.GM.SetMachineState(MachineState.Idel);
                break;
            case MachineState.TestOnly:
                //��������
                //HttpConnect.instance.SendBattleFieldEnv(GameManager.GM.currentPiece);

                //GameUtility.Save = "TestSave";
                //GameUtility.saveData = new SaveData("TestSave", "PVP");
                //GameUtility.������Ϸ();

                //Debug.Log(FixGameData.FGD.resultMem.FinalScore());

                for(int i = 0; i < 4; i++)
                {
                    Debug.Log((SpecialEvent)i);
                }

                break;
            default:
                //HttpConnect.instance.UpdatePieceKey();
                
                
                Debug.Log("δ֪��״̬:" + GameManager.GM.GetMachineState());
                break;
        }

    }
    //���ù�ע
    void UpdateForcuse()
    {
        if (MousePos == GameManager.GM.currentPosition) return;
        else GameManager.GM.currentPosition = MousePos;
        List<GameObject> PieceLst = FixGameData.FGD.HumanPiecePool.getChildByPos(MousePos);
        if(PieceLst.Count == 0) PieceLst = FixGameData.FGD.CrashPiecePool.getChildByPos(MousePos);

        List<LandShape> LandLst = Map.GetPLaceInfo(MousePos, 0);

        FixGameData.FGD.uiIndex.scrollView.UpdateCellChilds(PieceLst,LandLst);

    }

}
