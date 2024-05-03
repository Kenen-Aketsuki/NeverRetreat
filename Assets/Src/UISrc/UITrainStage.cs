using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITrainStage : MonoBehaviour , IUIHandler
{
    public static bool keepTrain = false;
    public static bool stepTrain = false;

    public void OnPieceSelect(bool isFriend)
    {

    }

    public void OnTerrainSelect(bool isFac)
    {

    }

    public void OnPositionSelect(Vector3Int pos)
    {

    }

    public void UpdateShow()
    {
        
    }

    public string WhatShouldIDo()
    {
        return "AIÑµÁ·ÖÐ";
    }


    private void Awake()
    {
        keepTrain = false;
        stepTrain = false;
    }

    private void OnEnable()
    {
        FixGameData.FGD.uiManager.actUI = this;
    }

    public void switchTrain()
    {
        if (!keepTrain && !stepTrain)
        {
            keepTrain = true;
            stepTrain = false;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>().text = "×Ô¶¯ÑµÁ·";
        } else if (keepTrain && !stepTrain)
        {
            keepTrain = true;
            stepTrain = true;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>().text = "²½½øÑµÁ·";
        }
        else if(keepTrain && stepTrain)
        {
            keepTrain = false;
            stepTrain = false;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>().text = "Í£Ö¹ÑµÁ·";
        }
        
    }

    public void resetServe()
    {
        HttpConnect.instance.JustGetRequest(FixSystemData.AIUrl + "/Reset", x =>
        {
            FixGameData.FGD.uiIndex.HintUI.SetText(x);
            FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
        });
    }

    public void saveModel()
    {
        HttpConnect.instance.JustGetRequest(FixSystemData.AIUrl + "/SaveModel", x =>
        {
            FixGameData.FGD.uiIndex.HintUI.SetText(x);
            FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
        });
    }

    public void updatePieceKey()
    {
        HttpConnect.instance.UpdatePieceKey(x =>
        {
            FixGameData.FGD.uiIndex.HintUI.SetText(x);
            FixGameData.FGD.uiIndex.HintUI.SetExitTime(1);
        });
    }

}
