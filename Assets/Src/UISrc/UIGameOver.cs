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
                inf += "\n寒樱帝国保住了自己的火种。\n不久后流亡政府建立，寒樱帝国的人民依然回应自己祖国的召唤，只要灾难过去，一切都会好起来。";
                break;
            case EndingType.Good:
                inf += "<size=80>火种尚在</size>";
                inf += "\n寒樱帝国被灾难从地图上抹去了，但历史的载体尚未覆没。\n遗民会继承帝国的文化，在新的祖国延续故乡的火种。";
                break;
            case EndingType.Normal:
                inf += "<size=80>名存实亡</size>";
                inf += "\n寒樱帝国被灾难从地图上抹去了。\n虽然流亡政府成功建立，但已无人相应祖国的召唤。";
                break;
            case EndingType.Bad:
                inf += "<size=80>帝国覆灭</size>";
                inf += "\n寒樱帝国被灾难从地图上抹去了。\n这场灾难无人生还，只有城市的断壁残垣还在诉说这段历史。";
                break;
        }

        info.text = inf;
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
