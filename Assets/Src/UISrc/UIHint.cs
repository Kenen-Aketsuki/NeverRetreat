using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHint : MonoBehaviour
{
    [SerializeField]
    TMP_Text textArea;

    public void SetText(string str)
    {
        textArea.text = str;
    }
}
