using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class TurnData
{
    //�غϱ��
    public short TurnNo { get; private set; }
    //���෽���ڵ㼤����
    public short MaxActiveBarrierAmmount { get; private set; }
    //���෽��Ա��
    public short MobilizationRate { get; private set; }
    //���෽Ԥ��������
    public int PreTrainedAmount { get; private set; }

    //�����������϶������
    public short MaxActiveFissureAmmount { get; private set; }
    //�����������϶������
    public int CrashBundith { get; private set; }
    
    //���غ���ʼ�׶�
    public TurnStage StartStage { get; private set; }
    //���෽֧Ԯ�б�
    public List<Tuple<string,string,int>> HumanReinforceList = new List<Tuple<string,string,int>>();
    //������֧Ԯ�б�
    public List<Tuple<string, string,int>> CrashReinforceList = new List<Tuple<string, string,int>>();

    public TurnData(XmlNode root)
    {
        TurnNo = short.Parse(root.Attributes["No"].Value);

        XmlNode tmp = root.SelectSingleNode("MaxActiveBarrierAmmount");
        if (tmp != null) MaxActiveBarrierAmmount = short.Parse(tmp.InnerText);
        else MaxActiveBarrierAmmount = 0;

        tmp = root.SelectSingleNode("MaxActiveFissureAmmount");
        if (tmp != null) MaxActiveFissureAmmount = short.Parse(tmp.InnerText);
        else MaxActiveFissureAmmount = 0;

        tmp = root.SelectSingleNode("CrashBundith");
        if (tmp != null) CrashBundith = int.Parse(tmp.InnerText);
        else CrashBundith = 0;

        tmp = root.SelectSingleNode("StartStage");
        if (tmp != null) StartStage = (TurnStage)Enum.Parse(typeof(TurnStage), tmp.InnerText);
        else StartStage = TurnStage.Strategy;

        int tmpInt;
        tmp = root.SelectSingleNode("reinforceList-Human");
        if (tmp != null)
        {
            foreach(XmlNode node in tmp.ChildNodes)
            {
                if (node.Attributes["ResTurn"] != null) tmpInt = int.Parse(node.Attributes["ResTurn"].Value);
                else tmpInt = 0;

                HumanReinforceList.Add(new Tuple<string, string,int>(node.Attributes["TroopName"].Value, node.Attributes["EnterPlace"].Value,tmpInt));
            }
        }

        tmp = root.SelectSingleNode("reinforceList-Crash");
        if (tmp != null)
        {
            foreach (XmlNode node in tmp.ChildNodes)
            {
                if (node.Attributes["ResTurn"] != null) tmpInt = int.Parse(node.Attributes["ResTurn"].Value);
                else tmpInt = 0;
                CrashReinforceList.Add(new Tuple<string, string, int>(node.Attributes["TroopName"].Value, node.Attributes["EnterPlace"].Value, tmpInt));
            }
        }

        tmp = root.SelectSingleNode("MobilizationRate");
        if (tmp != null) MobilizationRate = short.Parse(tmp.InnerText);
        else MobilizationRate = -1;

        tmp = root.SelectSingleNode("TrainedTroopMount");
        if (tmp != null) PreTrainedAmount = int.Parse(tmp.InnerText);
        else PreTrainedAmount = -1;
    }
}
