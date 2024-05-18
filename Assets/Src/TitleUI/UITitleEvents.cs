using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UITitleEvents : MonoBehaviour
{
    public static UITitleEvents instance;

    Animator ani;
    [SerializeField]
    GameObject saveElement;
    [SerializeField]
    Transform saveParent;
    [SerializeField]
    public TMP_Text saveDetail;
    [SerializeField]
    bool faceAI = false;
    [SerializeField]
    string saveName;
    [SerializeField]
    int UnNamedCount;

    private void Awake()
    {
        instance = this;
        ani = GetComponent<Animator>();
        FixSystemData.InitPath();
        BasicUtility.DataInit();
        ani.SetBool("startGame", false);

        UnNamedCount = 0;
        foreach (string path in Directory.GetDirectories(FixSystemData.SaveDirectory))
        {
            if (Regex.Match(path, @"��������\d+").Success)
            {
                UnNamedCount++;
            }
        }

    }

    public void GameInit(bool isNew)
    {
        if (SaveElement.onlySelect == null && !isNew) return;

        if (SaveElement.onlySelect?.getData().canLoad ?? false)
        {
            GameUtility.fromSave = true;
            GameUtility.Save = SaveElement.onlySelect.getData().SaveID;
            GameUtility.saveData = SaveElement.onlySelect.getData();
        }
        else if (isNew)
        {
            //���ɴ浵����
            if (saveName == "")
            {
                saveName = $"��������{UnNamedCount}";
                while (Directory.Exists(FixSystemData.SaveDirectory + saveName))
                {
                    UnNamedCount++;
                    saveName = $"��������{UnNamedCount}";
                }
            }

            Debug.Log(saveName);
            GameUtility.fromSave = false;
            GameUtility.Save = "";
            GameUtility.saveData = new SaveData(saveName, faceAI ? "PVE" : "PVP");
        }

        ani.SetBool("startGame", true);
    }

    public void NewGame()
    {
        ani.SetBool("DisStart", true);
        ani.SetBool("DisNewGame", true);
        faceAI = false;
    }

    public void LoadFromSave()
    {
        ani.SetBool("DisStart", true);
        ani.SetBool("DisSaveSelect", true);

        List<SaveData> datas = BasicUtility.LoadSaves();
        StartCoroutine(UpdateSaveList(datas));
    }

    public void BackToTitle()
    {
        ani.SetBool("DisStart", false);
        ani.SetBool("DisSaveSelect", false);
        ani.SetBool("DisNewGame", false);
    }

    public void loadGameAfterAni()
    {
        StartCoroutine(StartGame());
    }

    public void saveNameSet(TMP_Text change)
    {
        Debug.Log(change.text);
        saveName = change.text;
    }

    public void gameModeChange(TMP_Text txt)
    {
        faceAI = !faceAI;
        txt.text = "��Ϸģʽ��" + (faceAI ? "PVE" : "PVP");
    }

    public void checkHttpConnection(TMP_Text txt)
    {
        txt.text = "�����С�";
        Action<string> callback = x =>
        {
            if(x == "OK")
            {
                txt.text = "��������";
            }
            else
            {
                txt.text = "���ӳ���";
            }
            
        };
        HttpConnect.instance.InitServe(callback);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator UpdateSaveList(List<SaveData> saveList)
    {
        for(int i = 0; i < saveParent.childCount; i++)
        {
            Destroy(saveParent.GetChild(i).gameObject);
        }
        yield return null;
        SaveElement.onlySelect = null;
        foreach (SaveData data in saveList)
        {
            GameObject saveItem = Instantiate(saveElement, saveParent);
            saveItem.GetComponent<SaveElement>().setData(data);
            saveItem.name = data.SaveID;
        }
    }

    IEnumerator StartGame()
    {
        // ��ʼ�첽���س���  
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameSense", LoadSceneMode.Single);

        // �ȴ�ֱ������������ɣ�����Ը�����Ҫ��Ӹ�����߼����������UI��  
        while (!asyncLoad.isDone)
        {

            // �ȴ�ֱ����һ֡  
            yield return null;
        }
    }
}
