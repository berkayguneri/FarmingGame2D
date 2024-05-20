using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class Player : SingletonMonobehaviour<Player>,ISaveable
{
    
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds afterPickAnimationPause;
    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;
    private Cursor cursor;

    // Movement Parameters

    private float xInput;
    private float yInput;
    private bool isCarrying = false;
    private bool isIdle;        
    private bool isLiftingToolDown;
    private bool isLiftingToolLeft;
    private bool isLiftingToolRight;
    private bool isLiftingToolUp;
    private bool isRunning;
    private bool isUsingToolDown;
    private bool isUsingToolLeft;
    private bool isUsingToolRight;
    private bool isUsingToolUp;
    private bool isSwingingToolDown;
    private bool isSwingingToolLeft;
    private bool isSwingingToolRight;
    private bool isSwingingToolUp;
    private bool isWalking;
    private bool isPickingUp;
    private bool isPickingDown;
    private bool isPickingLeft;
    private bool isPickingRight;
    private ToolEffect toolEffect = ToolEffect.none;

    private Camera mainCam;
    private bool playerToolUseDisabled = false;

    private Rigidbody2D rb;

    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds pickAnimationPause;

    private Direction playerDirection;


    private List<CharacterAttribute> characterAttributesCustomisationList;
    private float movementSpeed;

    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;

    //Player attributes that can be swapped
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;


    private bool playerInputIsDisabled = false;

    public bool PlayerInputIsDisabled { get => playerInputIsDisabled; set => playerInputIsDisabled = value;}

    private string _iSaveableUniqueID;

    public string ISaveableUniqueID { get { return _iSaveableUniqueID; }set { _iSaveableUniqueID = value; } }

    private GameobjectSave _gameObjectSave;

    public GameobjectSave GameobjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        //Initialise swappable character attributes
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);

        //Initialise character attributes list
        characterAttributesCustomisationList = new List<CharacterAttribute>();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameobjectSave = new GameobjectSave();



        mainCam = Camera.main;
    }

    private void OnDisable()
    {
        ISaveableDeRegister();

        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    private void Update()
    {
        #region Player Input

        if (!PlayerInputIsDisabled)
        {
            ResetAnimationTriggers();

            PlayerMovementInput();

            PlayerWalkInput();

            PlayerClickInput();

            PlayerTestInput();
           

            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,
         toolEffect,
         isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
         isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
         isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
         isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
         false, false, false, false);


        }       


        #endregion Player Input
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        Vector2 move = new Vector2(xInput * movementSpeed * Time.deltaTime, yInput * movementSpeed * Time.deltaTime);

        rb.MovePosition(rb.position + move);
    }

    protected void ResetAnimationTriggers()
    {
        //Picking
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;

        //Using
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;

        //Lifting
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;

        //Swinging
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;

        toolEffect = ToolEffect.none;

    }

    private void PlayerMovementInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        
        if (yInput != 0 && xInput != 0)
        {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }
        if (xInput != 0 || yInput != 0)
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;

            //Capture player direction for save game
            if (xInput < 0)
            {
                playerDirection = Direction.left;
            }
            else if (xInput > 0)
            {
                playerDirection = Direction.right;
            }
            else if (yInput < 0)
            {
                playerDirection = Direction.down;
            }
            else 
            {
                playerDirection = Direction.up;
            }

        }
        // The player is not moving

        else if (xInput == 0 && yInput == 0)
        {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    private void PlayerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            movementSpeed = Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
        }
    }

    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
                {
                    //get cursor grid position
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    //get player grid position
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
        
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition,Vector3Int playerGridPosition)
    {
        ResetMovement();


        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails != null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails,itemDetails);
                    }
                    break;

                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Watering_Tool:
                case ItemType.Breaking_Tool:
                case ItemType.Chopping_Tool:
                case ItemType.Hoeing_Tool:
                case ItemType.Reaping_Tool:
                case ItemType.Collecting_Tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;

                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;

            }
        }
    }
    
    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else if (cursorGridPosition.x < playerGridPosition.x)
        {
            return Vector3Int.left;
        }
        else if (cursorGridPosition.y > playerGridPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }

    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        if (

          cursorPosition.x > playerPosition.x
          &&
          cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
          &&
          cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)

          )
        {
            return Vector3Int.right;
        }
        else if (
            cursorPosition.x < playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.left;
        }

        else if (cursorPosition.y > playerPosition.y)
        {
            return Vector3Int.up;
        }

        else
        {
            return Vector3Int.down;
        }
    }


    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {

        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }


        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null)
        { 

            //update grid properties with seed items
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            //display planted crop at grid property details
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            //remove item from inventory 
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();
        }
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_Tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Watering_Tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            case ItemType.Chopping_Tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Collecting_Tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Breaking_Tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Reaping_Tool:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }
                break;

           

            default:
                break;
        }
    }
    
    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        //trigger animation
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //set tool animation to hoe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        if (playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if(playerDirection==Vector3Int.down)
        {
            isUsingToolDown = true;
        }

        yield return useToolAnimationPause;

        //set grid property details for dug around

        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        //set grid property to dug
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        //display dug grid tiles
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        //after animation pause 
        yield return afterUseToolAnimationPause;

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection,gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //set tool animation to watering can in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        //if there is water in the watering can
        toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }

        else if (playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }

        else if (playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }

        else if (playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        //set grid property details for watered ground

        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        //set grid property to watered 
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        GridPropertiesManager.Instance.DisplayWaterGround(gridPropertyDetails);

        //after animation pause
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails,ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ChopInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator ChopInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //set tool animation to axe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.axe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        //after animation pause
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails,ItemDetails equippedItemDetails,Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return pickAnimationPause;

        //after animation pause
        yield return afterPickAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }
    
    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails euippedItemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(BreakInPlayerDirectionRoutine(gridPropertyDetails, euippedItemDetails, playerDirection));
    }

    private IEnumerator BreakInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        toolCharacterAttribute.partVariantType = PartVariantType.pickaxe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }
    

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // set tool animation to scythe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        //reap in player direction
        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_Tool:
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;
                    }

                    else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;
                    }

                    else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;
                    }

                    else if (playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown = true;
                    }
                    break;
            }

            //define centre point of square which will be used for collision testing
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)), GetPlayerCentrePosition().y +
                playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            //define size of the square which will be used for collision testing
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            //get item components with 2D collider located in the square at the centre point defined (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            Item[] itemArray = HelperMethods.GetComponentAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;

            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_Scenary)
                    {
                        //effect position
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f,
                            itemArray[i].transform.position.z);

                        //trigger reaping effect
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        Destroy(itemArray[i].gameObject);

                        reapableItemCount++;
                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }
        }
    }

    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, ItemDetails equippedItemDetails, GridPropertyDetails gridPropertyDetails)
    {
        switch (equippedItemDetails.itemType)
        {

            case ItemType.Chopping_Tool:
            case ItemType.Breaking_Tool:

                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isUsingToolUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isUsingToolDown = true;
                }
                break;


            case ItemType.Collecting_Tool:

                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;
                }

                else if (playerDirection == Vector3Int.left)
                {
                    isPickingLeft = true;
                }
                
                else if (playerDirection == Vector3Int.up)
                {
                    isPickingUp = true;
                }
                
                else if (playerDirection == Vector3Int.down)
                {
                    isPickingDown = true;
                }
                break;

            case ItemType.none:
                break;
        }

        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        if (crop != null)
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Chopping_Tool:
                case ItemType.Breaking_Tool:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown, isUsingToolUp);
                    break;

                case ItemType.Collecting_Tool:
                    crop.ProcessToolAction(equippedItemDetails,isPickingRight,isPickingLeft,isPickingDown,isPickingUp);
                    break;
            }
        }
    }


    private void PlayerTestInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }
      
    }

    public void ResetMovement()
    {
        //Reset Movement
        xInput = 0f;
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,
        toolEffect,
        isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
        isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
        isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
        isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
        false, false, false, false);


    }


    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        //Apply base character arms customisation
        armsCharacterAttribute.partVariantType = PartVariantType.none;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode)
    {
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if (itemDetails != null)
        {
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            //Apply 'carry' character arms customisation ---- Karakter silahlarýna 'taþýma' özelleþtirmesi uygula

            armsCharacterAttribute.partVariantType = PartVariantType.carry;
            characterAttributesCustomisationList.Clear();
            characterAttributesCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

            isCarrying = true;
        }
    }

    public Vector3 GetPlayerViewportPosition()
    {
        //vector3 viewport position for player ((0,0) viewport bottom left(1,1,) viewport top right)
        return mainCam.WorldToViewportPoint(transform.position);
    }

    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);
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
        GameobjectSave.sceneData.Remove(Settings.PersistentScene);

        SceneSave sceneSave = new SceneSave();

        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serilazilble>();

        sceneSave.stringDictionary = new Dictionary<string, string>();

        Vector3Serilazilble vector3Serilazilble = new Vector3Serilazilble(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serilazilble);

        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);

        sceneSave.stringDictionary.Add("playerDirection", playerDirection.ToString());

        GameobjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameobjectSave;
    }


    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameobjectSave gameObjectSave))
        {
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serilazilble playerPosition))
                {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                if (sceneSave.stringDictionary != null)
                {
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene))
                    {
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }

                    if (sceneSave.stringDictionary.TryGetValue("playerDirection", out string playerDir))
                    {
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);

                        if (playerDirFound)
                        {
                            playerDirection = direction;
                            SetPlayerDirection(playerDirection);
                        }
                    }
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

    public void SetPlayerDirection(Direction direction)
    {
        switch (playerDirection)
        {
            case Direction.up:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, true, false, false, false);
                break;


            case Direction.down:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, true, false, false);
                break;

            case Direction.left:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, true, false);
                break;

            case Direction.right:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, true);
                break;

            default:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, true, false, false);
                break;

        }
    }
}
