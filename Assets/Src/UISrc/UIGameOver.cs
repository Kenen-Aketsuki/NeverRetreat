using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameOver : MonoBehaviour
{
    [SerializeField]
    Animator ani;
    [SerializeField]
    TMP_Text info;
    [SerializeField]
    EndingType endingType;

    private void OnEnable()
    {
        //endingType = FixGameData.FGD.resultMem.GetEndType();
        setInfo();
        StartCoroutine(MoveCam());
    }

    IEnumerator MoveCam()
    {
        float currZoom = FixGameData.FGD.CameraNow.orthographicSize;
        float tarZoom = 30;
        Func<float, float, float, float> floatLerp = (a, b, t) =>
        {
            return a + (b - a) * t;
        };

        Vector3 camPos = FixGameData.FGD.CameraNow.transform.position;
        Vector3 endPos = new Vector3(0, 0, camPos.z);
        float totalTime = 1;
        float curTime = 0;
        while(curTime < totalTime)
        {
            float t = curTime / totalTime;
            FixGameData.FGD.CameraNow.transform.position = Vector3.Lerp(camPos, endPos, t);
            FixGameData.FGD.CameraNow.orthographicSize = floatLerp(currZoom, tarZoom, t);
            curTime += Time.deltaTime;
            yield return null;
        }
        
    }

    void setInfo()
    {
        string inf = "";
        switch (endingType)
        {
            case EndingType.Perfict:
                inf += "<size=80>ILS NE PASSERONT PAS</size>";
                inf += "\n��ӣ�۹���ס���Լ��Ļ��֡�\n���ú�����������������ӣ�۹���������Ȼ��Ӧ�Լ�������ٻ���ֻҪ���ѹ�ȥ��һ�ж����������";
                break;
            case EndingType.Good:
                inf += "<size=80>��������</size>";
                inf += "\n��ӣ�۹������Ѵӵ�ͼ��Ĩȥ�ˣ�����ʷ��������δ��û��\n�����̳е۹����Ļ������µ������������Ļ��֡�";
                break;
            case EndingType.Normal:
                inf += "<size=80>����ʵ��</size>";
                inf += "\n��ӣ�۹������Ѵӵ�ͼ��Ĩȥ�ˡ�\n��Ȼ���������ɹ�����������������Ӧ������ٻ���";
                break;
            case EndingType.Bad:
                inf += "<size=80>�۹�����</size>";
                inf += "\n��ӣ�۹������Ѵӵ�ͼ��Ĩȥ�ˡ�\n�ⳡ��������������ֻ�г��еĶϱڲ�ԫ������˵�����ʷ��";
                break;
        }

        info.text = inf;
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
