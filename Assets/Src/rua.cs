using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class rua : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
