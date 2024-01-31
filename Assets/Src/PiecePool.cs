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
    //获取棋子的随机(?)编号
    public string getRedomNo()
    {
        counter++;
        return counter.ToString() + "\\";
    }

    //以棋子ID为索引搜索棋子
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
    //以棋子位置为索引，搜索此位置上所有棋子
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

        if (!listIndex.TryGetValue(Pos.y, out addr))
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
            if (!ok)
            {
                listIndex.Add(Pos.y, new Tuple<int, int>(0, 1));
                addr = listIndex[Pos.y];
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
    public void UpdateChildPos(string ID, Vector3Int newPos)
    {
        DelChildByID(ID);

        AddChildInOrder(ID, newPos);
    }
    //当棋子被删除时更新子元素列表
    public void DelChildByID(string ID)
    {
        GameObject tar = getChildByID(ID);

        if (tar == null)
        {
            Debug.Log(gameObject.name + "未找到目标");
            return;
        }

        int posY = FixGameData.FGD.InteractMap.WorldToCell(tar.transform.position).y;
        Tuple<int, int> AddrT = listIndex[posY];
        int addr;

        //删除棋子位置信息
        for (int i = 0;i < AddrT.Item2; i++)
        {
            addr = i + AddrT.Item1;
            if(childList[addr].Item1 == ID)
            {
                childList.RemoveAt(addr);
                break;
            }
        }

        //修改被删减的目录
        AddrT = listIndex[posY];
        if (AddrT.Item2 == 1) listIndex.Remove(posY);
        else listIndex[posY] = new Tuple<int, int>(AddrT.Item1, AddrT.Item2 - 1);

        //修改其它目录
        for (int i = GameUtility.columRange.Item2; i > GameUtility.columRange.Item1; i--)
        {
            if (listIndex.ContainsKey(i) && i > posY)
            {
                listIndex[i] = new Tuple<int, int>(listIndex[i].Item1 - 1, listIndex[i].Item2);
            }
        }
    }

    //棋子跳边
    public static void ChangeSide(string ID,Vector3Int Pos,ArmyBelong old)
    {
        PiecePool oldSide;
        PiecePool newSide;
        if(old == ArmyBelong.Human)
        {
            oldSide = FixGameData.FGD.HumanPiecePool;
            newSide = FixGameData.FGD.CrashPiecePool;
        }
        else
        {
            oldSide = FixGameData.FGD.CrashPiecePool;
            newSide = FixGameData.FGD.HumanPiecePool;
        }

        newSide.AddChildInOrder(ID, Pos);
        oldSide.DelChildByID(ID);

    }
}
