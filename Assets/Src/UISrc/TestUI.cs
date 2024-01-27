using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    public void AddPiece()
    {
        //BasicUtility.SpawnPiece("Crash.Incite",
        //    new Vector3Int(0, 0, 0), 
        //    null, 
        //    true);
        //Debug.Log("������һ������");

        FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().UpdateChildList("1\\Crash.Blade", FixGameData.MapToWorld(35, 3), new Vector3Int(0, 0, 0));
        FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().getChildByID("1\\Crash.Blade").transform.position = new Vector3Int(0, 0, 0);
        Debug.Log("�ƶ���һ������");

        List<Tuple<string, int, int>> clist = FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().childList;
        Dictionary<int, Tuple<int, int>> listIndex = FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().listIndex;
        Debug.Log(clist.Count);
        foreach (KeyValuePair<int, Tuple<int, int>> piece in listIndex)
        {
            Debug.Log((piece.Key + 21) + "�ĳ���Ϊ: " + piece.Value.Item2 + "��ʼ����Ϊ: " + piece.Value.Item1);
        }

        List<GameObject> lst = FixGameData.FGD.CrashPieceParent.GetComponent<PiecePool>().getChildByPos(new Vector3Int(0, 0, 0));
        if (lst.Count != 0)
        {
            Debug.Log("�ڸô��ҵ�����,����Ϊ��" + lst[0].name);
        }
        else
        {
            Debug.Log("����ô��һ�������أ�");
        }
        
    }
}
