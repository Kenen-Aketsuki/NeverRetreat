using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITurnData : MonoBehaviour
{
    [SerializeField]
    TMP_Text ActionSideShow;
    [SerializeField]
    TMP_Text TurnStageShow;
    // Start is called before the first frame update
    public void UpdateInfo()
    {
        ActionSideShow.text = "行动方：" + (GameManager.GM.ActionSide == ArmyBelong.Human ? "人类势力" : "崩坏意志");
        TurnStageShow.text = "回合阶段：";
        switch (GameManager.GM.Stage)
        {
            case TurnStage.ZeroTurn:
                TurnStageShow.text += "人类方移动";
                break;
            case TurnStage.Strategy:
                TurnStageShow.text += "策略阶段";
                break;
            case TurnStage.ModBattle:
                TurnStageShow.text += "模组战阶段";
                break;
            case TurnStage.Action:
                TurnStageShow.text += "行动阶段";
                break;
            case TurnStage.Support:
                TurnStageShow.text += "增援阶段";
                break;
            case TurnStage.Settle:
                TurnStageShow.text += "结算阶段";
                break;
            default:
                TurnStageShow.text += "我不到啊";
                break;
        }
    }

    
}
