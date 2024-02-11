using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CameraMov : MonoBehaviour
{
    float addSpd;
    [SerializeField]
    Vector2 borderLD;
    [SerializeField]
    Vector2 borderRT;
    

    // Start is called before the first frame update
    void Start()
    {
        borderRT = new Vector2(2.16f * GameUtility.mapSize.x / 2, 2.16f * GameUtility.mapSize.y / 2);
        borderLD = borderRT * new Vector2(-1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W) && transform.position.y < borderRT.y) transform.position += new Vector3(0, 0.05f + addSpd, 0);
        if (Input.GetKey(KeyCode.S) && transform.position.y > borderLD.y) transform.position += new Vector3(0, -0.05f - addSpd, 0);
        if (Input.GetKey(KeyCode.A) && transform.position.x > borderLD.x) transform.position = transform.position + new Vector3(-0.05f - addSpd, 0, 0);
        if (Input.GetKey(KeyCode.D) && transform.position.x < borderRT.x) transform.position = transform.position + new Vector3(0.05f + addSpd, 0, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift)) addSpd = 0.05f;
        if (Input.GetKeyUp(KeyCode.LeftShift)) addSpd = 0;

        if (borderRT == Vector2.zero)
        {
            borderRT = new Vector2(2.16f * GameUtility.mapSize.x / 2.5f, 2.16f * GameUtility.mapSize.y / 2);
            borderLD = borderRT * new Vector2(-1, -1);
        }
    }
}
