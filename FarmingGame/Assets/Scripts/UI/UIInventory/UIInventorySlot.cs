using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    private Camera mainCamera;
    private Transform parentItem;
    public GameObject draggedItem;
    private GridCursor gridCursor;
    private Cursor cursor;
    private Canvas parentCanvas;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    
    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject itemPrefab = null;
    [SerializeField] private int slotNumber = 0;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [HideInInspector] public bool isSelected = false;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventoryEvent;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventoryEvent;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
    }
    private void Start()
    {
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
    }

    private void ClearCursors()
    {
        gridCursor.DisableCursor();
        cursor.DisableCursor();

        gridCursor.SelectedItemType = ItemType.none;
        cursor.SelectedItemType = ItemType.none;
    }
    public void SetSelectedItem()
    {
        //Clear currently highlighted items ---- �u anda vurgulanan ��eleri temizle
        inventoryBar.ClearHighlightOnInventorySlots();

        //Highlighted item on inventory --- Envanterde vurgulanan ��e
        isSelected = true;

        //Set highlighted inventory slots ---- Vurgulanan envanter yuvalar�n� ayarlama
        inventoryBar.SetHighlightedInventorySlots();

        //set use radius for cursors
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;
        cursor.ItemUseRadius = itemDetails.itemUseRadius;

        //if item requires a grid cursor then enable cursor--- ��e bir �zgara imleci gerektiriyorsa imleci etkinle�tirin
        if (itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        //if item requires a cursor then enable cursor
        if (itemDetails.itemUseRadius > 0f)
        {
            cursor.EnableCursor();
        }
        else
        {
            cursor.DisableCursor();
        }

        //set item type
        gridCursor.SelectedItemType = itemDetails.itemType;
        cursor.SelectedItemType = itemDetails.itemType;

        //Set item selected in inventory ---- Envanterde se�ilen ��eyi ayarla
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);

        if (itemDetails.canBeCarried == true)
        {
            //Show player carrying item
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        }
        else // show player carrying nothing
        {
            Player.Instance.ClearCarriedItem();
        }
    }

    public void ClearSelectedItem()
    {

        ClearCursors();

        //Clear currently highlighted items ---- Su anda vurgulanan ��eleri temizle
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        //set no item selected in inventory ---- envanterde se�ili ��e yok olarak ayarla
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);

        //Clear player carrying item
        Player.Instance.ClearCarriedItem();
    }


    //drop the item (if selected) at the current mouse position. Called by the DropItem evet
    //��eyi (se�iliyse) ge�erli fare konumuna b�rak�r. DropItem taraf�ndan �a�r�l�r 
    private void DropSelectedItemAtMousePosition()
    {
        if (itemDetails != null && isSelected)
        {           

            if (gridCursor.CursorPositionIsValid)
            {
                Vector3 worldPoisiton = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
                //Create item from prefab at mouse position ----- Fare konumunda prefab ��e olu�turma
                GameObject itemGameobject = Instantiate(itemPrefab,new Vector3(worldPoisiton.x,worldPoisiton.y-Settings.gridCellSize/2f,worldPoisiton.z), Quaternion.identity, parentItem);
                Item item = itemGameobject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                //Remove  item from player inventory --- Oyuncu envanterinden ��eyi kald�r
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

                //If no more of item the clear selected ---- Se�ilen ��e kalmad�ysa temizle 
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }

           
        }
    }
 
    private void RemoveSelectedItemFromInventoryEvent()
    {
        if (itemDetails != null && isSelected)
        {
            int itemCode = itemDetails.itemCode;

            //remove item from player inventory
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);


            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            //Disable keyboard input
            Player.Instance.DisablePlayerInputAndResetMovement();

            //Instantiate gameobject as dragged item  ---- S�r�klenen ��e olarak gameobject olu�turun  
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Get image for dragged item ---- S�r�klenen ��e i�in resim al
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            SetSelectedItem();

        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Move gameobject  as dragged item ----- Oyun nesnesini s�r�klenen ��e olarak ta��
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       //Destroy gameobject as dragged item
       if (draggedItem != null)
       {
            Destroy(draggedItem);

            //If drag ends over inventory bar,get item drag is over and swap them ---- E�er s�r�kleme envanter �ubu�unun �zerinde biterse, s�r�klenen ��eyi al ve de�i�tir
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                //get to slot number where the drag ended ---- s�r�klemenin sona erdi�i yuva numaras�na git
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;


                //Swap inventory items in invetory list ---- Envanter listesindeki envanter ��elerini de�i�tir
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                //Destroy Inventory text box
                DestroyInventoryTextBox();

                //Clear selected item
                ClearSelectedItem();
            }

            //else attempt to drop the item if it can be dropped ----- d���r�lebiliyorsa ��eyi d���rmeye �al���n
            else
            {
                if (itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }

            //Enable player input
            Player.Instance.EnablePlayerInput();
          
       }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Populate text box with item details ----- Metin kutusunu ��e ayr�nt�lar�yla doldurun
        if (itemQuantity != 0)
        {
            //Instantiate inventory text box ---- Envanter olu�tur metin kutusu
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            //Set item type description ---- ��e t�r� a��klamas�n� ayarla 
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            //Populate text box ---- Metin kutusunu doldur
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            //Set the text box position according to inventory bar position ------ Metin kutusu konumunu envanter �ubu�u konumuna g�re ayarlay�n
            if (inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);

            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }

    public void DestroyInventoryTextBox()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if the click ---- eger tiklanmissa
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //if inventory slot currently selected then deselect ----- envanter yuvas� o anda se�iliyse se�imi kald�r�n
            if (isSelected == true)
            {
                ClearSelectedItem();
            }
            else
            {
             if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }
}
