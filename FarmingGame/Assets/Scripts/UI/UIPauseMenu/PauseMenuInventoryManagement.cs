using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuInventoryManagement : MonoBehaviour
{
    [SerializeField] private PauseMenuInventoryManagementSlots[] inventoryManagementSlot = null;

    public GameObject inventoryManagementDraggedItemPrefab;

    [SerializeField] private Sprite transparent16x16;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

        if (InventoryManager.Instance != null)
        {
            PopulatePlayerInventory(InventoryLocation.player, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player]);
        }
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;
        DestroyInventoryTextBoxGameobject();
    }


    public void DestroyInventoryTextBoxGameobject()
    {
        if (inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryTextBoxGameobject);
        }
    }

    public void DestroyCurrentlyDraggedItem()
    {
        for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int) InventoryLocation.player].Count; i++)
        {
            if (inventoryManagementSlot[i].draggedItem != null)
            {
                Destroy(inventoryManagementSlot[i].draggedItem);
            }
        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            InitialiseInventoryManagementSlots();


            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)
            {
                inventoryManagementSlot[i].itemDetails = InventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);
                inventoryManagementSlot[i].itemQuantity = playerInventoryList[i].itemQuantity;

                if (inventoryManagementSlot[i].itemDetails != null)
                {
                    inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = inventoryManagementSlot[i].itemDetails.itemSprite;
                    inventoryManagementSlot[i].textMeshProUGUI.text = inventoryManagementSlot[i].itemQuantity.ToString();
                }

            }
        }
    }

    private void InitialiseInventoryManagementSlots()
    {
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(false);
            inventoryManagementSlot[i].itemDetails = null;
            inventoryManagementSlot[i].itemQuantity = 0;
            inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = transparent16x16;
            inventoryManagementSlot[i].textMeshProUGUI.text = "";
        }

        for (int i =InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.player]; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(true);
        }
    }
}
