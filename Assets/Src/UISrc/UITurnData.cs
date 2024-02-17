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
        ActionSideShow.text = "�ж�����" + (GameManager.GM.ActionSide == ArmyBelong.Human ? "��������" : "������־");
        TurnStageShow.text = "�غϽ׶Σ�";
        switch (GameManager.GM.Stage)
        {
            case TurnStage.ZeroTurn:
                TurnStageShow.text += "���෽�ƶ�";
                break;
            case TurnStage.Strategy:
                TurnStageShow.text += "���Խ׶�";
                break;
            case TurnStage.ModBattle:
                TurnStageShow.text += "ģ��ս�׶�";
                break;
            case TurnStage.Action:
                TurnStageShow.text += "�ж��׶�";
                break;
            case TurnStage.Support:
                TurnStageShow.text += "��Ԯ�׶�";
                break;
            case TurnStage.Settle:
                TurnStageShow.text += "����׶�";
                break;
            default:
                TurnStageShow.text += "�Ҳ�����";
                break;
        }
    }

    
}
