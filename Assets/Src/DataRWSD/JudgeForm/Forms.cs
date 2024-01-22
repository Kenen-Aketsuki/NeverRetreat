using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

public abstract class Form
{
    //���þ����ѹ���洢
    List<Tuple<string, int, int>> ColumIndex = new List<Tuple<string, int, int>>();//�洢�еļ�-��ʼλ-����

    List<string> DataSet = new List<string>();

    internal string getFromDataSet(int addr)
    {
        return DataSet[addr];
    }

    internal Tuple<string, int, int> getFromColumIndex(string key)
    {
        foreach(Tuple<string, int, int> tup in ColumIndex)
        {
            if (tup.Item1 == key) return tup;
        }
        return null;
    }

    internal Tuple<string, int, int> getFromColumIndex(int addr)
    {
        return ColumIndex[addr];
    }

    public Form(XmlNode root)
    {
        Dictionary<string, string> rowdata = new Dictionary<string, string>();
        int startIndex = 0;
        string key;
        int length;
        foreach (XmlNode node in root.ChildNodes)
        {
            //��������
            rowdata.Clear();
            key = node.Attributes["Key"].Value;
            length = 0;

            foreach(XmlNode data  in node.ChildNodes)
            {
                rowdata.Add(data.Attributes["Key"].Value, data.InnerText);
                length++;
            }
            foreach (KeyValuePair<string,string> dta in rowdata.OrderBy(x => int.Parse(x.Key)))
            {
                DataSet.Add(dta.Value);
            }
            ColumIndex.Add(new Tuple<string, int, int>(key, startIndex, length));

            startIndex += length;
        }
    }
}

public class BattleJudgeForm : Form
{
    public BattleJudgeForm(XmlNode root) : base(root)
    {

    }

    public string getResult(double ATK,double DEF)
    {
        int Address;
        //������ʼ��ַ
        double rrk = Math.Round(Math.Pow((ATK / DEF), Math.Sign(ATK - DEF))) * 2 - 2;
        rrk *= Math.Sign(ATK - DEF);
        if (rrk > 3) rrk = Math.Round(rrk / 2) + 1;
        else if (rrk < -1) rrk = -1;
        else if (rrk > 10) rrk = 10;
        rrk++;
        //�����ַƫ��
        Address =  getFromColumIndex((int)rrk).Item2;
        Address += new Random().Next(0,7);

        return getFromDataSet(Address);
    }
}

public class AirBattleJudgeForm : Form
{
    public AirBattleJudgeForm(XmlNode root) : base(root)
    {
    }

    public string getResult(int ATK,int DEF)
    {
        int Addr;
        //������ʼ��ַ
        if (ATK < 1) ATK = 1;
        double rrk = DEF / ATK ;
        rrk = Math.Floor(rrk);
        if (rrk > 6) rrk = 6;
        Addr = getFromColumIndex((int)rrk).Item2;
        //�����ַƫ��
        Addr += new Random().Next(0, 5);

        return getFromDataSet(Addr);
    }
}

public class FireRankForm : Form
{
    public FireRankForm(XmlNode root) : base(root){}

    public string getData(string id,int dis)//���ƣ�����
    {
        if (getFromColumIndex(id) == null) return "-1";

        int move;
        //����ƫ����
        if(dis  < 10) move = dis - 1;
        else move = (int)Math.Floor((double)(dis / 2)) + 5;

        //�ж�ƫ�����Ƿ�Խ�磬�����ػ���ֵ
        if(move >= getFromColumIndex(id).Item3)
        {
            return (-1).ToString();
        }
        else
        {
            int addr;
            addr = getFromColumIndex(id).Item2;
            addr += move;
            return getFromDataSet(addr);
        }
        
    }

}

public class FireStrikeJudgeForm : Form
{
    public FireStrikeJudgeForm(XmlNode root) : base(root){}

    public string getResult(string fireRank)
    {
        int addr;
        int rank = int.Parse(fireRank);
        if (rank < 2) rank = 2;
        addr = getFromColumIndex(rank - 2).Item2;

        addr += new Random().Next(0, 5);

        return getFromDataSet(addr);
    }
}
