using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    //数据区
    //当前行动方
    public ArmyBelong ActionSide;
    


    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }

    
}
