using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>,ISaveable
{
    private Transform cropParentTransform;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;
    private bool isFirstTimeSceneLoaded = true;
    private Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;
    [SerializeField] private SO_CropDetailsList so_cropDetailList = null;
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] waterGround = null;

    private string _iSaveableUniqeID;
    public string ISaveableUniqueID { get { return _iSaveableUniqeID; } set { _iSaveableUniqeID = value; } }

    private GameobjectSave _gameObjectSave;

    public GameobjectSave GameobjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameobjectSave = new GameobjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
       
    }

    private void OnDisable()
    {
        ISaveableDeRegister();

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }


    private void Start()
    {
        InitialiseGridProperties();
    }

    private void ClearDisplayGroundDecorations()
    {
        //remove ground decorations
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayAllPlantedCrops()
    {
        //destroy all crops in scene 
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach (Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }
    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();

        ClearDisplayAllPlantedCrops();
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //dug
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugAround(gridPropertyDetails);
        }
    }

    public void DisplayWaterGround(GridPropertyDetails gridPropertyDetails)
    {
        //watered
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    private void ConnectDugAround(GridPropertyDetails gridPropertyDetails)
    {
        //select tile based on surrounding dug tiles

        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        //set 4 tiles if dug surrounding current tile - up,down,left,right now that this central tile has been dug

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);       
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY  -1, 0), dugTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY );
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX -1, gridPropertyDetails.gridY );
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX -1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1 , gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        //select tile based on surrounding watered tiles
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX-1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX -1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX -1 , gridPropertyDetails.gridY,0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY );
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX +1 , gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY,0), wateredTile4);
        }
    }

    private Tile SetDugTile(int xGrid, int yGrid)
    {
        //get whether surrounding tiles (up,down,right,left) are dug or not

        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tile are dug or not

        if (!upDug && !downDug && !leftDug && !rightDug)
        {
            return dugGround[0];
        }

        else if (!upDug && downDug && !leftDug && rightDug)
        {
            return dugGround[1];
        }

        else if (!upDug && downDug && leftDug && rightDug)
        {
            return dugGround[2];
        }

        else if (!upDug && downDug && leftDug && !rightDug)
        {
            return dugGround[3];
        }

        else if (!upDug && downDug && !leftDug && !rightDug)
        {
            return dugGround[4];
        }

        else if (upDug && downDug && !leftDug && rightDug)
        {
            return dugGround[5];
        }

        else if (upDug && downDug && leftDug && rightDug)
        {
            return dugGround[6];
        }

        else if (upDug && downDug && leftDug && !rightDug)
        {
            return dugGround[7];
        }

        else if (upDug && downDug && !leftDug && !rightDug)
        {
            return dugGround[8];
        }

        else if (upDug && !downDug && !leftDug && rightDug)
        {
            return dugGround[9];
        }

        else if (upDug && !downDug && leftDug && rightDug)
        {
            return dugGround[10];
        }

        else if (upDug && !downDug && leftDug && !rightDug)
        {
            return dugGround[11];
        }

        else if (upDug && !downDug && !leftDug && !rightDug)
        {
            return dugGround[12];
        }

        else if (!upDug && !downDug && !leftDug && rightDug)
        {
            return dugGround[13];
        }

        else if (!upDug && !downDug && leftDug && rightDug)
        {
            return dugGround[14];
        }

        else if (!upDug && !downDug && leftDug && !rightDug)
        {
            return dugGround[15];
        }
        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are dug or not

    }

    private bool IsGridSquareDug(int xGrid,int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        //get whether surroinding tiles (up,dopwn,left,right) are watered or not

        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid -1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1 , yGrid);

        #region Set appropriate tile based on whether surroinding tiles are watered or not

        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return waterGround[0];
        }

        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return waterGround[1];
        }

        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return waterGround[2];
        }

        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return waterGround[3];
        }

        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return waterGround[4];
        }

        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return waterGround[5];
        }

        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return waterGround[6];
        }

        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return waterGround[7];
        }

        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return waterGround[8];
        }

        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return waterGround[9];
        }

        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return waterGround[10];
        }

        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return waterGround[11];
        }

        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return waterGround[12];
        }

        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return waterGround[13];
        }

        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return waterGround[14];
        }

        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return waterGround[15];
        }

        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are watered or not
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;

        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    private void DisplayGridPropertyDetails()
    {
        foreach (KeyValuePair<string,GridPropertyDetails> item in gridPropertyDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;

            DisplayDugGround(gridPropertyDetails);

            DisplayWaterGround(gridPropertyDetails);

            DisplayPlantedCrop(gridPropertyDetails);
        }

    }


    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.seedItemCode > -1)
        {
            CropDetails cropDetails = so_cropDetailList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {
                GameObject cropPrefab;

                int growthStages = cropDetails.growthDays.Length;

                int currentGrowthStage = 0;
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= cropDetails.growthDays[i])
                    {
                        currentGrowthStage = i;
                        break;
                    }

                }

                cropPrefab = cropDetails.growthPrefab[currentGrowthStage];

                Sprite growthSprite = cropDetails.growthSprite[currentGrowthStage];

                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));

                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);

                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }

            
        }
    }




    private void InitialiseGridProperties()
    {
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();


            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                   
                }

                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);
            }

            //Create scene save for this gameobject
            SceneSave sceneSave = new SceneSave();

            //Add grid property dictioanary to scene save data
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            //If starting scene set the gridPropertyDictionary member variable to the current iteration
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            sceneSave.boolDictionary = new Dictionary<string, bool>();
            sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", true);

            //add scene save to game object scene data
            GameobjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }

    private void AfterSceneLoaded()
    {
        if (GameObject.FindGameObjectWithTag(Tags.CropParentTransform) != null)
        {
            cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropParentTransform).transform;
        }
        else
        {
            cropParentTransform = null;
        }


        grid = GameObject.FindObjectOfType<Grid>();

        //get tilemaps
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
       
    }

    //returns the gridPropertyDetails at the gridlocation  for the supplied dictionary, or null if no properties exist  at that location

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        //construct key from coordinate
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        //Check if grid property details exist for coordinate and retrieve

        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            //if not found
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] colliderArray = Physics2D.OverlapPointAll(worldPosition);

        //loop through colliders to get crop game object
        Crop crop = null;

        for (int i = 0; i < colliderArray.Length; i++)
        {
            crop = colliderArray[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            crop = colliderArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
        }

        return crop;
    }

    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_cropDetailList.GetCropDetails(seedItemCode);
    }

    //get the grid property details for the tile at (gridX,gridY). If no grid property details exist null is returned and can assume that all grid property,
    //details values are null or false
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }

    public bool GetGridDimension(SceneName sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
    {
        gridDimensions = Vector2Int.zero;
        gridOrigin = Vector2Int.zero;

        foreach (SO_GridProperties sO_GridProperties in so_gridPropertiesArray)
        {
            if (sO_GridProperties.sceneName == sceneName)
            {
                gridDimensions.x = sO_GridProperties.gridWidth;
                gridDimensions.y = sO_GridProperties.gridHeight;

                gridOrigin.x = sO_GridProperties.originX;
                gridOrigin.y = sO_GridProperties.originY;

                return true;
            }
        }

        return false;
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

            //restore data for current scene
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //get sceneSave for scene- it exist since we created it in initialise
        if (GameobjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }


            //get dictionary of bools- it exist since we created it in initialise
            if (sceneSave.boolDictionary != null && sceneSave.boolDictionary.TryGetValue("isFirstTimeSceneLoaded", out bool storedIsFirstTimeSceneLoaded))
            {
                isFirstTimeSceneLoaded = storedIsFirstTimeSceneLoaded;
            }

            //instantiate any crop prefabs initally present in scene
            if (isFirstTimeSceneLoaded)
                EventHandler.CallInstantiateCropPrefabsEvent();


            //if grid properties exist
            if (gridPropertyDictionary.Count > 0)
            {
                //grid property details found for the current scene destroy existing ground decoration
                ClearDisplayGridPropertyDetails();

                //Instantiate grid property details for current scene
                DisplayGridPropertyDetails();
            }

            if (isFirstTimeSceneLoaded == true)
            {
                isFirstTimeSceneLoaded = false;
            }
        }
    }

    public GameobjectSave ISaveableSave()
    {
        ISaveableStoreScene(SceneManager.GetActiveScene().name);

        return GameobjectSave;
    }

    
    public void ISaveableStoreScene(string sceneName)
    {
        //remove sceneSave for scene
        GameobjectSave.sceneData.Remove(sceneName);

        //create sceneSave for scene
        SceneSave sceneSave = new SceneSave();

        //create && add dict grid property details dictionary
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;


        sceneSave.boolDictionary = new Dictionary<string, bool>();
        sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", isFirstTimeSceneLoaded);

        //add scene save to game object scene data
        GameobjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }

    public void SetGridPropertyDetails(int gridX,int gridY,GridPropertyDetails gridPropertyDetails,Dictionary<string,GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDictionary[key] = gridPropertyDetails;
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        ClearDisplayGridPropertyDetails();

        foreach (SO_GridProperties sO_GridProperties in so_gridPropertiesArray)
        {
            if (GameobjectSave.sceneData.TryGetValue(sO_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count -1; i>=0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region Update all grid properties to reflect the advence in the day 

                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }

                        //if ground is watered,then clear water 
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);

                        #endregion Update all grid properties to reflect the advence in the day 
                    }
                }
            }
        }

        DisplayGridPropertyDetails();
       
    }
        

      
    

  
}
