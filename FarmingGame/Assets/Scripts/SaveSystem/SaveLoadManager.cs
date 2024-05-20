using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;


public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public GameSave gameSave;
    public List<ISaveable> iSaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }


    public void LoadDataFromFile()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/WildHopeCreek.dat"))
        {
            gameSave = new GameSave();

            FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Open);

            gameSave = (GameSave)bf.Deserialize(file);

            for (int i = iSaveableObjectList.Count-1 ; i > -1; i--)
            {
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID))
                {
                    iSaveableObjectList[i].ISaveableLoad(gameSave);
                }

                else
                {
                    Component component = (Component)iSaveableObjectList[i];
                    Destroy(component.gameObject);
                }
            }

            file.Close();
        }

        UIManager.Instance.DisablePauseMenu();
    }


    public void SaveDataToFile()
    {
        gameSave = new GameSave();

        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.ISaveableSave());
        }

        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Create);

        bf.Serialize(file, gameSave);

        file.Close();

        UIManager.Instance.DisablePauseMenu();
    }

    public void StoreCurrentSceneData()
    {
        //loop through all ISaveable objects and trigger store scene data for each
        //t�m ISaveable nesneleri aras�nda d�ng� olu�turun ve her biri i�in sahne verilerini depolamay� tetikleyin
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }

    }

    public void RestoreCurrentSceneData()
    {
        //loop through all ISaveable objects and trigger restore scene data for each
        //t�m ISaveable nesneleri aras�nda d�ng� olu�turun ve her biri i�in sahne verilerini geri y�klemeyi tetikleyin
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
}
