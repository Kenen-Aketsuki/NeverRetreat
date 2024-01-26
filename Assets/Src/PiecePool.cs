using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePool : MonoBehaviour
{
    int counter = 0;

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
    
    public List<Tuple<string,int,int>> getChildList()
    {
        List<Tuple<string, int, int>> childList = new List<Tuple<string, int, int>>();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            Vector3Int cpos = FixGameData.FGD.InteractMap.WorldToCell(child.position);
            childList.Add(new Tuple<string, int, int>(child.name, cpos.x, cpos.x));
        }
        //进行基数排序

        return childList;
    }

    void QuickSortPriKey(ref List<int> numList)//行坐标排序
    {
        //快速排序
        int anix;//快排轴枢
        int anixKey;//轴数值
        List<Tuple<int, int>> toSortQ = new List<Tuple<int, int>>();//待排序的片段起止队列
        toSortQ.Add(new Tuple<int, int>(0, numList.Count));//初始区间入队
                                                           //开始排序
        for (; toSortQ.Count > 0;)
        {
            //当前开始排序的片段出队
            Tuple<int, int> onSort = toSortQ[0];
            toSortQ.RemoveAt(0);

            //初始化
            int frontP = onSort.Item1;//前指针
            int backP = onSort.Item2;//后指针

            bool onReverse = false;//当前是否反向指针,false为从后往前,true 为从前往后

            anix = frontP;
            anixKey = numList[anix];
            for (; frontP != backP;)
            {
                int length = backP - frontP - 1;
                if (onReverse)
                {
                    if (forwardSerch(ref frontP, ref numList, anixKey, length))
                    {
                        numList[backP] = numList[frontP];
                        onReverse = false;
                    }
                }
                else
                {
                    if (backwardSerch(ref backP, ref numList, anixKey, length))
                    {
                        numList[frontP] = numList[backP];
                        onReverse = true;
                    }
                }
            }
            numList[backP] = anixKey;
            //一次完成,开始入队后续
            //左侧
            if (backP - onSort.Item1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(onSort.Item1, backP));
            }
            //右侧
            if (onSort.Item2 - backP - 1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(backP + 1, onSort.Item2));
            }

        }
    }

    bool forwardSerch(ref int pin, ref List<int> list, int anixKey, int length)//返回是否应该反向
    {
        pin += 1;
        int mem = pin;
        for (; pin - mem < length; pin++)
        {
            if (list[pin] > anixKey) return true;
        }
        return false;
    }

    bool backwardSerch(ref int pin, ref List<int> list, int anixKey, int length)//返回是否应该反向
    {
        pin -= 1;
        int mem = pin;
        for (; mem - pin < length; pin--)
        {
            if (list[pin] < anixKey) return true;
        }
        return false;
    }


}
