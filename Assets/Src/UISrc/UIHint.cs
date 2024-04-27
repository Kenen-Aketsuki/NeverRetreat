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

    private void OnEnable()
    {
        if (exitTime == -1)
        {
            exitTime = 2;
        }
    }

    private void Update()
    {
        if (exitTime > 0)
        {
            exitTime -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetText(string str)
    {
        textArea.text = str;
        exitTime = -1;
    }

    public void SetExitTime(float time)
    {
        exitTime = time;
        gameObject.SetActive(true);
    }
}
