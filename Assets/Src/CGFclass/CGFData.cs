using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class CGFDataJson
{
    //��������
    public string pieceId;
    public string pieceType;
    public Vector3Int pos;
    public int inCasualty;
    public int Mov;
    public int ATK;
    public int DEF;
    public Vector3Int targetPos;
    //���θ���
    //�洢���ε�ս����������ս���������ƶ�����
    public List<LandDataAI> LandData;
    ////����о�����
    //public List<RoughEnyData> RoughEnyDatas;
    ////�ٽ��о�����
    //public List<AccuretEnyData> NearEnyDatas;
    //�����ܵо�����
    public List<AccuretEnyData> HighEffectDatas;
    //�Ѿ�������
    public List<AccuretEnyData> FriendDatas;


    public CGFDataJson(OB_Piece piece,Vector3Int targetPos)
    {
        Piece pieceData = piece.getPieceData();

        //��������
        pieceId = piece.gameObject.name;
        pieceType = pieceData.PieceID;
        pos = piece.piecePosition;
        inCasualty = new Regex("����").IsMatch(piece.getPieceData().HealthStr) ? 0 : 1;
        Mov =pieceData.MOV;
        ATK =pieceData.ATK;
        DEF =pieceData.DEF;
        this.targetPos = targetPos;

        //���θ�����
        LandData = new List<LandDataAI>();
        for(int i = 0; i < 7; i++)
        {
            int rrk = Map.GetBattleRRK(Map.GetRoundSlotPos(pos, i), ArmyBelong.ModCrash, 1);
            float atk = Map.GetTargetATK(pos, i, ArmyBelong.ModCrash, ATK);
            float def = Map.GetTargetDEFK(Map.GetRoundSlotPos(pos, i), ArmyBelong.ModCrash, DEF);
            float mov = Map.GetNearMov(pos, i, ArmyBelong.ModCrash);

            LandData.Add(new LandDataAI(rrk, atk, def, mov, i));
        }

        //if(pieceData.canDoMagic || 
        //    pieceData.canFixMod || 
        //    pieceData.canMental ||
        //    pieceData.canStrike)
        //{
        //    HighTechScann();
        //}
        //else
        //{
        //    NormalScann();
        //}

        HighTechScann();

        FriendScann();
    }

    public CGFDataJson(string pieceId, string pieceType, Vector3Int pos)
    {
        this.pieceId = pieceId;
        this.pieceType = pieceType;
        this.pos = pos;
    }

    public static string CreateJson(CGFDataJson jsonObj)
    {
        string jsonPayload = JsonConvert.SerializeObject(jsonObj);

        return jsonPayload;
    }

    //void NormalScann()
    //{
    //    //����о�������
    //    RoughEnyDatas = new List<RoughEnyData>();
    //    List<CellInfo> Area = Map.PowerfulBrickAreaSearch(pos, 5).Where(x => Map.HexDistence(x.Positian, pos) > 1).ToList();
    //    foreach (CellInfo cell in Area)
    //    {
    //        List<GameObject> enyLst = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
    //        if (enyLst.Count() == 0) continue;
    //        int i = 0;
    //        while (RoughEnyDatas.Count() < 5 && i < enyLst.Count())
    //        {
    //            RoughEnyData data = RoughEnyData.setData(pos, cell.Positian);
    //            RoughEnyDatas.Add(data);
    //            i++;
    //        }
    //    }
    //    while (RoughEnyDatas.Count() < 5)
    //    {
    //        RoughEnyData data = RoughEnyData.setEmpty();
    //        RoughEnyDatas.Add(data);
    //    }

    //    //�ٽ��о�������
    //    NearEnyDatas = new List<AccuretEnyData>();
    //    Area = Map.PowerfulBrickAreaSearch(pos, 1);
    //    foreach (CellInfo cell in Area)
    //    {
    //        List<GameObject> enyLst = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
    //        if (enyLst.Count() == 0) continue;
    //        int i = 0;
    //        while (NearEnyDatas.Count() < 6 && i < enyLst.Count())
    //        {
    //            AccuretEnyData data = AccuretEnyData.setData(enyLst[i].GetComponent<OB_Piece>(), pos);
    //            NearEnyDatas.Add(data);
    //            i++;
    //        }
    //    }
    //    while (NearEnyDatas.Count() < 6)
    //    {
    //        AccuretEnyData data = AccuretEnyData.setEmpty();
    //        NearEnyDatas.Add(data);
    //    }
    //}

    void HighTechScann()
    {
        //�����ܵо�������
        HighEffectDatas = new List<AccuretEnyData>();
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(pos, 10);
        foreach (CellInfo cell in Area)
        {
            List<GameObject> enyLst = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
            if (enyLst.Count() == 0) continue;
            int i = 0;
            while (HighEffectDatas.Count() < 6 && i < enyLst.Count())
            {
                AccuretEnyData data = AccuretEnyData.setData(enyLst[i].GetComponent<OB_Piece>(), pos);
                HighEffectDatas.Add(data);
                i++;
            }
        }
        while (HighEffectDatas.Count() < 6)
        {
            AccuretEnyData data = AccuretEnyData.setEmpty();
            HighEffectDatas.Add(data);
        }
    }

    void FriendScann()
    {
        //�����ܵо�������
        FriendDatas = new List<AccuretEnyData>();
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(pos, 5).Where(x=>x.Positian != pos).ToList();
        foreach (CellInfo cell in Area)
        {
            List<GameObject> enyLst = GameManager.GM.ActionPool.getChildByPos(cell.Positian);
            if (enyLst.Count() == 0) continue;
            int i = 0;
            while (FriendDatas.Count() < 6 && i < enyLst.Count())
            {
                AccuretEnyData data = AccuretEnyData.setData(enyLst[i].GetComponent<OB_Piece>(), pos);
                FriendDatas.Add(data);
                i++;
            }
        }
        while (FriendDatas.Count() < 6)
        {
            AccuretEnyData data = AccuretEnyData.setEmpty();
            FriendDatas.Add(data);
        }
    }
}

public class LandDataAI
{
    public int dir;
    public int RRK;
    public float ATK;
    public float DEF;
    public float MOV;

    public LandDataAI(int rRK, float aTK, float dEF, float mOV, int dir)
    {
        RRK = rRK;
        ATK = aTK;
        DEF = dEF;
        MOV = mOV;
        this.dir = dir;
    }
}

public class RoughEnyData
{
    public int direction;
    public int distence;

    public static RoughEnyData setData(Vector3Int pos,Vector3Int dir)
    {
        RoughEnyData data = new RoughEnyData();
        data.direction = Map.HexDirectionInt(pos,dir);
        data.distence = Map.HexDistence(pos, dir);
        return data;
    }

    public static RoughEnyData setEmpty()
    {
        RoughEnyData data = new RoughEnyData();
        data.direction = -1;
        data.distence = -1;
        return data;
    }
}

public class AccuretEnyData
{
    public string pieceType;
    public int direction;
    public int distance;
    public int inCasualty;
    public float DEF;

    public static AccuretEnyData setData(OB_Piece piece,Vector3Int pos)
    {
        AccuretEnyData data = new AccuretEnyData();
        data.direction = Map.HexDirectionInt(pos, piece.piecePosition);
        data.distance = Map.HexDistence(pos, piece.piecePosition);
        data.pieceType = piece.getPieceData().PieceID;
        data.inCasualty = new Regex("����").IsMatch(piece.getPieceData().HealthStr) ? 1 : 0;
        data.DEF = piece.getPieceData().DEF;

        return data;
    }

    public static AccuretEnyData setEmpty()
    {
        AccuretEnyData data = new AccuretEnyData();
        data.pieceType = "N/A";
        data.direction = -1;
        return data;
    }
}

public class BackwardData // ���򴫲�������
{
    public string pieceId;//����ID
    public int distance; // ����Ŀ������
    public int inCasualty; // �Ƿ��ش�
    public int dealDmg; // ����˺�
    public int enemyCount;//�ܱߵ�������
    public bool commandState;//ָ��״��
    public bool isSpecial;//�Ƿ�Ϊ���ⵥλ

    public BackwardData(OB_Piece pic,int dealDmg,bool commandState)
    {
        this.commandState = commandState;
        this.dealDmg = dealDmg;

        pieceId = pic.gameObject.name;
        distance = Map.HexDistence(pic.piecePosition, GameManager.GM.ActionTargetPos);
        inCasualty = Regex.Match(pic.getPieceData().HealthStr, ".*����.*").Success ? 0 : 1;
        isSpecial = pic.isSpecialPiece;

        enemyCount = 0;
        foreach (CellInfo cell in Map.PowerfulBrickAreaSearch(pic.piecePosition, 10))
        {
            enemyCount += GameManager.GM.EnemyPool.getChildByPos(cell.Positian).Count;
        }
    }

    public string CreateJson()
    {
        string jsonPayload = JsonConvert.SerializeObject(this);

        return jsonPayload;
    }
}