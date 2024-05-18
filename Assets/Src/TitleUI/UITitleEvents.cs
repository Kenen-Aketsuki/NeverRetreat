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
            if (Regex.Match(path, @"誓死坚守\d+").Success)
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
            //生成存档名称
            if (saveName == "")
            {
                saveName = $"誓死坚守{UnNamedCount}";
                while (Directory.Exists(FixSystemData.SaveDirectory + saveName))
                {
                    UnNamedCount++;
                    saveName = $"誓死坚守{UnNamedCount}";
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
        txt.text = "游戏模式：" + (faceAI ? "PVE" : "PVP");
    }

    public void checkHttpConnection(TMP_Text txt)
    {
        txt.text = "连接中…";
        Action<string> callback = x =>
        {
            if(x == "OK")
            {
                txt.text = "正常连接";
            }
            else
            {
                txt.text = "连接出错";
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
        // 开始异步加载场景  
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameSense", LoadSceneMode.Single);

        // 等待直到场景加载完成（你可以根据需要添加更多的逻辑，比如更新UI）  
        while (!asyncLoad.isDone)
        {

            // 等待直到下一帧  
            yield return null;
        }
    }
}
