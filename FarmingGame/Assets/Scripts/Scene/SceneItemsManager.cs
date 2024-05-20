using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>,ISaveable
{
    private Transform parentItem;
    [SerializeField] private GameObject itemPrefab = null;

    private string _iSaveableUniqueID;

    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameobjectSave _gameObjectSave;

    public GameobjectSave GameobjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    private void AfterSceneLoad()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameobjectSave = new GameobjectSave();
    }

    private void DestroySceneItems()
    {
        //Get all the items in the scene

        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        //loop through all scene items and destroy them---t�m sahne ��eleri aras�nda d�ng� olu�turun ve onlar� yok edin
        for (int i = itemsInScene.Length - 1; i > -1 ;i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    public void InstantiateSceneItem(int itemCode,Vector3 itemPoisiton)
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPoisiton, Quaternion.identity,parentItem);
        Item item = itemGameObject.GetComponent<Item>();
        item.Init(itemCode);
    }

    private void InstantiateScenesItem(List<SceneItem> sceneItemList)
    {
        GameObject itemGameobject;

        foreach (SceneItem sceneItem in sceneItemList)
        {
            itemGameobject = Instantiate(itemPrefab, new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), Quaternion.identity, parentItem);

            Item item = itemGameobject.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    private void OnDisable()
    {
        ISaveableDeRegister();

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    public void ISaveableDeRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameobjectSave gameobjectSave))
        {
            GameobjectSave = gameobjectSave;

            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameobjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.listSceneItem != null)
            {
                //scene list items found - destroy existing items in scene --- sahne listesi ��eleri bulundu - sahnedeki mevcut ��eleri yok et
                DestroySceneItems();

                //now instantiate the list of scene items --- �imdi sahne ��elerinin listesini haz�rlay�n
                InstantiateScenesItem(sceneSave.listSceneItem);
            }
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }


    public GameobjectSave ISaveableSave()
    {
        ISaveableStoreScene(SceneManager.GetActiveScene().name);
        return GameobjectSave;
    }

    public void ISaveableStoreScene(string sceneName)
    {
        //Remove old scene save for gameobject if exist --- Varsa gameobject i�in eski sahne kayd�n� kald�r
        GameobjectSave.sceneData.Remove(sceneName);

        //get all items in the scene --- sahnedeki t�m ��eleri al
        List<SceneItem> sceneItemList = new List<SceneItem>();
        Item[] itemsInScene = FindObjectsOfType<Item>();

        //loop through all scene items --- t�m sahne ��eleri aras�nda d�ng�
        foreach (Item item in itemsInScene)
        {
            SceneItem sceneItem = new SceneItem();
            sceneItem.itemCode = item.ItemCode;
            sceneItem.position = new Vector3Serilazilble(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            sceneItem.itemName = item.name;

            //Add scene item to list
            sceneItemList.Add(sceneItem);
        }

        //Create list scene items dictionary in scene save and add to it
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneItem = sceneItemList;

        //add scene save to gameobject
        GameobjectSave.sceneData.Add(sceneName, sceneSave);
    }

   
}
