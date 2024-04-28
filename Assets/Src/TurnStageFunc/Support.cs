using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class Support
{
    public static GameObject SupportFromFacility(string facId, string troopId)
    {
        List<FacilityDataCell> lands = FixGameData.FGD.FacilityList.Where(
            x => x.Id == facId &&
            (
                x.Data.Item1 == typeof(SpecialFacility) && x.active ||
                !(x.Data.Item1 == typeof(SpecialFacility))
            )
        ).ToList();

        XmlNode pieceType;
        FixSystemData.HumanOrganizationList.TryGetValue(troopId, out pieceType);
        if (pieceType == null) FixSystemData.CrashOrganizationList.TryGetValue(troopId, out pieceType);

        
        Piece Data = new Piece(pieceType, null);
        GameObject piece = null;

        int staDir = Random.Range(0, lands.Count);
        for(int i = 0; i < lands.Count; i++)
        {
            if (SupportArriveAt(Data, lands[staDir].Positian, ref piece)) break;
            staDir = (staDir + 1) % lands.Count;
        }

        return piece;
    }

    public static GameObject SupportFromDiraction(string dir, string troopId)
    {
        Vector3Int orgSta;
        Vector3Int dirES;
        switch (dir)
        {
            case "Bottom":
                orgSta = new Vector3Int(GameUtility.rowRange.Item1, 0);
                dirES = Vector3Int.right;
                break;
            case "Top":
                orgSta = new Vector3Int(GameUtility.rowRange.Item2, 0);
                dirES = Vector3Int.up;
                break;
            case "Left":
                orgSta = new Vector3Int(0, GameUtility.columRange.Item1);
                dirES = Vector3Int.up;
                break;
            case "Right":
                orgSta = new Vector3Int(0, GameUtility.columRange.Item2);
                dirES = Vector3Int.right;
                break;
            default:
                orgSta = Vector3Int.zero;
                dirES = Vector3Int.zero;
                break;
        }

        List<Vector3Int> lands = Map.LineSerch(orgSta - dirES * 5, orgSta + dirES * 5).Select(x=>x.Positian).ToList();
        ShuffleList(ref lands);

        XmlNode pieceType;
        FixSystemData.HumanOrganizationList.TryGetValue(troopId, out pieceType);
        if (pieceType == null) FixSystemData.CrashOrganizationList.TryGetValue(troopId, out pieceType);


        Piece Data = new Piece(pieceType, null);
        GameObject piece = null;
        foreach (Vector3Int cell in lands)
        {
            if (SupportArriveAt(Data, cell, ref piece)) break;
        }

        return piece;
    }


    static bool SupportArriveAt(Piece Data, Vector3Int Pos,ref GameObject piece)
    {
        Dictionary<Vector3Int, CellInfo> MovArea = Map.DijkstraPathSerch(Pos, Data.MOV).Where(x => x.Value.moveCost != float.PositiveInfinity).ToDictionary(x => x.Key, x => x.Value);

        if (MovArea.Count == 1 && Map.GetNearMov(Pos, 0, GameManager.GM.ActionSide) == -1) return false;
        FixGameData.FGD.CameraNow.transform.position = FixGameData.FGD.InteractMap.CellToWorld(Pos) + new Vector3(0, 0, -10);

        Vector3Int endPos = MovArea.OrderByDescending(x => Map.HexDistence(x.Key, Pos)).ToList()[Random.Range(0, MovArea.Count)].Key;
        piece = BasicUtility.SpawnPiece(Data, Pos, true);

        GameManager.GM.MoveArea = MovArea;
        Map.UpdateMoveArea();
        if (!piece.GetComponent<OB_Piece>().MoveTo(endPos))
        {
            OB_Piece.Kill(piece, Data);
            piece = null;
            return false;
        }

        return true;
    }

    static void ShuffleList(ref List<Vector3Int> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(n, list.Count);
            Vector3Int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
