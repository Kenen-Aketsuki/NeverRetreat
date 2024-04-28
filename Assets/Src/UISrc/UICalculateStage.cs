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
        yield return new WaitForSeconds(1);
        GameManager.GM.NextStage();
    }
}
