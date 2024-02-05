using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMov : MonoBehaviour
{
    float addSpd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) transform.position = transform.position + new Vector3 (0, 0.05f + addSpd, 0);
        if (Input.GetKey(KeyCode.S)) transform.position = transform.position + new Vector3(0, -0.05f - addSpd, 0);
        if (Input.GetKey(KeyCode.A)) transform.position = transform.position + new Vector3(-0.05f - addSpd, 0, 0);
        if (Input.GetKey(KeyCode.D)) transform.position = transform.position + new Vector3(0.05f + addSpd, 0, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift)) addSpd = 0.05f;
        if (Input.GetKeyUp(KeyCode.LeftShift)) addSpd = 0;

    }
}
