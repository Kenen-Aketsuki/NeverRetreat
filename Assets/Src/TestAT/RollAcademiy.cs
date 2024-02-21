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
        StartCoroutine(PostRequest("http://127.0.0.1:5000/GiveRua", CreateJsonPayload("�̣�")));
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
            // ��������ͷ��ָ�����͵���JSON����  
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //Accept - Charset: utf - 8
            webRequest.SetRequestHeader("Accept-Charset", "utf-8");


            // ������������ΪJSON����  
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonMsg);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // �������󲢵ȴ����  
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                byte[] dta = Encoding.GetEncoding("UTF-16BE").GetBytes(webRequest.downloadHandler.text);
                string decodedString = Encoding.GetEncoding("UTF-8").GetString(dta);

                // ��ʾ��Ӧ���  
                //Debug.Log(decodedString);
                Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    string CreateJsonPayload(string nam)
    {
        // ����һ��Ҫ���͵�JSON����  
        var data = new RuaJson();
        data.Rua = nam;

        // ������ת��ΪJSON�ַ���  
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
