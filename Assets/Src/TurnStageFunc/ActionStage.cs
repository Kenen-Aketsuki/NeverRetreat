using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class ActionStage
{
    public static bool CommitFireStrick(Vector3Int Pos)
    {
        Action<int[], List<GameObject>> TakeAllDMG = (x, targetSet) =>
        {
            for(int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeDemage(x[i]);
            }
        };

        Action<int[], List<GameObject>> TakeAllStabel = (x, targetSet) =>
        {
            for (int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeUnstable(x[i]);
            }
        };

        //if (!canHit(Pos))
        //{
        //    FixGameData.FGD.uiIndex.HintUI.SetText("�����޷�����");
        //    FixGameData.FGD.uiIndex.HintUI.SetExitTime(3);
        //    return false;
        //}

        List<GameObject> targets = GameManager.GM.EnemyPool.getChildByPos(Pos);
        if (targets.Count == 0 || FixGameData.FGD.AttackAreaMap.GetTile(Pos) == null) return false;

        string fireRank = FixSystemData.fireRankForm.getData(GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2], Map.HexDistence(Pos, GameManager.GM.currentPosition));
        string[] resuSet = FixSystemData.fireStrikeJudgeForm.getResult(fireRank).Split("+");

        Action<int[], List<GameObject>> tarAct;


        foreach (string res in resuSet)
        {
            int dmg = int.Parse(res.Substring(0, res.Length - 1));

            int[] dmgSet = new int[targets.Count];
            for (int i = 0; i < dmg; i++)
            {
                dmgSet[UnityEngine.Random.Range(0, targets.Count)]++;
            }

            if (res.EndsWith("K")) tarAct = TakeAllDMG;
            else if (res.EndsWith("D")) tarAct = TakeAllStabel;
            else tarAct = null;

            if (tarAct != null) tarAct(dmgSet, targets);
        }

        Map.UpdatePieceStackSign();
        Map.UpdateZOC();
        return true;
    }

    static int getWeponVelocity(string nam = "")
    {
        if (nam == "") nam = GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2];
        int V = 0;
        switch (nam)
        {
            case "Artillery":
                V = 564;
                break;
            case "SP-Artillery":
                V = 930;
                break;
            case "Machine-Grope":
                V = 1000;
                break;
        }
        return V;
    }

    //public static bool canHit(Vector3Int Target)
    //{
    //    int Velocity = getWeponVelocity();
    //    //int Velocity = 564;

    //    if (Velocity == 0) return true;

    //    int HexDis = Map.HexDistence(Target, GameManager.GM.currentPosition);
    //    //int HexDis = Map.HexDistence(Target, Vector3Int.zero);
    //    int Distance = HexDis * 500;//���룬��λ:��
    //    float g = 9.8f;//�������ٶȣ���/��^2


    //    float aTheta = Mathf.Asin(Distance * g / Mathf.Pow(Velocity, 2)) / 2;


    //    if (aTheta < -1 || aTheta > 1) return false;

    //    float Vx = Velocity * Mathf.Cos(aTheta);
    //    float Vh = Velocity * Mathf.Sin(aTheta);

    //    List<CellInfo> Line = Map.LineSerch(GameManager.GM.currentPosition, Target);
    //    //List<CellInfo> Line = Map.LineSerch(Vector3Int.zero, Target);

    //    for (int i = 1; i < Line.Count - 1; i++)
    //    {
    //        //Debug.Log("�߶ȣ�" + (TanTheta * i * 500 - Mathf.Pow(i * 500, 2) * gCosTheta2V));
    //        float t = i * 500 / Vx;
    //        float h = Vh * t - 0.5f * g * Mathf.Pow(t, 2);
    //        //Debug.Log("�ؿ�߶�" + Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir) + "  �߶ȣ�" + h);

    //        if (h < Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir))
    //        {
    //            return false;
    //        }
    //    }

    //    return true;

    //}

    public static bool canHit(Vector3Int Target,Vector3Int LunchPos, string nam = "",bool needPos = false)
    {
        int Velocity = getWeponVelocity(nam);
        //int Velocity = 564;

        if (Velocity == 0) return true;
        if (!needPos) LunchPos = GameManager.GM.currentPosition;

        int HexDis = Map.HexDistence(Target, LunchPos);
        //int HexDis = Map.HexDistence(Target, Vector3Int.zero);
        int Distance = HexDis * 500;//���룬��λ:��
        float g = 9.8f;//�������ٶȣ���/��^2


        float aTheta = Mathf.Asin(Distance * g / Mathf.Pow(Velocity, 2)) / 2;


        if (aTheta < -1 || aTheta > 1) return false;

        float Vx = Velocity * Mathf.Cos(aTheta);
        float Vh = Velocity * Mathf.Sin(aTheta);

        List<CellInfo> Line = Map.LineSerch(LunchPos, Target);
        //List<CellInfo> Line = Map.LineSerch(Vector3Int.zero, Target);

        for (int i = 1; i < Line.Count - 1; i++)
        {
            //Debug.Log("�߶ȣ�" + (TanTheta * i * 500 - Mathf.Pow(i * 500, 2) * gCosTheta2V));
            float t = i * 500 / Vx;
            float h = Vh * t - 0.5f * g * Mathf.Pow(t, 2);
            //Debug.Log("�ؿ�߶�" + Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir) + "  �߶ȣ�" + h);

            if (h < Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir))
            {
                return false;
            }
        }

        return true;

    }

    public static void PrepareSpell(string SpellNam)
    {
        int radios = 0;
        switch (SpellNam)
        {
            case "��������":
                radios = 1;
                break;
            case "���Ҹ�ȼ":
                radios = 1;
                break;
            case "����һ��":
                radios = 1;
                break;
            case "���ؽ��":
                radios = 2;
                break;
            case "PosConfuse":
                radios = 1;
                break;
            case "ReverseEngineering":
                radios = 1;
                break;
            case "ChannelBlocking":
                radios = 2;
                break;
            case "TrojanVirus":
                radios = 2;
                break;
        }


        Map.SetArea(GameManager.GM.currentPosition,
            radios,
            FixGameData.FGD.AttackAreaMap,
            FixGameData.FGD.MoveArea,
            true);
    }

    public static void CastSpell(string SpellNam,Vector3Int Pos)
    {
        switch (SpellNam)
        {
            case "��������":
                Spell_PrefectFreeze(Pos);
                break;
            case "���Ҹ�ȼ":
                Spell_Flame(Pos);
                break;
            case "����һ��":
                Spell_Strom(Pos);
                break;
            case "���ؽ��":
                Spell_ArcaneBarrier(Pos);
                break;
            case "PosConfuse":
                Spell_PosConfuse(Pos);
                break;
            case "ReverseEngineering":
                Spell_ReverseEngineering(Pos);
                break;
            case "ChannelBlocking":
                Spell_ChannelBlocking(Pos);
                break;
            case "TrojanVirus":
                Spell_TrojanVirus(Pos);
                break;
        }
    }

    public static void BuildFacility(string FacName, Vector3Int Pos)
    {
        int dis = 1;
        int mainDir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        int subDir = (mainDir + 4) % 6 + 1;
        int mainDir2 = mainDir % 6 + 1;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, dis);//.Where(x => Map.HexDistence(GameManager.GM.currentPosition, x.Positian) == dis).ToList();
        //�����򣬺�һ���������һ
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //������
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        //��������
        foreach (CellInfo area in Area1)
        {
            //������
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell(FacName, tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[8 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.FacilityList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.FacilityList[addr] = tmp;
            }

            //�������һ
            tmpT = Map.GetSideAddr(area.Positian, mainDir2);
            tmp = new FacilityDataCell(FacName, tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[8 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x =>  x.Positian == tmp.Positian && x.dir == mainDir2);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.FacilityList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.FacilityList[addr] = tmp;
            }
        }

        //������
        foreach (CellInfo area in Area2)
        {
            //������
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell(FacName, tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[8 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x =>  x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.FacilityList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.FacilityList[addr] = tmp;
            }

            //������
            tmpT = Map.GetSideAddr(area.Positian, subDir);
            tmp = new FacilityDataCell(FacName, tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[8 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == tmp.Positian && x.dir == subDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.FacilityList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.FacilityList[addr] = tmp;
            }
        }

        //��ȫ
        tmpT = Map.GetSideAddr(Pos, subDir);
        FacilityDataCell tmpFac = new FacilityDataCell(FacName, tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
        FixGameData.FGD.MapList[8 + tmpT.Item1].SetTile(tmpT.Item2, tmpFac.Data.Item2.GetSideTile(tmpT.Item1 + 1));
        int addrS = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == tmpFac.Positian && x.dir == mainDir);
        if (addrS == -1)
        {
            //����
            FixGameData.FGD.FacilityList.Add(tmpFac);
        }
        else
        {
            //ˢ��
            FixGameData.FGD.FacilityList[addrS] = tmpFac;
        }
    }

    static void Spell_PrefectFreeze(Vector3Int Pos)
    {
        int Hexdir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 3).Where(x=>Map.HexDirectionAxis(GameManager.GM.currentPosition,x.Positian) == Hexdir).ToList();
        foreach (CellInfo Cell in Area)
        {
            Tuple<int, Vector3Int> sideLoc = Map.GetSideAddr(Cell.Positian, Hexdir);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == Cell.Positian);
            int sideAddr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == sideLoc.Item2 && x.dir == sideLoc.Item1);

            //���ķ���
            if (addr != -1 && FixGameData.FGD.SpecialTerrainList[addr].Id == "FlameZone")
            {
                FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell("FrozenZone", Cell.Positian, 0, 2, false, false));
                FixGameData.FGD.MapList[14].SetTile(Cell.Positian, null);
            }
            else if(addr >=0 && (FixGameData.FGD.SpecialTerrainList[addr].Id == "Ice" || FixGameData.FGD.SpecialTerrainList[addr].Id == "FrozenZone"))
            {
                FixGameData.FGD.SpecialTerrainList[addr] = new FacilityDataCell(FixGameData.FGD.SpecialTerrainList[addr].Id, Cell.Positian, 0, 2, false, false);
            }
            else if(addr == -1 && FixGameData.FGD.MapList[0].GetTile(Cell.Positian).name=="Water")
            {
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell("Ice", Cell.Positian, 0, 2, false, false));
                FixGameData.FGD.MapList[14].SetTile(Cell.Positian, FixSystemData.GlobalSpecialTerrainList["Ice"].Top);
            }
            else
            {
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell("FrozenZone", Cell.Positian, 0, 2, false, false));
                FixGameData.FGD.MapList[14].SetTile(Cell.Positian, FixSystemData.GlobalSpecialTerrainList["FrozenZone"].Top);
            }

            //��߷���
            if (FixGameData.FGD.MapList[1+ sideLoc.Item1].GetTile(sideLoc.Item2) != null)
            {
                if(sideAddr != -1)
                {
                    FixGameData.FGD.SpecialTerrainList[sideAddr] = new FacilityDataCell("IceRiver", sideLoc.Item2, sideLoc.Item1, 1, true, false);
                }
                else
                {
                    FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell("IceRiver", sideLoc.Item2, sideLoc.Item1, 1, true, false));
                }
                //Debug.Log(FixGameData.FGD.MapList[1 + sideLoc.Item1].name);
                FixGameData.FGD.MapList[11 + sideLoc.Item1].SetTile(sideLoc.Item2, FixSystemData.GlobalSpecialTerrainList["IceRiver"].GetSideTile(sideLoc.Item1+1));
            }
            
        }

        


    }

    static void Spell_Flame(Vector3Int Pos)
    {
        int Hexdir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 3).Where(x => Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == Hexdir).ToList();
        foreach (CellInfo Cell in Area)
        {
            Tuple<int, Vector3Int> sideLoc = Map.GetSideAddr(Cell.Positian, Hexdir);
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == Cell.Positian);
            int sideAddr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == sideLoc.Item2 && x.dir == sideLoc.Item1);

            //���ķ���
            if(addr >=0 && (FixGameData.FGD.SpecialTerrainList[addr].Id == "FrozenZone" || FixGameData.FGD.SpecialTerrainList[addr].Id == "Ice"))
            {
                FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
                FixGameData.FGD.MapList[14].SetTile(Cell.Positian, null);
            }else if(addr >= 0 && FixGameData.FGD.SpecialTerrainList[addr].Id == "FlameZone")
            {
                FixGameData.FGD.SpecialTerrainList[addr] = new FacilityDataCell("FlameZone", Cell.Positian, 0, 2, false, false);
            }else if(addr < 0)
            {
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell("FlameZone", Cell.Positian, 0, 2, false, false));
                FixGameData.FGD.MapList[14].SetTile(Cell.Positian, FixSystemData.GlobalSpecialTerrainList["FlameZone"].Top);
            }

            //��߷���
            if (sideAddr >= 0)
            {

                //Debug.Log(FixGameData.FGD.MapList[1 + sideLoc.Item1].name);
                FixGameData.FGD.SpecialTerrainList.RemoveAt(sideAddr);
                FixGameData.FGD.MapList[11 + sideLoc.Item1].SetTile(sideLoc.Item2, null);
            }

        }
    }

    static void Spell_Strom(Vector3Int Pos)
    {
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 3);
        int dir = Map.HexDirectionInt(GameManager.GM.currentPosition, Pos);
        int dir2 = (dir + 4) % 6 + 1;
        int dir3 = dir % 6 + 1;
        List<CellInfo> BackArea = Area.Where(
            x => (Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir ||
            Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir2 ||
            Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == dir3)).ToList();

        Area = BackArea.Where(x => Map.HexDistence(x.Positian, GameManager.GM.currentPosition) < 3).ToList();
        BackArea = BackArea.Where(x => Map.HexDistence(x.Positian, GameManager.GM.currentPosition) == 3).ToList();

        foreach(CellInfo Cell in Area)
        {
            CellInfo tar = BackArea.OrderBy(x => Map.HexDistence(Cell.Positian, x.Positian)).First();
            List<GameObject> PieceList = GameManager.GM.EnemyPool.getChildByPos(Cell.Positian);
            int hereDir = Map.HexDirectionInt(GameManager.GM.currentPosition, Cell.Positian);
            
            foreach(GameObject Enemy in PieceList)
            {
                if (!Enemy.GetComponent<OB_Piece>().ForceMoveTo(tar.Positian))
                {
                    Enemy.GetComponent<OB_Piece>().TakeDemage(2);
                }
            }
        }
        OB_Piece.needChenkVisibility = BackArea.Select(x => x.Positian).ToList();
        OB_Piece.CheckVisibility();
        Map.UpdateZOC();

    }

    static void Spell_ArcaneBarrier(Vector3Int Pos)
    {
        int dis = Map.HexDistence(Pos, GameManager.GM.currentPosition);
        int mainDir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        int subDir = (mainDir + 4) % 6 + 1;
        int mainDir2 = mainDir % 6 + 1;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, dis).Where(x => Map.HexDistence(GameManager.GM.currentPosition, x.Positian) == dis).ToList();
        //�����򣬺�һ���������һ
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //������
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        //��������
        foreach (CellInfo area in Area1)
        {
            //������
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //�������һ
            tmpT = Map.GetSideAddr(area.Positian, mainDir2);
            tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir2);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

        //������
        foreach (CellInfo area in Area2)
        {
            //������
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //������
            tmpT = Map.GetSideAddr(area.Positian, subDir);
            tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == subDir);
            if (addr == -1)
            {
                //����
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //ˢ��
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

        //��ȫ
        tmpT = Map.GetSideAddr(Pos, subDir);
        FacilityDataCell tmpFac = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
        FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmpFac.Data.Item2.GetSideTile(tmpT.Item1 + 1));
        int addrS = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmpFac.Positian && x.dir == mainDir);
        if (addrS == -1)
        {
            //����
            FixGameData.FGD.SpecialTerrainList.Add(tmpFac);
        }
        else
        {
            //ˢ��
            FixGameData.FGD.SpecialTerrainList[addrS] = tmpFac;
        }
    }

    static void Spell_PosConfuse(Vector3Int Pos)
    {
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 1);
        foreach (CellInfo Cell in Area)
        {
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == Cell.Positian && x.dir == 0);
            if(addr == -1)
            {
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                    "PosDisorderZone",
                    Cell.Positian,
                    0,1,
                    false,
                    false
                    ));
            }
            else
            {
                FixGameData.FGD.SpecialTerrainList[addr] = new FacilityDataCell(
                    "PosDisorderZone",
                    Cell.Positian,
                    0, 1,
                    false,
                    false
                    );
            }

            FixGameData.FGD.MapList[14].SetTile(Cell.Positian, FixSystemData.GlobalSpecialTerrainList["PosDisorderZone"].Top);
            
        }
    }

    static void Spell_TrojanVirus(Vector3Int Pos)
    {
        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 1);
        foreach (CellInfo Cell in Area)
        {
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == Cell.Positian && x.dir == 0);
            if (addr == -1)
            {
                FixGameData.FGD.SpecialTerrainList.Add(new FacilityDataCell(
                    "DataDisorderZone",
                    Cell.Positian,
                    0, 1,
                    false,
                    false
                    ));
            }
            else
            {
                FixGameData.FGD.SpecialTerrainList[addr] = new FacilityDataCell(
                    "DataDisorderZone",
                    Cell.Positian,
                    0, 1,
                    false,
                    false
                    );
            }

            FixGameData.FGD.MapList[14].SetTile(Cell.Positian, FixSystemData.GlobalSpecialTerrainList["DataDisorderZone"].Top);

        }
    }

    public static void Spell_ReverseEngineering(Vector3Int Pos)
    {
        //int dis = Map.HexDistence(Pos, GameManager.GM.currentPosition);
        int mainDir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        int subDir = (mainDir + 4) % 6 + 1;
        int mainDir2 = mainDir % 6 + 1;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 3);
        //�����򣬺�һ���������һ
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //������
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        foreach (CellInfo area in Area1)
        {
            //tmpT = Map.GetSideAddr(area.Positian, 1);
            //����
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == area.Positian &&
            (x.Data.Item2 as Facility).Belone == ArmyBelong.Human &&
            (x.Data.Item2 as Facility).isSpecialLandShape &&
            x.dir == 0
            );
            if(addr != -1)
            {
                FixGameData.FGD.MapList[14].SetTile(area.Positian, null);
                FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
            }

            //���
            for(int i = 1; i < 4; i++)
            {
                tmpT = Map.GetSideAddr(area.Positian, i);
                addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == area.Positian &&
                (x.Data.Item2 as Facility).Belone == ArmyBelong.Human &&
                (x.Data.Item2 as Facility).isSpecialLandShape &&
                x.dir == i
                );
                if (addr != -1)
                {
                    FixGameData.FGD.MapList[11+tmpT.Item1].SetTile(tmpT.Item2, null);
                    FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
                }
            }
        }

        foreach (CellInfo area in Area2)
        {
            //tmpT = Map.GetSideAddr(area.Positian, 1);
            //����
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == area.Positian &&
            (x.Data.Item2 as Facility).Belone == ArmyBelong.Human &&
            (x.Data.Item2 as Facility).isSpecialLandShape &&
            x.dir == 0
            );
            if (addr != -1)
            {
                FixGameData.FGD.MapList[14].SetTile(area.Positian, null);
                FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
            }

            //���
            for (int i = 1; i < 4; i++)
            {
                tmpT = Map.GetSideAddr(area.Positian, i);
                addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Positian == area.Positian &&
                (x.Data.Item2 as Facility).Belone == ArmyBelong.Human &&
                (x.Data.Item2 as Facility).isSpecialLandShape &&
                x.dir == i
                );
                if (addr != -1)
                {
                    FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, null);
                    FixGameData.FGD.SpecialTerrainList.RemoveAt(addr);
                }
            }
        }
    }

    public static void Spell_ChannelBlocking(Vector3Int Pos)
    {

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(Pos, 1);
        foreach(CellInfo cell in Area)
        {
            List<GameObject> Pieces = GameManager.GM.EnemyPool.getChildByPos(cell.Positian);
            foreach(GameObject p in Pieces)
            {
                p.GetComponent<OB_Piece>().CulateSupplyConnection(true, false);
            }

        }

    }

    //����˫��ս����
    public static void CulATK(ref float ATK,ref float DEF, List<Vector3Int> AtkLst, Vector3Int DefPos)
    {
        DEF = 0;
        ATK = 0;

        foreach(GameObject pic in GameManager.GM.EnemyPool.getChildByPos(DefPos))
        {
            DEF += pic.GetComponent<OB_Piece>().getPieceData().DEF;
        }

        DEF = Map.GetTargetDEFK(DefPos, (ArmyBelong)(((int)GameManager.GM.ActionSide + 1) % 2), DEF);

        foreach(Vector3Int pos in AtkLst)
        {
            float tmpAtk = 0;
            foreach (GameObject pic in GameManager.GM.ActionPool.getChildByPos(pos))
            {
                if (pic.GetComponent<OB_Piece>().ActionPoint == 0) continue;
                tmpAtk += pic.GetComponent<OB_Piece>().getPieceData().ATK;
            }
            tmpAtk = Map.GetTargetATK(pos,Map.HexDirectionInt(DefPos,pos),GameManager.GM.ActionSide,tmpAtk);

            ATK += tmpAtk;
        }

    }

    //�������
    public static void CommitAttack(float ATK,float DEF,List<Vector3Int> AtkLst,Vector3Int DefPos,int RRKMend)
    {
        double rrk = FixSystemData.battleJudgeForm.GetRRK(ATK, DEF);
        //RRK����
        rrk = Map.GetBattleRRK(DefPos, GameManager.GM.ActionSide, rrk) + RRKMend;
        

        string Result = FixSystemData.battleJudgeForm.GetResult(rrk);
        Debug.Log(Result);
        if (Result == "X") return;

        int dmg;
        bool hasNum = int.TryParse(Result.Substring(0, 1), out dmg);

        string ActSide = hasNum ? Result.Substring(1, 1) : Result.Substring(0, 1);
        int retDis = hasNum ? Result.Substring(2).Length : Result.Substring(1).Length;
        ArmyBelong SufferSide;
        Vector3Int RetCenter;
        List<OB_Piece> SufferList = new List<OB_Piece>();

        if(ActSide == "A")
        {
            //ѡ����������
            foreach (Vector3Int pos in AtkLst)
            {
                List<OB_Piece> tmp = GameManager.GM.ActionPool.getChildByPos(pos).Select(x => x.GetComponent<OB_Piece>()).ToList();
                SufferList.AddRange(tmp);
            }
            SufferSide = GameManager.GM.ActionSide;
            RetCenter = DefPos;
        }
        else
        {
            //ѡ����������
            SufferList = GameManager.GM.EnemyPool.getChildByPos(DefPos).Select(x => x.GetComponent<OB_Piece>()).ToList();
            SufferSide = (ArmyBelong)(((int)GameManager.GM.ActionSide + 1) % 2);

            RetCenter = FixGameData.FGD.InteractMap.WorldToCell(
                    new Vector3(AtkLst.Average(x => FixGameData.FGD.InteractMap.CellToWorld(x).x), AtkLst.Average(x => FixGameData.FGD.InteractMap.CellToWorld(x).y))
                );
        } 


        //�����˺�
        for (int i = 0; i < dmg; i++)
        {
            int luckyGuy = UnityEngine.Random.Range(0, SufferList.Count);
            SufferList[luckyGuy].TakeDemage(1);
        }
        //�����ƶ�
        List<CellInfo> BackArea;
        List<OB_Piece> killList = new List<OB_Piece>();
        //�����ƶ�
        foreach (OB_Piece pic in SufferList)
        {
            int curDis = Map.HexDistence(pic.piecePosition, RetCenter);

            int dir = Map.HexDirectionInt(RetCenter, pic.piecePosition);
            int dir2 = (dir + 4) % 6 + 1;
            int dir3 = dir % 6 + 1;
            //��ȡ����
            BackArea = Map.PowerfulBrickAreaSearch(pic.piecePosition, retDis).Where(
                x => (Map.HexDirectionInt(RetCenter, x.Positian) == dir ||
                Map.HexDirectionInt(RetCenter, x.Positian) == dir2 ||
                Map.HexDirectionAxis(RetCenter, x.Positian) == dir3) &&
                Map.HexDistence(pic.piecePosition,x.Positian) == retDis &&
                Map.HexDistence(RetCenter, x.Positian) > curDis).ToList();
            //���տ���������
            _ = BackArea.OrderBy(x => Map.GetNearMov(x.Positian, 0, SufferSide)).ToList();

            int counter = 0;
            foreach(CellInfo cell in BackArea)
            {
                if (pic.ForceMoveTo(cell.Positian))
                {
                    break;
                }
                counter++;
            }
            if (counter == BackArea.Count) killList.Add(pic);

        }
        //ɱ���޷��ƶ���
        for(int i = 0; i < killList.Count; i++)
        {
            killList[0].TakeDemage(100);
        }
    }

    public static void CommitAirStrick(Dictionary<Piece, int> FriendList, Dictionary<Piece, int> EnemyList,int GroundEnemySup)
    {
        //ͳ��˫����սս����
        int ATK = 0;
        int ATKtoGround = 0;
        foreach(KeyValuePair<Piece, int> pair in FriendList)
        {
            ATK += pair.Key.DEF * pair.Value;
            if (!pair.Key.canAirBattle)
            {
                ATKtoGround += pair.Key.ATK * pair.Value;
            }
        }

        int DEF = 0;
        foreach (KeyValuePair<Piece, int> pair in EnemyList)
        {
            DEF += pair.Key.ATK * pair.Value;
        }

        DEF += GroundEnemySup;//������ջ���

        //�ж�
        string resu = FixSystemData.airBattleJudgeForm.getResult(ATK, DEF);

        switch (resu[resu.Length - 1])
        {
            case 'D':
                ATKtoGround -= int.Parse(resu.Substring(0, resu.Length - 1));
                break;
            case 'X':
                break;
            case 'C':
                return;
        }
        //����
        string[] resuSet = FixSystemData.fireStrikeJudgeForm.getResult(ATKtoGround).Split("+");

        //�����˺�
        Action<int[], List<GameObject>> TakeAllDMG = (x, targetSet) =>
        {
            for (int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeDemage(x[i]);
            }
        };
        //�����ȶ���
        Action<int[], List<GameObject>> TakeAllStabel = (x, targetSet) =>
        {
            for (int i = 0; i < x.Length; i++)
            {
                targetSet[i].GetComponent<OB_Piece>().TakeUnstable(x[i]);
            }
        };

        Action<int[], List<GameObject>> tarAct;
        List<GameObject> targets = GameManager.GM.EnemyPool.getChildByPos(GameManager.GM.currentPosition);

        if (resuSet[0] == "X") return;
        foreach (string res in resuSet)
        {
            int dmg = int.Parse(res.Substring(0, res.Length - 1));

            int[] dmgSet = new int[targets.Count];
            for (int i = 0; i < dmg; i++)
            {
                dmgSet[UnityEngine.Random.Range(0, targets.Count)]++;
            }

            if (res.EndsWith("K")) tarAct = TakeAllDMG;
            else if (res.EndsWith("D")) tarAct = TakeAllStabel;
            else tarAct = null;

            if (tarAct != null) tarAct(dmgSet, targets);
        }

        Map.UpdatePieceStackSign();
        Map.UpdateZOC();

    }

    public static void CommitMentalAttack(Vector3Int TarPos)
    {
        if (GameManager.GM.HumanEventList.Where(x=>x.Item1 == SpecialEvent.MentalAD).Count() > 0) return;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, 2);
        int dir = Map.HexDirectionInt(GameManager.GM.currentPosition, TarPos);
        int dir2 = (dir + 4) % 6 + 1;
        int dir3 = dir % 6 + 1;
        Area = Area.Where(
            x => (Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir ||
            Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == dir2 ||
            Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == dir3)).ToList();


        Map.SetArea(Area, FixGameData.FGD.AttackAreaMap, FixGameData.FGD.MoveArea, true);

        foreach(Vector3Int pos in Area.Select(x => x.Positian))
        {
            List<GameObject> pieces = GameManager.GM.EnemyPool.getChildByPos(pos);
            foreach(GameObject piece in pieces)
            {
                piece.GetComponent<OB_Piece>().Betray();
            }

            if(pieces.Count == 0 && Map.GetPLaceInfo(pos, 0)[3]?.id == "Shelter")
            {
                GameObject tmp = BasicUtility.SpawnPiece("Human.Betray", pos, null, true);
                tmp.GetComponent<OB_Piece>().Betray();
            }

        }


    }

    public static void BuildDefenceArea(Vector3Int Pos)
    {
        int addr = FixGameData.FGD.FacilityList.FindIndex(
            x => x.Positian == Pos &&
                x.dir == 0 &&
                x.Id == "DefenceArea"
                );

        FacilityDataCell temp = new FacilityDataCell("DefenceArea", Pos, 0, 2, false);
        FixGameData.FGD.MapList[7].SetTile(Pos, temp.Data.Item2.Top);
        
        if (addr == -1)
        {
            FixGameData.FGD.FacilityList.Add(temp);
        }
        else
        {

            FixGameData.FGD.FacilityList[addr] = temp;
        }
    }

    public static void DoEmergencyMaintenance(Vector3Int Pos)
    {
        List<GameObject> targets = GameManager.GM.ActionPool.getChildByPos(Pos);
        foreach(GameObject piece in targets)
        {
            piece.GetComponent<OB_Piece>().RecoverStable(1);
        }
    }
}
