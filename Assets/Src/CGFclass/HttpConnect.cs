using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HttpConnect : MonoBehaviour
{
    public static HttpConnect instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CheckConnect(Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);
        StartCoroutine(GetRequest(FixSystemData.AIUrl, callback));
    }

    public void InitServe(Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);
        StartCoroutine(GetRequest(FixSystemData.AIUrl + "/ServeInit", callback));
    }

    public void SendBattleFieldEnv(OB_Piece piece, Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);
        CGFDataJson priObj = new CGFDataJson(piece,GameManager.GM.ActionTargetPos);
        string json = CGFDataJson.CreateJson(priObj);

        StartCoroutine(PostRequest(FixSystemData.AIUrl + "/PostEnvData", json, callback));
    }

    public void SendCommandResult(BackwardData data, Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);

        string json = data.CreateJson();

        StartCoroutine(PostRequest(FixSystemData.AIUrl + "/AiBackword", json, callback));
    }

    public void UpdatePieceKey(Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);
        string json = JsonConvert.SerializeObject(FixSystemData.GlobalPieceDataList.Keys);

        StartCoroutine(PostRequest(FixSystemData.AIUrl + "UpdatePieceKey", json, callback));
    }

    public void JustGetRequest(string url, Action<string> callback = null)
    {
        if (callback == null) callback = x => Debug.Log("����˷���: " + x);
        StartCoroutine(GetRequest(url, callback));
    }

    IEnumerator GetRequest(string url, Action<string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.  
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Show results as text  
                //Debug.Log(webRequest.downloadHandler.text);
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                callback(webRequest.error);
            }
        }
    }

    IEnumerator PostRequest(string url,string Json,Action<string> callback)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            // ��������ͷ��ָ�����͵���JSON����  
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //Accept - Charset: utf - 8
            webRequest.SetRequestHeader("Accept-Charset", "utf-8");


            // ������������ΪJSON����  
            byte[] bodyRaw = Encoding.UTF8.GetBytes(Json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // �������󲢵ȴ����  
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Show results as text  
                //Debug.Log(webRequest.downloadHandler.text);
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                callback(webRequest.error);
            }
        }
    }

}
