using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    List<SpecialEvent> SelectedEvent = new List<SpecialEvent>();
    SpecialEvent CurrentEvent;

    ArmyBelong LastCur = ArmyBelong.ModCrash;

    int cost = 3;

    private void OnEnable()
    {
        if(GameManager.GM.ActionSide != LastCur)
        {
            cost = 3;
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
            btn1.name = SelectedEvent[0].ToString();
            btn1.SetActive(true);
            btn1.transform.GetChild(0).GetComponent<TMP_Text>().text = GameUtility.GetEventName(SelectedEvent[0]);
        }else ChosenSet.GetChild(0).gameObject.SetActive(false);

        if (SelectedEvent.Count > 1)
        {
            GameObject btn1 = ChosenSet.GetChild(1).gameObject;
            btn1.name = SelectedEvent[1].ToString();
            btn1.SetActive(true);
            btn1.transform.GetChild(0).GetComponent<TMP_Text>().text = GameUtility.GetEventName(SelectedEvent[1]);
        }else ChosenSet.GetChild(1).gameObject.SetActive(false);
    }

    //±À»µ·½Ñ¡Ïî
    public void SelectEvent(GameObject evnN)
    {
        SpecialEvent evn = (SpecialEvent)Enum.Parse(typeof(SpecialEvent), evnN.name);
        int needCost;
        if (evn == SpecialEvent.MentalAD || evn == SpecialEvent.PosConfuse || evn == SpecialEvent.DataStrom)
        {
            needCost = 2;
        }
        else needCost = 1;

        if (CurrentEvent == evn && cost - needCost >= 0 && SelectedEvent.Count < 2)
        {
            SelectedEvent.Add(evn);
            cost -= needCost;
        }
        else if(CurrentEvent != evn)
        {
            CurrentEvent = evn;
        }

        UpdateInfo();
    }

    public void DeSelect(GameObject Btn)
    {
        SpecialEvent evn = (SpecialEvent)Enum.Parse(typeof(SpecialEvent), Btn.name);

        SelectedEvent.Remove(evn);
        if (evn == SpecialEvent.MentalAD || evn == SpecialEvent.PosConfuse || evn == SpecialEvent.DataStrom)
        {
            cost += 2;
        }
        else cost += 1;

        UpdateInfo();
    }

    public void ConformSelect()
    {
        if(LastCur == ArmyBelong.Human)
        {
            FixGameData.FGD.HumanSpecialEventList = SelectedEvent;
        }
        else
        {
            FixGameData.FGD.CrashSpecialEventList = SelectedEvent;
        }

        gameObject.SetActive(false);
    }
}
