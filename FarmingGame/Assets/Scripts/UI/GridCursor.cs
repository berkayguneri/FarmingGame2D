using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;

    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private SO_CropDetailsList so_cropDetailsList = null;

    private bool _cursorPositionIsValid = false;

    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }

    private int _itemUseGridRadius = 0;

    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }

    private ItemType _selectedItemType;

    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    private bool _cursorIsEnabled = false;

    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            //get grid position for cursor
            Vector3Int gridPosition = GetGridPositionForCursor();

            //get grid position for player
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            //set cursor sprite
            SetCursorValidity(gridPosition, playerGridPosition);

            //get rect transform position for cursor
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();

        //check item use radius is valid -- öðe kullaným yarýçapýnýn geçerli olup olmadýðýný kontrol edin
        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();
            return;
        }
        //get selected item details -- seçilen öðe ayrýntýlarýný al
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInvalid();
            return;
        }

        //get grid property details at cursor position --- imlec konumundaki grid özellik ayrýntýlarýný al
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            //determine cursor validity based on inventory item selected and grid property details
            //Seçilen envanter öðesine ve grid özellik ayrýntýlarýna göre imleç geçerliliðini belirleme
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:

                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Commodity:

                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Watering_Tool:
                case ItemType.Breaking_Tool:
                case ItemType.Chopping_Tool:
                case ItemType.Hoeing_Tool:
                case ItemType.Reaping_Tool:
                case ItemType.Collecting_Tool:

                    if (!IsCursorValidForTool(gridPropertyDetails,itemDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;


                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
        else
        {
            SetCursorToInvalid();
            return;
        }
    }

    private void SetCursorToInvalid()
    {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }

    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    //set cursor validity for a commodity for the target gridPropertyDetails. Returns true if valid,false if invalid
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    //set cursor validity for a seed for the target gridPropertyDetails. Returns true if valid,false if invalid
    //Hedef gridPropertyDetails için bir tohum için imleç geçerliliðini ayarlar. Geçerliyse true, geçersizse false döndürür
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails,ItemDetails itemDetails)
    {
        //switch on tool
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_Tool:
                if (gridPropertyDetails.isDiggable == true && gridPropertyDetails.daysSinceDug == -1)
                {
                    #region Need to get any items at location so we can check if they are reapable
                    //Yeniden kullanýlabilir olup olmadýklarýný kontrol edebilmemiz için herhangi bir öðeyi yerinde almamýz gerekiyor

                    //get world position for cursor
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f , GetWorldPositionForCursor().y + 0.5f, 0f);

                    //get list of items at cursor location
                    List<Item> itemList = new List<Item>();


                    HelperMethods.GetComponentAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);
                    #endregion

                    //loop through items found to see if any are reapable type- we aren't going to let the player dig where there are reapable scenary items
                    bool foundReapable = false;

                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reaping_Tool)
                        {
                            foundReapable = true;
                            break;
                        }
                    }
                    
                    if (foundReapable)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

            case ItemType.Watering_Tool:
                if(gridPropertyDetails.daysSinceDug>-1&&gridPropertyDetails.daysSinceWatered==-1)
                {
                    return true;
                }

                else
                {
                    return false;
                }

            case ItemType.Chopping_Tool:
            case ItemType.Collecting_Tool:
            case ItemType.Breaking_Tool:

                //check if item can be harvested with item selected, check item is fully grown

                //check if seed planted 
                if (gridPropertyDetails.seedItemCode != -1)
                {
                    CropDetails cropDetails = so_cropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

                    if (cropDetails != null)
                    {
                        //check if crop fully grown--- mahsulün tamamen yetiþip yetiþmediðini kontrol et
                        if (gridPropertyDetails.growthDays >= cropDetails.growthDays[cropDetails.growthDays.Length-1])
                        {
                            //check if crop can be harvested with tool selected ---- mahsulün seçilen araçla hasat edilip edilemeyeceðini kontrol et 
                            if (cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            
                return false;

            default:
                return false;
        }
    }

    public void DisableCursor()
    {
        cursorImage.color = Color.clear;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public Vector3Int GetGridPositionForCursor()
    {
        //z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front of = 10
        //z, nesnelerin kameranýn önünde ne kadar uzakta olduðunu gösterir - kamera -10'dadýr, dolayýsýyla nesneler (-)-10 önündedir = 10

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

        return grid.WorldToCell(worldPosition);
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }
    
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }
}
