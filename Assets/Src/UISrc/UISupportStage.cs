using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UISupportStage : MonoBehaviour
{

    private void OnEnable()
    {
        List<Tuple<string, string, int>> orgList;
        if (GameManager.GM.ActionSide == ArmyBelong.Human) orgList = FixGameData.FGD.HumanLoadList;
        else orgList = FixGameData.FGD.CrashLoadList;

        StartCoroutine(LoadPiece(orgList));
        
    }

    IEnumerator LoadPiece(List<Tuple<string, string, int>> orgList)
    {
        List<Tuple<string, string, int>> usablePiece = orgList.Where(x => x.Item3 == 0).ToList();

        foreach(Tuple<string, string, int> pair in usablePiece)
        {
            Debug.Log(pair.Item1 + " " + pair.Item2 + " " + pair.Item3);
            string[] enterPls = pair.Item2.Split("-");
            int center = enterPls.Count() > 2 ? int.Parse(enterPls[1]) : 0;

            GameObject piece;
            switch (enterPls[0])
            {
                case "Bottom":
                case "Top":
                case "Left":
                case "Right":
                    piece = Support.SupportFromDiraction(enterPls[0], pair.Item1);
                    break;
                default:
                    piece = Support.SupportFromFacility(enterPls[0],pair.Item1);
                    break;
            }

            if(piece != null) // 棋子移动结束前阻塞
            {
                OB_Piece pic = piece.GetComponent<OB_Piece>();
                while (pic.needMove) yield return null;
            }
        }
        yield return new WaitForSeconds(2);
        GameManager.GM.NextStage();
    }
}
