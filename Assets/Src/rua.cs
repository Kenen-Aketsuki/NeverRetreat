using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class rua : MonoBehaviour
{
    [SerializeField]
    ArmyBelong loy = ArmyBelong.Human;

    public Tuple<string, Vector2Int, string, int, int, bool> getData()
    {
        Vector3Int pos = FixGameData.FGD.InteractMap.WorldToCell(gameObject.transform.position);
        Vector2Int pos2 = FixGameData.WorldToMap(pos);

        return new Tuple<string, Vector2Int, string, int, int, bool>(gameObject.name.Split(" ")[0], pos2, loy.ToString(), 0, 0, false);
    }
    
}
