using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        if (!canHit(Pos))
        {
            FixGameData.FGD.uiIndex.HintUI.SetText("火炮无法命中");
            FixGameData.FGD.uiIndex.HintUI.SetExitTime(3);
            return false;
        }
        
        string fireRank = FixSystemData.fireRankForm.getData(GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2], Map.HexDistence(Pos, GameManager.GM.currentPosition));
        string[] resuSet = FixSystemData.fireStrikeJudgeForm.getResult(fireRank).Split("+");

        List<GameObject> targets = GameManager.GM.EnemyPool.getChildByPos(Pos);
        Action<int[], List<GameObject>> tarAct;
        if (targets.Count == 0) return false;

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
        return true;
    }

    static int getWeponVelocity()
    {
        int V = 0;
        switch (GameManager.GM.currentPiece.getPieceData().PName.Split("/")[2])
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

    public static bool canHit(Vector3Int Target)
    {
        int Velocity = getWeponVelocity();
        //int Velocity = 564;

        if (Velocity == 0) return true;

        int HexDis = Map.HexDistence(Target, GameManager.GM.currentPosition);
        //int HexDis = Map.HexDistence(Target, Vector3Int.zero);
        int Distance = HexDis * 500;//距离，单位:米
        float g = 9.8f;//重力加速度，米/秒^2


        float aTheta = Mathf.Asin(Distance * g / Mathf.Pow(Velocity, 2)) / 2;


        if (aTheta < -1 || aTheta > 1) return false;

        float Vx = Velocity * Mathf.Cos(aTheta);
        float Vh = Velocity * Mathf.Sin(aTheta);

        List<CellInfo> Line = Map.LineSerch(GameManager.GM.currentPosition, Target);
        //List<CellInfo> Line = Map.LineSerch(Vector3Int.zero, Target);

        for (int i = 1; i < Line.Count - 1; i++)
        {
            //Debug.Log("高度：" + (TanTheta * i * 500 - Mathf.Pow(i * 500, 2) * gCosTheta2V));
            float t = i * 500 / Vx;
            float h = Vh * t - 0.5f * g * Mathf.Pow(t, 2);
            Debug.Log("地块高度" + Map.GetCellHeightForStrick(Line[i].Positian, Line[i].fromDir) + "  高度：" + h);

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
            case "完美冻结":
                radios = 1;
                break;
            case "死灰复燃":
                radios = 1;
                break;
            case "风神一扇":
                radios = 1;
                break;
            case "二重结界":
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
            case "完美冻结":
                Spell_PrefectFreeze(Pos);
                break;
            case "死灰复燃":
                Spell_Flame(Pos);
                break;
            case "风神一扇":
                Spell_Strom(Pos);
                break;
            case "二重结界":
                Spell_ArcaneBarrier(Pos);
                break;
            case "PosConfuse":
                Spell_PosConfuse(Pos);
                break;
            case "ReverseEngineering":
                Spell_ReverseEngineering(Pos);
                break;
            case "ChannelBlocking":
                
                break;
            case "TrojanVirus":
                Spell_TrojanVirus(Pos);
                break;
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

            //格心放置
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

            //格边放置
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

            //格心放置
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

            //格边放置
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
    }

    static void Spell_ArcaneBarrier(Vector3Int Pos)
    {
        int dis = Map.HexDistence(Pos, GameManager.GM.currentPosition);
        int mainDir = Map.HexDirectionAxis(GameManager.GM.currentPosition, Pos);
        int subDir = (mainDir + 4) % 6 + 1;
        int mainDir2 = mainDir % 6 + 1;

        List<CellInfo> Area = Map.PowerfulBrickAreaSearch(GameManager.GM.currentPosition, dis).Where(x => Map.HexDistence(GameManager.GM.currentPosition, x.Positian) == dis).ToList();
        //主方向，和一个主方向加一
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //副方向
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        //主方向上
        foreach (CellInfo area in Area1)
        {
            //主方向
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //主方向加一
            tmpT = Map.GetSideAddr(area.Positian, mainDir2);
            tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir2);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

        //副方向
        foreach (CellInfo area in Area2)
        {
            //主方向
            tmpT = Map.GetSideAddr(area.Positian, mainDir);
            FacilityDataCell tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            int addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == mainDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }

            //副方向
            tmpT = Map.GetSideAddr(area.Positian, subDir);
            tmp = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
            FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmp.Data.Item2.GetSideTile(tmpT.Item1 + 1));
            addr = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmp.Positian && x.dir == subDir);
            if (addr == -1)
            {
                //加入
                FixGameData.FGD.SpecialTerrainList.Add(tmp);
            }
            else
            {
                //刷新
                FixGameData.FGD.SpecialTerrainList[addr] = tmp;
            }
        }

        //补全
        tmpT = Map.GetSideAddr(Pos, subDir);
        FacilityDataCell tmpFac = new FacilityDataCell("ArcaneBarrier", tmpT.Item2, tmpT.Item1 + 1, 2, true, false);
        FixGameData.FGD.MapList[11 + tmpT.Item1].SetTile(tmpT.Item2, tmpFac.Data.Item2.GetSideTile(tmpT.Item1 + 1));
        int addrS = FixGameData.FGD.SpecialTerrainList.FindIndex(x => x.Id == "ArcaneBarrier" && x.Positian == tmpFac.Positian && x.dir == mainDir);
        if (addrS == -1)
        {
            //加入
            FixGameData.FGD.SpecialTerrainList.Add(tmpFac);
        }
        else
        {
            //刷新
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
        //主方向，和一个主方向加一
        List<CellInfo> Area1 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == mainDir ||
        Map.HexDirectionAxis(GameManager.GM.currentPosition, x.Positian) == mainDir2).ToList();
        //副方向
        List<CellInfo> Area2 = Area.Where(x => Map.HexDirectionInt(GameManager.GM.currentPosition, x.Positian) == subDir).ToList();

        Tuple<int, Vector3Int> tmpT;
        foreach (CellInfo area in Area1)
        {
            //tmpT = Map.GetSideAddr(area.Positian, 1);
            //本格
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

            //格边
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
            //本格
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

            //格边
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
}
