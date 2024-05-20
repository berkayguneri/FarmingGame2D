using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>,ISaveable
{
    private UIInventoryBar uiInventoryBar;

    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem; //the index of the array is the inventory list,and value is the item code 

    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityIntArray;

    [SerializeField] private SO_ItemList itemList = null;

    private string _iSaveableUniqueID;

    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameobjectSave _gameObjectSave;

    public GameobjectSave GameobjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory List
        CreateInventoryLists();

        //Creat item details dictionary
        CreatItemDetailsDictionary();

        //Initilise selected inventory item array
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameobjectSave = new GameobjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeRegister();
    }


    private void Start()
    {
        uiInventoryBar = FindObjectOfType<UIInventoryBar>();
    }


    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        //Initiliaze inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //Initiliaze player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    private void CreatItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();
        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameobjectToDelete)
    {
        AddItem(inventoryLocation, item);

        Destroy(gameobjectToDelete);
    }

    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryListt = inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item 
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryListt, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryListt, itemCode);
        }
        //Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //check if inventory already contains the item 
        int itemPositon = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPositon != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPositon);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }


    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        //DebugPrintInventoryList(inventoryList);
    }

    private void AddItemAtPosition(List<InventoryItem>inventoryList,int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantitiy = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemQuantity = quantitiy;
        inventoryItem.itemCode = itemCode;
        inventoryList[position] = inventoryItem;

        //DebugPrintInventoryList(inventoryList);
    }

    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        //If fromItem index and toItemIndex are within the bounds of the list, not the same, and greater than or equal to zero
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count
            && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];

            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;

            //Send event that inventory has been updated ---- Envanterin güncellendiðine dair event gönder
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }
    }

    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryLisst = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryLisst.Count; i++)
        {
            if (inventoryLisst[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }


    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1) //item secilmemisse
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
    }



    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    public string GetItemTypeDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_Tool:
                itemTypeDescription = Settings.BreakingTool;
                break;
            case ItemType.Chopping_Tool:
                itemTypeDescription = Settings.ChoppingTool;
                break;
            case ItemType.Hoeing_Tool:
                itemTypeDescription = Settings.HoeingTool;
                break;
            case ItemType.Reaping_Tool:
                itemTypeDescription = Settings.ReapingTool;
                break;
            case ItemType.Watering_Tool:
                itemTypeDescription = Settings.WateringTool;
                break;
            case ItemType.Collecting_Tool:
                itemTypeDescription = Settings.CollectingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;
        }
        return itemTypeDescription;
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }
    public void ISaveableDeRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameobjectSave ISaveableSave()
    {
        SceneSave sceneSave = new SceneSave();

        GameobjectSave.sceneData.Remove(Settings.PersistentScene);

        sceneSave.listInventoryItemArray = inventoryLists;

        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        GameobjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);
        return GameobjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameobjectSave gameobjectSave))
        {
            GameobjectSave = gameobjectSave;

            if (gameobjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.listInventoryItemArray != null)
                {
                    inventoryLists = sceneSave.listInventoryItemArray;

                    for (int i = 0; i <(int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }

                    Player.Instance.ClearCarriedItem();

                    uiInventoryBar.ClearHighlightOnInventorySlots();
                }

                if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[]
                    inventoryCapacityArray))
                {
                    inventoryListCapacityIntArray = inventoryCapacityArray;
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {

    }

    public void ISaveableRestoreScene(string sceneName)
    {

    }


    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryListt = inventoryLists[(int)inventoryLocation]; // olmazsa buraya bak

        //Check  if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryListt, itemCode, itemPosition);
        }

        //Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    public void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }

        else
        {
            inventoryList.RemoveAt(position);
        }
    }


    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    //private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    //{
    //    foreach (InventoryItem inventoryItem in inventoryList)
    //    {
    //        Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + "     Item Quantity: " + inventoryItem.itemQuantity);
    //    }
    //    Debug.Log("**************************************************");
    //}
}
