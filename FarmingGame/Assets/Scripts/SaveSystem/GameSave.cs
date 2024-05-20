using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSave 
{
    public Dictionary<string, GameobjectSave> gameObjectData;

    public GameSave()
    {
        gameObjectData = new Dictionary<string, GameobjectSave>();
    }
}
