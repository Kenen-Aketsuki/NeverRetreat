using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Map
{
    //��ͼ�������
    //��ȡĳ���ܱߵĸ���,�����Ͽ�ʼ˳ʱ��1~6��ţ�0��������
    static Vector3Int GetRoundSlotPos(Vector3Int Pos, int direction)
    {
        Vector3Int endPos;
        int y = Math.Abs(Pos.y);
        switch (direction)
        {
            case 0:
                endPos = Pos;
                break;
            case 1:
                endPos = new Vector3Int(Pos.x + y % 2, Pos.y - 1, Pos.z);
                break;
            case 2:
                endPos = new Vector3Int(Pos.x + 1, Pos.y, Pos.z);
                break;
            case 3:
                endPos = new Vector3Int(Pos.x + y % 2, Pos.y + 1, Pos.z);
                break;
            case 4:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y + 1, Pos.z);
                break;
            case 5:
                endPos = new Vector3Int(Pos.x - 1, Pos.y, Pos.z);
                break;
            case 6:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y - 1, Pos.z);
                break;
            default:
                endPos = Vector3Int.zero;
                break;
        }
        return endPos;
    }
    //��ȡ��Ԫ�ص�λ�á���Ԫ��Ϊ��Ŀ���ͼ���롰λ�á�
    static Tuple<int, Vector3Int> GetSideAddr(Vector3Int Pos, int direction)
    {
        int map;
        int y = Math.Abs(Pos.y);
        Vector3Int endPos;
        switch (direction)
        {
            case 1:
                endPos = Pos;
                map = 0;
                break;
            case 2:
                endPos = Pos;
                map = 1;
                break;
            case 3:
                endPos = Pos;
                map = 2;
                break;
            case 4:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y + 1, Pos.z);
                map = 0;
                break;
            case 5:
                endPos = new Vector3Int(Pos.x - 1, Pos.y, Pos.z);
                map = 1;
                break;
            case 6:
                endPos = new Vector3Int(Pos.x - (y + 1) % 2, Pos.y - 1, Pos.z);
                map = 2;
                break;
            default:
                endPos = Vector3Int.zero;
                map = 0;
                break;
        }

        return new Tuple<int, Vector3Int>(map, endPos);
    }

    //��ȡ���ڸ��ӵ�����,���뵱ǰλ�á�����
    //�б�˳��0 �������� - 1 ���� - 2 ��· - 3 ������ʩ - 4 �����ʩ - 5 ���������� - 6 ����������� - 7 ������ - 8 �������
    public static List<LandShape> GetNearInfo(Vector3Int Pos,int Dir)
    {
        List<LandShape> lst = new List<LandShape>();
        Vector3Int targetPos = GetRoundSlotPos(Pos, Dir);
        Tuple<int, Vector3Int> sidePos = GetSideAddr(Pos, Dir);
        TileBase tmpName;

        //��������
        tmpName = FixGameData.FGD.MapList[0].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name]);
        else lst.Add(null);
        //����
        tmpName = FixGameData.FGD.MapList[1 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalBasicTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //��·
        tmpName = FixGameData.FGD.MapList[4 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //������ʩ
        tmpName = FixGameData.FGD.MapList[7].GetTile(targetPos);
        if (tmpName != null && FixSystemData.GlobalFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name]);
        else if(tmpName != null && FixSystemData.GlobalSpFacilityList.ContainsKey(tmpName.name)) lst.Add(FixSystemData.GlobalSpFacilityList[tmpName.name]);
        else lst.Add(null);
        //�����ʩ
        tmpName = FixGameData.FGD.MapList[8 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalFacilityList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //����������
        tmpName = FixGameData.FGD.MapList[11 + sidePos.Item1].GetTile(sidePos.Item2);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name.Split("_")[0]]);
        else lst.Add(null);
        //�����������
        tmpName = FixGameData.FGD.MapList[14].GetTile(targetPos);
        if (tmpName != null) lst.Add(FixSystemData.GlobalSpecialTerrainList[tmpName.name]);
        else lst.Add(null);

        //��ȡ������
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 0));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);
        //��ȡ�������
        tmpName = FixGameData.FGD.ZoneMap.GetTile(new Vector3Int(targetPos.x, targetPos.y, 1));
        if (tmpName != null) lst.Add(FixSystemData.GlobalZoneList[tmpName.name]);
        else lst.Add(null);

        return lst;
    }
    //��ȡ�ٽ��ƶ������ģ����뵱ǰλ�úͷ���
    public static float GetNearMov(Vector3Int Pos, int Dir , ArmyBelong ActionSide)
    {
        float Mov = -1;
        float MovAdd = 0;
        float MovMULTY = 1;
        List<LandShape> Lands = GetNearInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            //ͳ�ƹ���
            if (Mov < 0 || Land.enterCount == -1) Mov = Land.enterCount;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ADD) MovAdd += Land.MOV_All.Item2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.ALL) Mov = -2;
            else if (Land.MOV_All != null && Land.MOV_All.Item1 == FixWay.NOPE) Mov = -1;
            //ͳ����Ӫ
            if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.MULTY) MovMULTY *= Land.MOV_All.Item2;
            else if (Land.ATK_IFF(ActionSide) != null && Land.ATK_IFF(ActionSide).Item1 == FixWay.ADD) MovAdd += Land.MOV_All.Item2;
        }
        if(Mov < 0 ) return Mov;
        else return Math.Max((Mov + MovAdd) , 1) * MovMULTY;
    }
    //����ܷ����������
    static bool canSetZoc(Vector3Int Pos, int Dir)
    {
        bool canset = true;
        List<LandShape> Lands = GetNearInfo(Pos, Dir);

        for (int i = 0; i < Lands.Count; i++)
        {
            LandShape Land = Lands[i];
            if (Land == null) continue;
            if(!Land.canZoc)
            {
                canset = false;
                break;
            }
        }
        
        return canset;
    }

    //λ�á��ƶ������߶ȡ��ѵ����ܷ��Խ��ÿ��������Ƿ���Ҫȫ��
    public static Tuple<Vector3Int, float, int, int, bool, bool> GetNextSlotWithData(Vector3Int Pos,int direction)//������Ϊ�Ǵ�Pos��Ӧλ����ĳ�������ƶ�
    {
        float Mov = -1;
        int height = 0;
        int Stk = 2;
        bool canZoc = true;
        bool AllMovNeed = false;

        float MovAdd = 0;
        float MovMULTY = 1;

        string tmpStr;
        //��ȡ������
        Tuple<int, Vector3Int> tmpTup = GetSideAddr(Pos, direction);
        //��ȡĿ������
        Vector3Int endPos = GetRoundSlotPos(Pos, direction);

        //�����������
        if (FixGameData.FGD.MapList[0].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[0].GetTile(endPos).name;
            BasicLandShape tmpLand = FixSystemData.GlobalBasicTerrainList[tmpStr];
            Mov = tmpLand.enterCount;
            height = tmpLand.height;
            if (!tmpLand.canZoc) canZoc = false;
        }
        

        //�������
        if (FixGameData.FGD.MapList[1+tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            AllMovNeed = true;
            canZoc = false;
        }

        //�����·
        if (FixGameData.FGD.MapList[4 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[4 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];
            height = tmpLand.height;
            if (Mov == -1) Mov = tmpLand.enterCount;
            else MovAdd += tmpLand.MOV_All.Item2;
        }


        //���������ʩ
        if (FixGameData.FGD.MapList[7].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[7].GetTile(endPos).name;
            Facility tmpLand = null;
            FixSystemData.GlobalFacilityList.TryGetValue(tmpStr, out tmpLand);
            if(tmpLand != null)
            {
                height = tmpLand.height;
                if (canZoc) canZoc = tmpLand.canZoc;
                if (tmpLand.STK_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) Stk += (int)tmpLand.STK_All.Item2;
                //���㹫����
                if (Mov == -1) Mov = tmpLand.enterCount;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

                //������Ӫ��
                if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
            }
        }
        //������ʩ
        if (FixGameData.FGD.MapList[7].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[7].GetTile(endPos).name;
            SpecialFacility tmpLand = null;
            FixSystemData.GlobalSpFacilityList.TryGetValue(tmpStr, out tmpLand);
            if (tmpLand != null)
            {
                height = tmpLand.height;
                if (canZoc) canZoc = tmpLand.canZoc;
                if (tmpLand.STK_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) Stk += (int)tmpLand.STK_All.Item2;
                //���㹫����
                if (Mov == -1) Mov = tmpLand.enterCount;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

                //������Ӫ��
                if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
                else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
            }
        }

        //��������ʩ
        if (FixGameData.FGD.MapList[8 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[8 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];
            
            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //���㹫����
            if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //������Ӫ��
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }

        //�������������
        if (FixGameData.FGD.MapList[11 + tmpTup.Item1].GetTile(tmpTup.Item2) != null)
        {
            tmpStr = FixGameData.FGD.MapList[11 + tmpTup.Item1].GetTile(tmpTup.Item2).name;
            Facility tmpLand = FixSystemData.GlobalSpecialTerrainList[tmpStr];

            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //���㹫����
            if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //������Ӫ��
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }
        //��������������
        if (FixGameData.FGD.MapList[14].GetTile(endPos) != null)
        {
            tmpStr = FixGameData.FGD.MapList[14].GetTile(endPos).name;
            Facility tmpLand = FixSystemData.GlobalFacilityList[tmpStr];

            height = tmpLand.height;
            if (canZoc) canZoc = tmpLand.canZoc;
            //���㹫����
            if (Mov == -1) Mov = tmpLand.enterCount;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.MOV_All != null && tmpLand.MOV_All.Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;

            //������Ӫ��
            if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.MULTY) MovMULTY *= tmpLand.MOV_All.Item2;
            else if (tmpLand.ATK_IFF(GameManager.GM.ActionSide) != null && tmpLand.ATK_IFF(GameManager.GM.ActionSide).Item1 == FixWay.ADD) MovAdd += tmpLand.MOV_All.Item2;
        }


        //λ�á��ƶ������߶ȡ��ѵ����ܷ��Խ��ÿ��������Ƿ��к���
        return new Tuple<Vector3Int, float, int, int, bool, bool>(endPos, Mov, height, Stk, canZoc, AllMovNeed);
    }

    //ˢ�¿�������δ����
    public static void UpdateZOC()
    {
        FixGameData.FGD.ZoneMap.ClearAllTiles();

        ArmyBelong EnemySide = (ArmyBelong)(((int)GameManager.GM.ActionSide + 1) % 2);
        PiecePool EnemyPool;
        if(EnemySide == ArmyBelong.Human)
        {
            EnemyPool = FixGameData.FGD.HumanPiecePool;
        }
        else
        {
            EnemyPool = FixGameData.FGD.CrashPiecePool;;
        }

        foreach(Tuple<string,int,int> piece in EnemyPool.childList)
        {
            Vector3Int pos = new Vector3Int(piece.Item2, piece.Item3, 0);
            FixGameData.FGD.ZoneMap.SetTile(pos, FixSystemData.GlobalZoneList["ZOC"].Top);

            for(int i = 1; i < 7; i++)
            {
                Vector3Int tmp = GetRoundSlotPos(pos, i);
                if (FixGameData.FGD.ZoneMap.GetTile(tmp) != null || !canSetZoc(pos, i)) continue;
                FixGameData.FGD.ZoneMap.SetTile(
                    tmp, 
                    FixSystemData.GlobalZoneList["ZOC"].Top);
            }
        }

    }
}
