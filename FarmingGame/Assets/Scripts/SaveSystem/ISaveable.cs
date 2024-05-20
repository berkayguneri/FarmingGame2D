public interface ISaveable
{
    string ISaveableUniqueID { get; set; }

    GameobjectSave GameobjectSave { get; set; }

    void ISaveableRegister();

    void ISaveableDeRegister();

    GameobjectSave ISaveableSave();

    void ISaveableLoad(GameSave gameSave);

    void ISaveableStoreScene(string sceneName);

    void ISaveableRestoreScene(string sceneName);


}