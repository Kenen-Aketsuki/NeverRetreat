using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TestUI : MonoBehaviour
{
    GameObject piece;

    int movDir = 1;
    int counter;

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
        FixGameData.FGD.CrashPiecePool.UpdateChildPos("1\\Crash.Blade", FixGameData.MapToWorld(21, 21));
        FixGameData.FGD.CrashPiecePool.getChildByID("1\\Crash.Blade").transform.position = FixGameData.MapToWorld(21, 21);
        
        piece = FixGameData.FGD.CrashPiecePool.getChildByID("1\\Crash.Blade");
        counter = 0;

       // FixGameData.FGD.MapList[7].SetTile(new Vector3Int(2, 2, 0), FixSystemData.GlobalFacilityList["DefenceArea"].Top);
       FixGameData.FGD.ZoneMap.ClearAllTiles();

    }

    public void TestFunc1()
    {

        counter = (counter + 1) % 6 + 1;
        FixGameData.FGD.ZoneMap.SetTile(FixGameData.FGD.InteractMap.WorldToCell(piece.transform.position), FixSystemData.GlobalZoneList["ZOC"].Top);
        //Debug.Log(Map.GetNextSlotWithData(FixGameData.FGD.InteractMap.WorldToCell(piece.transform.position), counter));


    }

    public void TestFunc2()
    {
        Vector3Int tmp = FixGameData.FGD.InteractMap.WorldToCell(piece.transform.position) + new Vector3Int(0, movDir, 0);
        
        FixGameData.FGD.CrashPiecePool.UpdateChildPos("1\\Crash.Blade", tmp);
        FixGameData.FGD.CrashPiecePool.getChildByID("1\\Crash.Blade").transform.position = FixGameData.FGD.InteractMap.CellToWorld(tmp);
        //if (FixGameData.FGD.HumanPiecePool.getChildByID(piece.name) != null) Debug.Log("���෽�ҵ�Ŀ��");
        //if (FixGameData.FGD.CrashPiecePool.getChildByID(piece.name) != null) Debug.Log("�������ҵ�Ŀ��");
    }

    public void TestFunc3()
    {
        Vector3Int tmp = FixGameData.FGD.InteractMap.WorldToCell(piece.transform.position) + new Vector3Int(movDir, 0, 0);

        FixGameData.FGD.CrashPiecePool.UpdateChildPos("1\\Crash.Blade", tmp);
        FixGameData.FGD.CrashPiecePool.getChildByID("1\\Crash.Blade").transform.position = FixGameData.FGD.InteractMap.CellToWorld(tmp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) movDir *= -1;
    }
}
