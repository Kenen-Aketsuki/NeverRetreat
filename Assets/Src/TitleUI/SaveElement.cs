using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class SaveElement : MonoBehaviour
{
    SaveData data;

    static SaveElement _onlySelect = null;

    public static SaveElement onlySelect {
        get
        {
            return _onlySelect;
        }

        set
        {
            _onlySelect?.transform.GetChild(4).gameObject.SetActive(false);
            value?.transform.GetChild(4).gameObject.SetActive(true);

            _onlySelect = value;
            UITitleEvents.instance.saveDetail.text = value?.GetInfo();
        }
    }

    public SaveData getData()
    {
        return data;
    }

    public void setData(SaveData data)
    {
        this.data = data;
        transform.GetChild(0).GetComponent<TMP_Text>().text = data.saveTime;
        transform.GetChild(1).GetComponent<TMP_Text>().text = "�غ�: "+data.currentTurn + "/8";
        transform.GetChild(2).GetComponent<TMP_Text>().text = data.saveName;
        transform.GetChild(3).GetComponent<TMP_Text>().text = "��Ϸģʽ: "+data.gameMode;
    }

    public string GetInfo()
    {
        string info = "";
        info += "�浵����" + data.saveName;
        info += "\n<size=25>" + FixSystemData.SaveDirectory + data.SaveID + "</size>";
        info += "\n�ϴ�����ʱ�䣺" + data.saveTime;
        info += "\n��Ϸģʽ��" + data.gameMode;
        if(data.gameMode == "PVP")
        {
            info += "\nʹ������ģʽ���ж�ս";
        }else if (data.gameMode == "PVE")
        {
            info += "\n��AI��ս����ȷ��AI����������";
        }

        if (!data.canLoad)
        {
            info += "\n�浵��������ȱʧ���޷�����";
        }

        return info;
    }

    public void selectMe()
    {
        onlySelect = this;
    }
}

public class SaveData
{
    public string SaveID { get; private set; }
    public string saveName { get; private set; }
    public string gameMode { get; private set; }
    public string saveTime { get; private set; }
    public int currentTurn { get; private set; }

    public bool canLoad { get; private set; }

    public SaveData(XmlNode SaveData,string SaveID)
    {
        canLoad = true;
        this.SaveID = SaveID;
        
        XmlNode tmp = SaveData.SelectSingleNode("saveName");
        if (tmp != null) saveName = tmp.InnerText;
        else
        {
            saveName = "δ֪�浵";
            canLoad = false;
        }

        tmp = SaveData.SelectSingleNode("saveTime");
        if (tmp != null) saveTime = tmp.InnerText;
        else
        {
            saveTime = "0000/00/00";
            canLoad = false;
        }

        tmp = SaveData.SelectSingleNode("gameMode");
        if (tmp != null) gameMode = tmp.InnerText;
        else
        {
            gameMode = "ģʽȱʧ";
            canLoad = false;
        }

        tmp = SaveData.SelectSingleNode("currentTurn");
        int turn;
        if(tmp != null && int.TryParse(tmp.InnerText, out turn)) currentTurn = turn;
        else
        {
            currentTurn = -1;
        }
    }

    public SaveData(string saveID, string gameMode)
    {
        SaveID = saveID;
        saveName = saveID;
        this.gameMode = gameMode;
    }

    public void setSavingData(int turn)
    {
        currentTurn = turn;
        // ��ȡ��ǰʱ��  
        DateTime currentTime = DateTime.Now;
        saveTime = currentTime.ToString("yyyy/MM/dd");
    }
}