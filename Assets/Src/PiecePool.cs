using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PiecePool : MonoBehaviour
{
    int counter = 0;
    
    //子对象列表
    public List<Tuple<string, int, int>> childList = new List<Tuple<string, int, int>>();
    //行目录
    public Dictionary<int, Tuple<int, int>> listIndex = new Dictionary<int, Tuple<int, int>>();

    public string getRedomNo()
    {
        counter++;
        return counter.ToString() + "\\";
    }

    public GameObject getChildByID(string ID)
    {
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            if(gameObject.transform.GetChild(i).name == ID)
            {
                return gameObject.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }

    public List<GameObject> getChildByPos(Vector3Int Pos)
    {
        List<GameObject> lst = new List<GameObject>();
        for (int i = listIndex[Pos.y].Item1; i < listIndex[Pos.y].Item2 + listIndex[Pos.y].Item1; i++)
        {
            if (childList[i].Item2 == Pos.x)
            {
                lst.Add(getChildByID(childList[i].Item1));
            }
        }

        return lst;
    }

    //有序加入子对象
    public void AddChildInOrder(string childName,Vector3Int Pos)
    {
        Tuple<int, int> addr;
        if(!listIndex.TryGetValue(Pos.y, out addr))
        {
            bool ok = false;
            for(int i = GameUtility.columRange.Item2; i > GameUtility.columRange.Item1; i--)
            {
                if (listIndex.ContainsKey(i) && i < Pos.y && !ok)
                {
                    listIndex.Add(Pos.y, new Tuple<int, int>(listIndex[i].Item1 + listIndex[i].Item2, 1));
                    ok = true;
                    addr = listIndex[Pos.y];
                }
                else if (listIndex.ContainsKey(i) && i > Pos.y)
                {
                    listIndex[i] = new Tuple<int, int>(listIndex[i].Item1 + 1, listIndex[i].Item2);
                }
            }
            listIndex = listIndex.OrderBy(x => x.Key).ToDictionary(x => x.Key, p => p.Value);

        }
        else
        {
            listIndex[Pos.y] = new Tuple<int, int>(addr.Item1, addr.Item2 + 1);
            for (int i = GameUtility.columRange.Item2; i > GameUtility.columRange.Item1; i--)
            {
                if (listIndex.ContainsKey(i) && i > Pos.y)
                {
                    listIndex[i] = new Tuple<int, int>(listIndex[i].Item1 + 1, listIndex[i].Item2);
                }
            }
        }
        childList.Insert(addr.Item1, new Tuple<string, int, int>(childName, Pos.x, Pos.y));
    }
    //无序加入子对象
    public void AddChildNoOrder(string childName, Vector3Int Pos)
    {
        childList.Add(new Tuple<string, int, int>(childName, Pos.x, Pos.y));
    }
    //当棋子改变位置时更新字元素列表
    public void UpdateChildList(string ID, Vector3Int oldPos, Vector3Int newPos)
    {
        for (int i = listIndex[oldPos.y].Item1; i < listIndex[oldPos.y].Item2 + listIndex[oldPos.y].Item1; i++)
        {
            if (childList[i].Item1 == ID)
            {
                childList.RemoveAt(i);
            }
        }
        AddChildInOrder(ID, newPos);
    }
}
