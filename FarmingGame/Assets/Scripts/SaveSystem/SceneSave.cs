using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    public Dictionary<string, bool> boolDictionary;  // string key  is an identifier name we choose for this list
    //string key is an identifier name we choose for this list --- string key bu liste i�in se�ti�imiz bir tan�mlay�c� ad�d�r
    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
}