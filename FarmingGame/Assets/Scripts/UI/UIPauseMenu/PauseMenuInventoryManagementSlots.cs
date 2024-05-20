using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuInventoryManagementSlots : MonoBehaviour ,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Image inventoryManagementSlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    public GameObject greyedOutImageGO;
    [SerializeField] private PauseMenuInventoryManagement invetoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;

    //private Vector3 startingPosition;
    public GameObject draggedItem;
    private Canvas parentCanvas;


    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       if (itemQuantity != 0)
        {
            draggedItem = Instantiate(invetoryManagement.inventoryManagementDraggedItemPrefab, invetoryManagement.transform);

            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlots>() != null)
            {
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlots>().slotNumber;

                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                invetoryManagement.DestroyInventoryTextBoxGameobject();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemQuantity != 0)
        {
            invetoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            invetoryManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = invetoryManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            if (slotNumber > 23)
            {
                invetoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector3(0.5f, 0f);
                invetoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                invetoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector3(0.5f, 1f);
                invetoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        invetoryManagement.DestroyInventoryTextBoxGameobject();
    }
}
