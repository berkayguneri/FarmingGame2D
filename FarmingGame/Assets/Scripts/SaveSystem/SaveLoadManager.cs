using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public List<ISaveable> iSaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }

    public void StoreCurrentSceneData()
    {
        //loop through all ISaveable objects and trigger store scene data for each
        //tüm ISaveable nesneleri arasýnda döngü oluþturun ve her biri için sahne verilerini depolamayý tetikleyin
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }

    }

    public void RestoreCurrentSceneData()
    {
        //loop through all ISaveable objects and trigger restore scene data for each
        //tüm ISaveable nesneleri arasýnda döngü oluþturun ve her biri için sahne verilerini geri yüklemeyi tetikleyin
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
}
