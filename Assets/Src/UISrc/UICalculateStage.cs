using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICalculateStage : MonoBehaviour
{
    private void OnEnable()
    {
        Calculate.EndStageCalculate();
        if (FixGameData.FGD.resultMem.CanGameEnd())
        {
            GameManager.GM.GameEnd();
        }
        else
        {
            StartCoroutine(Delay());
        }
    }

    IEnumerator Delay()
    {
        FixGameData.FGD.ZoneMap.ClearAllTiles();
        yield return new WaitForSeconds(0.5f);
        
        foreach(FacilityDataCell fac in FixGameData.FGD.SpecialFacilityList)
        {
            if (fac.active) fac.ChangeActive();
        }
        yield return new WaitForSeconds(0.5f);

        for(int i = 0; i < FixGameData.FGD.FacilityList.Count; i++)
        {
            if (FixGameData.FGD.FacilityList[i].PassTime())
            {
                FixGameData.FGD.FacilityList[i].RemoveSelf();
            }
        }
        yield return new WaitForSeconds(0.5f);

        GameManager.GM.NextStage();
    }
}
