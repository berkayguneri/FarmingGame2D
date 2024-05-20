using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIInventoryBar : MonoBehaviour
{
    [SerializeField] private Sprite blank16x16sprite = null;
    [SerializeField] private UIInventorySlot[] inventorySlot = null;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;

    public GameObject inventoryBarDraggedItem;

    private RectTransform rectTransform;

    public bool _isInventoryBarPositionBottom = true;

    public bool IsInventoryBarPositionBottom { get => _isInventoryBarPositionBottom; set => _isInventoryBarPositionBottom = value; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }
    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void Update()
    {
        //Switch inventory bar position depending on player position ---- Oyuncu konumuna baðlý olarak envanter çubuðu konumunu deðiþtirme
        SwitchInventoryBarPosition();
    }

    public void ClearHighlightOnInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            //loop through inventory slots and clear highligh sprites ---- envanter yuvalarý arasýnda döngü oluþtur ve vurgulu sprite'larý temizle
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                if (inventorySlot[i].isSelected)
                {
                    inventorySlot[i].isSelected = false;
                    inventorySlot[i].inventorySlotHighlight.color = new Color(0f, 0f, 0f, 0f);

                    //Update inventory to show item as not selected ----- Öðeyi seçili deðil olarak göstermek için envanteri güncelle
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                }
            }
        }
    }

    private void ClearInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            //loop through inventory slots and update with blank sprite ---- envanter yuvalarý arasýnda döngü ve boþ sprite ile güncelleme
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                inventorySlot[i].inventorySlotImage.sprite = blank16x16sprite;
                inventorySlot[i].textMeshProUGUI.text = "";
                inventorySlot[i].itemDetails = null;
                inventorySlot[i].itemQuantity = 0;

                SetHighlightedInventorySlots(i); 
            }
        }
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation,List<InventoryItem> inventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            ClearInventorySlots();

            if (inventorySlot.Length > 0 && inventoryList.Count > 0)
            {
                //loop through inventory slots and update with corresponding invetory list item 
                //envanter yuvalarý arasýnda döngü oluþturun ve ilgili envanter listesi öðesiyle güncelleyin
                for (int i = 0; i < inventorySlot.Length; i++)
                {
                    if (i < inventoryList.Count)
                    {
                        int itemCode = inventoryList[i].itemCode;

                        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                        if (itemDetails != null)
                        {
                            inventorySlot[i].inventorySlotImage.sprite = itemDetails.itemSprite;
                            inventorySlot[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                            inventorySlot[i].itemDetails = itemDetails;
                            inventorySlot[i].itemQuantity = inventoryList[i].itemQuantity;
                            SetHighlightedInventorySlots(i);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    //Set the selected highlight if set on all inventory item positions ---- Tüm envanter öðesi konumlarýnda ayarlanmýþsa seçili vurguyu ayarlayýn
    public void SetHighlightedInventorySlots()
    {
        if (inventorySlot.Length>0)
        {
            //loop through inventory slots and clear highlight sprites ----- envanter yuvalarý arasýnda döngü oluþtur ve vurgulu sprite'larý temizle
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                SetHighlightedInventorySlots(i);
            }
        }
    }

    //Set the selected highlight if set on inventory item for a given slot item position
    //Belirli bir yuva öðesi konumu için envanter öðesinde ayarlanmýþsa seçili vurguyu ayarlayýn
    public void SetHighlightedInventorySlots(int itemPosition)
    {
        if (inventorySlot.Length > 0 && inventorySlot[itemPosition].itemDetails != null)
        {
            if (inventorySlot[itemPosition].isSelected)
            {
                inventorySlot[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f);

                //Update inventory to show item as selected ---- Öðeyi seçili olarak göstermek için envanteri güncelle
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, inventorySlot[itemPosition].itemDetails.itemCode);
            }          
        }
    }


    private void SwitchInventoryBarPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();

        if (playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false)
        {
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if (playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionBottom = false;
        }

    }

    public void DestroyCurrentlyDraggedItems()
    {
        for (int i = 0; i < inventorySlot.Length; i++)
        {
            if (inventorySlot[i].draggedItem != null)
            {
                Destroy(inventorySlot[i].draggedItem);
            }
        }
    }

    public void ClearCurrentlySelectedItems()
    {
        for (int i = 0; i < inventorySlot.Length; i++)
        {
            inventorySlot[i].ClearSelectedItem();
        }
    }
}
