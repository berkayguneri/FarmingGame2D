using UnityEngine;

[System.Serializable]
public class CropDetails 
{
    [ItemCodeDescription]
    public int seedItemCode; // this is the item code for the corresponding seed
    public int[] growthDays; // days growth for each stage
    public GameObject[] growthPrefab;
    public Sprite[] growthSprite;
    public Season[] seasons;
    public Sprite harvestedSprite;
    [ItemCodeDescription]
    public int harvestedTransformItemCode;// if the item transforms into another item when harvested this item code will be populated
    public bool hideCropBeforeHarvestedAnimation;//if the crop should be disabled before the harvested animation
    public bool disableCropCollidersBeforeHarvestedAnimation;//if colliders on crop should be disabled to avoid the harvested animation effecting any other game object
    public bool isHarvestedAnimation;
    public bool isHarvestActionEffect = false;
    public bool spawnCropProductAtPlayerPosition;
    public HarvestActionEffect harvestActionEffect;

    [ItemCodeDescription]
    public int[] harvestToolItemCode;
    public int[] requiredHarvestActions;
    [ItemCodeDescription]
    public int[] cropProducedItemCode;
    public int[] cropProducedMinQuantity;
    public int[] cropProducedMaxQuantity;
    public int daysToRegrow;


    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode) == -1)
        {
            return false;
        }

        else
        {
            return true;
        }
    }

    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestActions[i];
            }

        }
        return -1;
    }
}
