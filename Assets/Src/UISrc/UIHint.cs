using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHint : MonoBehaviour
{
    [SerializeField]
    TMP_Text textArea;
    [SerializeField]
    float exitTime;
    bool startCount;

    private void Update()
    {
        if (startCount)
        {
            if(exitTime > 0)
            {
                exitTime -= Time.deltaTime;
            }
            else
            {
                startCount = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void SetText(string str)
    {
        textArea.text = str;
    }

    public void SetExitTime(float time)
    {
        exitTime = time;
        startCount = true;
        gameObject.SetActive(true);
    }
}
