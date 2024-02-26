using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIHandler
{
    abstract public void OnPieceSelect(bool isFriendly);

    abstract public void OnTerrainSelect(bool isFac);

    abstract public void OnPositionSelect(Vector3Int Pos);

    abstract public void UpdateShow();

    abstract public string WhatShouldIDo();

    
}
