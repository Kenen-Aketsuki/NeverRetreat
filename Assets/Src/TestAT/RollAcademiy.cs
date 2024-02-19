using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RollAcademiy : MonoBehaviour
{
    void Start()
    {
        //StartCoroutine(GetRequest("http://127.0.0.1:5000/Rua"));
        StartCoroutine(PostRequest("http://127.0.0.1:5000/GiveRua", new WWWForm()));
    }

    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.  
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Show results as text  
                Debug.Log(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError(webRequest.error);
            }
        }
    }

    IEnumerator PostRequest(string url, WWWForm form)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            // ��������ͷ�������Ҫ�Ļ�  
            // webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");  
            // webRequest.SetRequestHeader("Authorization", "Bearer YOUR_TOKEN_HERE");  

            // ��ӱ�����  
            form.AddField("key1", "value1");
            form.AddField("key2", "value2");

            // ������������Ϊ������  
            byte[] bodyRaw = form.data;
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // �������󲢵ȴ����  
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // ��ʾ��Ӧ���  
                Debug.Log(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError(webRequest.error);
            }
        }
    }

}
