using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class RollAcademiy : MonoBehaviour
{
    void Start()
    {
        //StartCoroutine(GetRequest("http://127.0.0.1:5000/Rua"));
        StartCoroutine(PostRequest("http://127.0.0.1:5000/GiveRua", CreateJsonPayload("Yee")));
        StartCoroutine(PostRequest("http://127.0.0.1:5000/GiveRua", CreateJsonPayload("Rua")));
        StartCoroutine(PostRequest("http://127.0.0.1:5000/GiveRua", CreateJsonPayload("焯！")));
        //StartCoroutine(PostRequest("http://127.0.0.1:5000/Rua", CreateJsonPayload("Rua")));
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

    IEnumerator PostRequest(string url, string JsonMsg)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            // 设置请求头，指定发送的是JSON数据  
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //Accept - Charset: utf - 8
            webRequest.SetRequestHeader("Accept-Charset", "utf-8");


            // 设置请求正文为JSON数据  
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonMsg);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // 发送请求并等待完成  
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                byte[] dta = Encoding.GetEncoding("UTF-16BE").GetBytes(webRequest.downloadHandler.text);
                string decodedString = Encoding.GetEncoding("UTF-8").GetString(dta);

                // 显示响应结果  
                //Debug.Log(decodedString);
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    string CreateJsonPayload(string nam)
    {
        // 创建一个要发送的JSON对象  
        var data = new RuaJson();
        data.Rua = nam;

        // 将对象转换为JSON字符串  
        string jsonPayload = JsonUtility.ToJson(data);

        Debug.Log(jsonPayload);

        return jsonPayload;
    }

}

public class RuaJson
{
    public string Rua;
    [SerializeField]
    string yeet = "Don't";
}
