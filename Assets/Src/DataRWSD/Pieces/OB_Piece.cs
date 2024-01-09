using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OB_Piece : MonoBehaviour
{
    public Piece Data;//Æå×ÓÊý¾Ý

    [SerializeField]
    SpriteRenderer BaseColor_Normal;
    [SerializeField]
    SpriteRenderer BaseColor_Casualty;
    [SerializeField]
    GameObject CrashCover;
    [SerializeField]
    SpriteRenderer TroopType;
    [SerializeField]
    Transform TextMessage;
    // Start is called before the first frame update
    void Start()
    {
        CrashCover.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        TextMessage.position = this.transform.position;
    }


}
