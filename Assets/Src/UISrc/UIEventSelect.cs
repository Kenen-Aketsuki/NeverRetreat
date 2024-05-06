using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIEventSelect : MonoBehaviour
{
    [SerializeField]
    TMP_Text ContentText;
    [SerializeField]
    RectTransform ChoiceSet;
    [SerializeField]
    RectTransform ChosenSet;
    [SerializeField]
    GameObject normBtn;

    List<SpecialEvent> AvaliableList;
    List<Tuple<SpecialEvent,Vector3Int>> SelectedEvent = new List<Tuple<SpecialEvent, Vector3Int>>();


    SpecialEvent CurrentEvent;

    ArmyBelong LastCur = ArmyBelong.ModCrash;

    private void OnEnable()
    {
        if(GameManager.GM.ActionSide != LastCur)
        {
            SelectedEvent.Clear();
            LastCur = GameManager.GM.ActionSide;
        }

        if(GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            AvaliableList = FixGameData.FGD.HumanSpecialEventList;
        }
        else
        {
            AvaliableList = FixGameData.FGD.CrashSpecialEventList;
        }

        ChosenSet.GetChild(0).gameObject.SetActive(false);
        ChosenSet.GetChild(1).gameObject.SetActive(false);
        for(int i = 0; i < 4; i++)
        {
            if (i < AvaliableList.Count)
            {
                ChoiceSet.GetChild(i).gameObject.SetActive(true);
                ChoiceSet.GetChild(i).gameObject.name = AvaliableList[i].ToString();
                ChoiceSet.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = GameUtility.GetEventName(AvaliableList[i]);
            }
            else
            {
                ChoiceSet.GetChild(i).gameObject.SetActive(false);
            }
        }

        UpdateInfo();
    }

    public void windowClose()
    {
        gameObject.SetActive(false);
    }

    public void UpdateInfo()
    {
        ContentText.text = GameUtility.GetEventInfo(CurrentEvent);

        if (SelectedEvent.Count > 0)
        {
            GameObject btn1 = ChosenSet.GetChild(0).gameObject;
            btn1.name = SelectedEvent[0].Item1.ToString() + "\\"+ SelectedEvent[0].Item2.ToString();
            btn1.SetActive(true);
            btn1.transform.GetChild(0).GetComponent<TMP_Text>().text = GameUtility.GetEventName(SelectedEvent[0].Item1);
        }else ChosenSet.GetChild(0).gameObject.SetActive(false);

        if (SelectedEvent.Count > 1)
        {
            GameObject btn1 = ChosenSet.GetChild(1).gameObject;
            btn1.name = SelectedEvent[1].Item1.ToString() + "\\" + SelectedEvent[1].Item2.ToString();
            btn1.SetActive(true);
            btn1.transform.GetChild(0).GetComponent<TMP_Text>().text = GameUtility.GetEventName(SelectedEvent[1].Item1);
        }else ChosenSet.GetChild(1).gameObject.SetActive(false);
    }

    //崩坏方选项
    public void SelectEvent(GameObject evnN)
    {
        SpecialEvent evn = (SpecialEvent)Enum.Parse(typeof(SpecialEvent), evnN.name);

        if (CurrentEvent == evn && SelectedEvent.Count < 2)
        {
            SelectedEvent.Add(new Tuple<SpecialEvent, Vector3Int>(evn, Vector3Int.zero));
            if((int)CurrentEvent < 4)
            {
                FixGameData.FGD.uiIndex.HintUI.SetText("选择事件位置");
                FixGameData.FGD.uiIndex.HintUI.SetExitTime(2);

                GameManager.GM.SetMachineState(MachineState.SelectEventPosition);
                gameObject.SetActive(false);
            }
        }
        else if(CurrentEvent != evn)
        {
            CurrentEvent = evn;
        }

        UpdateInfo();
    }

    public void SelectedPos(Vector3Int Pos)
    {
        gameObject.SetActive(true);
        int addr = SelectedEvent.FindIndex(x => x.Item1 == CurrentEvent && x.Item2 == Vector3Int.zero);
        SelectedEvent[addr] = new Tuple<SpecialEvent, Vector3Int>(CurrentEvent, Pos);
        UpdateInfo();
        FixGameData.FGD.uiIndex.HintUI.gameObject.SetActive(false);
    }

    public void DeSelect(GameObject Btn)
    {
        SpecialEvent evn = (SpecialEvent)Enum.Parse(typeof(SpecialEvent), Btn.name.Split("\\")[0]);
        string[] tmpStr = Btn.name.Split("\\")[1].Replace(")", "").Replace("(", "").Split(",");

        Vector3Int pos = new Vector3Int(int.Parse(tmpStr[0]), int.Parse(tmpStr[1]), int.Parse(tmpStr[2]));

        SelectedEvent.RemoveAt(SelectedEvent.FindIndex(x => x.Item1 == evn && x.Item2 == pos));

        UpdateInfo();
    }

    public void ConformSelect()
    {
        if(GameManager.GM.ActionSide == ArmyBelong.Human)
        {
            GameManager.GM.HumanEventList = new List<Tuple<SpecialEvent, Vector3Int>>(SelectedEvent);
        }
        else
        {
            GameManager.GM.CrashEventList = new List<Tuple<SpecialEvent, Vector3Int>>(SelectedEvent);
        }

        gameObject.SetActive(false);
    }
}
