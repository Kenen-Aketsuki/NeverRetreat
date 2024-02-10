using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TestUI : MonoBehaviour
{

    public void AddPiece()
    {
        //BasicUtility.SpawnPiece("Crash.Incite",
        //    new Vector3Int(0, 0, 0), 
        //    null, 
        //    true);
        //Debug.Log("������һ������");

        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().UpdateChildPos("1\\DawIII.101", new Vector3Int(0, 0, 0));
        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().getChildByID("1\\DawIII.101").transform.position = new Vector3Int(0, 0, 0);
        FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().DelChildByID("1\\DawIII.101");
        Debug.Log("�ƶ���һ������");

        List<Tuple<string, int, int>> clist = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().childList;
        Dictionary<int, Tuple<int, int>> listIndex = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().listIndex;
        Debug.Log(clist.Count);
        foreach (KeyValuePair<int, Tuple<int, int>> piece in listIndex)
        {
            Debug.Log(piece.Key + "�ĳ���Ϊ: " + piece.Value.Item2 + "��ʼ����Ϊ: " + piece.Value.Item1);
        }

        List<GameObject> lst = FixGameData.FGD.HumanPieceParent.GetComponent<PiecePool>().getChildByPos(new Vector3Int(0, 0, 0));
        if (lst.Count != 0)
        {
            Debug.Log("�ڸô��ҵ�����,����Ϊ��" + lst[0].name);
        }
        else
        {
            Debug.Log("����ô��һ�������أ�");
        }
        
    }

    public void MovePiece()
    {
        GameManager.GM.currentPiece = FixGameData.FGD.HumanPiecePool.getChildByID("1\\DawIII.101").GetComponent<OB_Piece>();
        Map.UpdateZOC();
        GameManager.GM.SetMachineState(MachineState.FocusOnPiece);
    }

    public void TestFunc1()
    {
        GameManager.GM.currentPiece.OverTurn();

    }

    public void TestFunc2()
    {
        
    }

    public void TestFunc3()
    {
        
    }

    private void Update()
    {
        
    }
}
