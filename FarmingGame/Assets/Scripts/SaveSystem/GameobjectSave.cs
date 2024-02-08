using System.Collections.Generic;

[System.Serializable]

public class GameobjectSave
{
    //string key = scene name
    public Dictionary<string, SceneSave> sceneData;

    public GameobjectSave()
    {
        sceneData = new Dictionary<string, SceneSave>();
    }
    public GameobjectSave(Dictionary<string, SceneSave> sceneData)
    {
        this.sceneData = sceneData;
    }
}