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
