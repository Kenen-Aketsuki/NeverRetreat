using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    
    public ArmyBelong ActionSide;


    // Start is called before the first frame update
    void Start()
    {
        GM = this;
    }

    
}
