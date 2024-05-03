using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITurnControl
{
    // ����׶�  
    public abstract void ZeroTurnStageStart();

    public abstract void ZeroTurnStageEnd();

    // ���Խ׶�  
    public abstract void StrategyStageStart();

    public abstract void StrategyStageEnd(bool isTurnChange);

    // ģ��ս�׶�  
    public abstract void ModeBattleStageStart();

    public abstract void ModeBattleStageEnd(bool isTurnChange);

    // �ж��׶�
    public abstract void ActionStageStart();

    public abstract void ActionStageEnd(bool isTurnChange);

    //��Ԯ�׶�

    public abstract void SupportStageStart();

    public abstract void SupportStageEnd();

    //����׶�
    public abstract void CalculateStageStart();

    public abstract void CalculateStageEnd();
    
}

public class PVPTurn : ITurnControl
{
    #region//����׶�
    public void  ZeroTurnStageStart()
    {
        FixGameData.FGD.uiIndex.TurnZeroUISet.SetActive(true);
        for (int i = 0; i < FixGameData.FGD.HumanPieceParent.childCount; i++)
        {
            FixGameData.FGD.HumanPieceParent.GetChild(i).GetComponent<OB_Piece>().ResetMov();
        }
    }

    public void ZeroTurnStageEnd()
    {
        FixGameData.FGD.uiIndex.TurnZeroUISet.SetActive(false);
    }
    #endregion

    #region//���Խ׶�
    public void StrategyStageStart()
    {
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(true);
        Streagy.GetAirForce();
    }
    public void StrategyStageEnd(bool isTurnChange)
    {
        if (isTurnChange)
        {
            //չ�����
            Map.UpdateStaticBarrier();
            //����˺�
            for (int i = 0; i < FixGameData.FGD.CrashPieceParent.childCount; i++)
            {
                var tmp = FixGameData.FGD.CrashPieceParent.GetChild(i).GetComponent<OB_Piece>();
                if (FixGameData.FGD.MapList[14].GetTile(tmp.piecePosition) != null)
                {
                    tmp.TakeDemage(1);
                }
            }
            //�����¼�
            Streagy.CulEvent();
            //������϶״̬
            foreach (FacilityDataCell fac in FixGameData.FGD.SpecialFacilityList.Where(x => x.Id == "DimensionFissure"))
            {
                if (FixGameData.FGD.MapList[14].GetTile(fac.Positian) != null)
                {
                    fac.SetActive(false);
                }
            }

        }
        FixGameData.FGD.uiIndex.StrategyUISet.SetActive(false);
        Map.UpdateCrashBindwith();
    }
    #endregion

    #region //ģ��ս�׶�
    public void ModeBattleStageStart()
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(true);
    }

    public void ModeBattleStageEnd(bool isTurnChange)
    {
        FixGameData.FGD.uiIndex.ModBattleUISet.SetActive(false);
    }

    #endregion

    #region//�ж��׶�
    public void ActionStageStart()
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(true);
    }

    public void ActionStageEnd(bool isTurnChange)
    {
        FixGameData.FGD.uiIndex.ActionUISet.SetActive(false);
    }

    #endregion

    #region //��Ԯ�׶�

    public void SupportStageStart()
    {
        List<Tuple<string, string, int>> temp;
        if (GameManager.GM.ActionSide == ArmyBelong.Human) temp = FixGameData.FGD.HumanLoadList;
        else temp = FixGameData.FGD.CrashLoadList;

        if (temp.Count > 0)
        {
            FixGameData.FGD.uiIndex.SupportUISet.SetActive(true);
            GameManager.GM.SetMachineState(MachineState.Supporting);
            GameManager.GM.CanMachineStateChange = false;
        }
        else
        {
            GameManager.GM.NextStage();
        }


    }

    public void SupportStageEnd()
    {
        FixGameData.FGD.uiIndex.SupportUISet.SetActive(false);

        if (GameManager.GM.ActionSide == ArmyBelong.Human) FixGameData.FGD.HumanLoadList = FixGameData.FGD.HumanLoadList.Where(x => x.Item3 != 0).ToList();
        else FixGameData.FGD.CrashLoadList = FixGameData.FGD.CrashLoadList.Where(x => x.Item3 != 0).ToList();

        GameManager.GM.CanMachineStateChange = true;
        GameManager.GM.SetMachineState(MachineState.Idel);
    }

    #endregion

    #region //����׶�
    public void CalculateStageStart()
    {
        if (GameManager.GM.ActionSide == ArmyBelong.ModCrash)
        {
            GameManager.GM.NextStage();
            return;
        }
        FixGameData.FGD.uiIndex.CalculateUISet.SetActive(true);
        GameManager.GM.SetMachineState(MachineState.Calculating);
        GameManager.GM.CanMachineStateChange = false;
    }

    public void CalculateStageEnd()
    {
        GameManager.GM.CanMachineStateChange = true;
        GameManager.GM.SetMachineState(MachineState.Idel);
        FixGameData.FGD.uiIndex.CalculateUISet.SetActive(false);
    }
    #endregion
}

public class TRATurn : ITurnControl
{
    int maxCommandTime = 10;
    string lastPiece = "";
    int currentCommandTime = 0;

    #region//����׶�
    public void ZeroTurnStageStart()
    {
        GameManager.GM.NextStage();
    }

    public void ZeroTurnStageEnd()
    {
        
    }
    #endregion

    #region//���Խ׶�
    public void StrategyStageStart()
    {
        GameManager.GM.NextStage();
        FixGameData.FGD.uiIndex.TrainUISet.SetActive(true);
    }
    public void StrategyStageEnd(bool isTurnChange)
    {
        if (isTurnChange) GameManager.GM.CurrentTurnCount--;
    }
    #endregion

    #region //ģ��ս�׶�
    public void ModeBattleStageStart()
    {
        GameManager.GM.NextStage();
    }

    public void ModeBattleStageEnd(bool isTurnChange)
    {
        
    }

    #endregion

    #region//�ж��׶�
    public void ActionStageStart()
    {
        if (GameManager.GM.ActionSide != ArmyBelong.ModCrash)
        {
            GameManager.GM.NextStage();
            return;
        }

        CommandControl.CC.ToActionPiece.Clear();
        for (int i = 0; i < GameManager.GM.ActionPool.transform.childCount; i++)
        {
            CommandControl.CC.ToActionPiece.Enqueue(GameManager.GM.ActionPool.transform.GetChild(i).gameObject.GetComponent<OB_Piece>());
        }

        GameManager.GM.StartCoroutine(AIAgentWork());
    }

    public void ActionStageEnd(bool isTurnChange)
    {
        Debug.Log("�뿪�ж��׶�");
    }

    #endregion

    #region //��Ԯ�׶�

    public void SupportStageStart()
    {
        GameManager.GM.NextStage();
    }

    public void SupportStageEnd()
    {
        
    }

    #endregion

    #region //����׶�
    public void CalculateStageStart()
    {
        GameManager.GM.NextStage();
    }

    public void CalculateStageEnd()
    {
        
    }
    #endregion

    IEnumerator AIAgentWork()
    {
        if(!UITrainStage.keepTrain) yield break;
        CommandControl.CC.AttackList.Clear();
        bool isHttpWait = false;
        while (CommandControl.CC.ToActionPiece.Count > 0)
        {
            //��ȡ����
            OB_Piece currentPiece = CommandControl.CC.ToActionPiece.Dequeue();
            string command = "N/A";
            if(currentPiece.gameObject.name == lastPiece)
            {
                currentCommandTime++;
            }
            else
            {
                currentCommandTime = 0;
                lastPiece = currentPiece.gameObject.name;
            }

            if (currentCommandTime >= maxCommandTime) continue;
            //ǰ�򴫲�
            isHttpWait = true;
            HttpConnect.instance.SendBattleFieldEnv(currentPiece, x => {
                command = x;
                isHttpWait = false;
            });
            while(isHttpWait) yield return null; // ����

            //���򴫲�
            bool canBack;
            bool commandState = CommandControl.CC.CommandTranslate(command, currentPiece, out canBack);
            isHttpWait = true;
            if (canBack)
            {
                BackwardData BAKdata = new BackwardData(currentPiece, 0, commandState);
                HttpConnect.instance.SendCommandResult(BAKdata, x =>
                {
                    Debug.Log("����" + x);
                    isHttpWait = false;
                });
            }
            while (isHttpWait) yield return null;// ����

        }

        if(!UITrainStage.stepTrain) GameManager.GM.NextStage();
    }


}
