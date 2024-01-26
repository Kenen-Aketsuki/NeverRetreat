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
        //���л�������

        return childList;
    }

    void QuickSortPriKey(ref List<int> numList)//����������
    {
        //��������
        int anix;//��������
        int anixKey;//����ֵ
        List<Tuple<int, int>> toSortQ = new List<Tuple<int, int>>();//�������Ƭ����ֹ����
        toSortQ.Add(new Tuple<int, int>(0, numList.Count));//��ʼ�������
                                                           //��ʼ����
        for (; toSortQ.Count > 0;)
        {
            //��ǰ��ʼ�����Ƭ�γ���
            Tuple<int, int> onSort = toSortQ[0];
            toSortQ.RemoveAt(0);

            //��ʼ��
            int frontP = onSort.Item1;//ǰָ��
            int backP = onSort.Item2;//��ָ��

            bool onReverse = false;//��ǰ�Ƿ���ָ��,falseΪ�Ӻ���ǰ,true Ϊ��ǰ����

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
            //һ�����,��ʼ��Ӻ���
            //���
            if (backP - onSort.Item1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(onSort.Item1, backP));
            }
            //�Ҳ�
            if (onSort.Item2 - backP - 1 > 1)
            {
                toSortQ.Add(new Tuple<int, int>(backP + 1, onSort.Item2));
            }

        }
    }

    bool forwardSerch(ref int pin, ref List<int> list, int anixKey, int length)//�����Ƿ�Ӧ�÷���
    {
        pin += 1;
        int mem = pin;
        for (; pin - mem < length; pin++)
        {
            if (list[pin] > anixKey) return true;
        }
        return false;
    }

    bool backwardSerch(ref int pin, ref List<int> list, int anixKey, int length)//�����Ƿ�Ӧ�÷���
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
