using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem instance;
    public int farmsize;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;

    [SerializeField] private PlacementObjData database;
    public GameObject playerRef;

    public int selectedObjectIndex = -1;
    public int currentPlantID = -1;
    public GridData gridData = new GridData();


    private List<GameObject> placedGameObjects = new();

    [SerializeField] public PreviewSystem preview;
    public bool isPlacing;
    public bool isPlanting;
    public bool isWatering;
    public bool isHarvesting;
    public int rotateindex;
    public Vector2Int playerGridPos;

    public Vector3Int gridPos;
    public RectTransform[] Rect;
    public GameObject progressbar;
    Slider progressSlider;
    private float holdTimer = 0f;
    public bool isHolding = false;
    private Vector3Int holdStartGridPos;
    public GameObject hoe, wateringCan , weapon;
    void Start()
    {
        instance = this;
        StopPlacement();
        progressSlider = progressbar.GetComponent<Slider>();
        weapon.SetActive(false);
        gridData.AddObjectAt(new Vector3Int(0,0,3),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(-1,0,3),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(1,0,3),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(0,0,4),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(-1,0,4),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(1,0,4),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(0,0,5),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(-1,0,5),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        gridData.AddObjectAt(new Vector3Int(1,0,5),database.placementDatas[2].Size,database.placementDatas[2].ID,0,rotateindex);
        playerRef = GameObject.FindGameObjectWithTag("Player");
    }
    public bool IsMouseOverInventory()
    {
        Vector2 mousePos = Input.mousePosition;
        foreach (var rect in Rect)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, null))
                return true;
        }
        return false;
    }
    public void StartPlacement(int ID)
    {
        StopPlacement();
        isPlacing = true;

        selectedObjectIndex = database.placementDatas.FindIndex(x => x.ID == ID);
        if (selectedObjectIndex < 0)
        {
            Debug.Log("No placement data found");
            return;
        }


        Vector3 mousePos = inputManager.GetSelectedMapPos();
        Vector3Int rawGridPos = grid.WorldToCell(mousePos + new Vector3(0.5f, 0, 0.5f));
        Vector3Int clampedPos = ClampGridPos(rawGridPos, 2); // or 1 if planting
        Vector3 startPos = grid.CellToWorld(clampedPos);

        preview.StartShowingPlacementPreview(
            database.placementDatas[selectedObjectIndex].Prefab,
            database.placementDatas[selectedObjectIndex].Size,
            startPos
        );
      
       

    }

    private void PlaceStructure()
    {
     

        Vector3 mousePosition = inputManager.GetSelectedMapPos();
      

        bool placementValid = CheckPlacementValid(gridPos, selectedObjectIndex);
        if (!placementValid)
            return;
       
        GameObject gameObject = Instantiate(database.placementDatas[selectedObjectIndex].Prefab);
        gameObject.transform.position = grid.CellToWorld(gridPos);
        gameObject.transform.rotation = Quaternion.Euler(0, 90 * rotateindex, 0);
       
      
        int index = placedGameObjects.FindIndex(x => x == null);
        if (index == -1)
        {
            // No empty slot, append at the end
            index = placedGameObjects.Count;
            placedGameObjects.Add(gameObject);
        }
        else
        {
            // Fill the empty slot
            placedGameObjects[index] = gameObject;
        }
        if (selectedObjectIndex == 0)
        {
            DirtTile tile = gameObject.GetComponent<DirtTile>();
            gridData.AddObjectAt(gridPos,database.placementDatas[selectedObjectIndex].Size,database.placementDatas[selectedObjectIndex].ID,index,rotateindex,tile);
        }
        else
        {
            gridData.AddObjectAt(gridPos,database.placementDatas[selectedObjectIndex].Size,database.placementDatas[selectedObjectIndex].ID,index,rotateindex);
            bool slotEmpty = InventoryManager.Instance.TryConsumeSelectedItem(1);
            if (slotEmpty)
            {
                StopPlacement(); // stop immediately if no more items
            }
        }
      
       
    }

    public bool CheckPlacementValid(Vector3Int gridPos, int selectedObjectIndex)
    {
        var data = database.placementDatas[selectedObjectIndex];
        return gridData.CanPlaceObject(gridPos, data.Size, rotateindex,
            new Vector3Int(playerGridPos.x, 0, playerGridPos.y));
    }
    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        currentPlantID = -1;
        isPlacing = false;
        isPlanting = false;
        isWatering = false;
        isHarvesting = false;
        preview.StopShowingPlacementPreview();
        inputManager.ClearEvents();
    }
    public void RemovePlacedTile(GameObject tile)
    {
        // Find the placed object's index in the list
        int index = placedGameObjects.IndexOf(tile);
        if (index < 0) return;

        // Remove all occupied positions
        foreach (var kvp in gridData.placeObjects.ToList()) 
        {
            if (kvp.Value.PlaceObjectIndex == index)
            {
                gridData.placeObjects.Remove(kvp.Key);
            }
        }

        // Remove from placedGameObjects
        placedGameObjects[index] = null;
        Destroy(tile);
    }
    // Update is called once per frame
    void Update()
    {
     
        playerGridPos = new Vector2Int(Mathf.RoundToInt(playerRef.transform.position.x),
            Mathf.RoundToInt(playerRef.transform.position.z));

        if (isHolding&&!isPlanting&&!isHarvesting)
        {
            if (database.placementDatas[selectedObjectIndex].ID == 0)
            {
                hoe.SetActive(true);
                hoe.GetComponent<Renderer>().enabled = true;
            }
               
        }
        else
        {
            hoe.GetComponent<Renderer>().enabled = false;
            hoe.SetActive(false);
        }

        if (PlantingSystem.instance.holdingWater)
        {
            wateringCan.SetActive(true);
            
        }
        else
        {
            wateringCan.SetActive(false);
            wateringCan.GetComponent<Renderer>().enabled = false;
        }
        if (isPlacing)
        {
            UpdatePlacement();
            Vector3Int currentGridPos = gridPos;
            if (!PlayerMovement.Instance.inventoryOn)
            {
                if (selectedObjectIndex >= 0 || isPlanting || isHarvesting)
                { 
                    
                    if (!IsMouseOverInventory()&&Input.GetMouseButtonDown(0) && !isHolding)
                    {
                        // Start holding
                      
                        if (isPlanting && !PlantingSystem.instance.IsPlantingValid(gridPos,currentPlantID)) return;
                        if (isHarvesting && !PlantingSystem.instance.IsHarvestValid(gridPos)) return;
                        if (selectedObjectIndex >= 0 && !CheckPlacementValid(gridPos, selectedObjectIndex)) return;

                        isHolding = true;
                        holdTimer = 0f;
                        progressbar.SetActive(true);
                        holdStartGridPos = currentGridPos; // remember start cell
                    }

                    if (isHolding)
                    {
                        // Cancel if the mouse moved to a different grid
                        if (currentGridPos != holdStartGridPos)
                        {
                            Debug.Log("Cancelled: moved to another grid cell");
                            ResetHold();
                            return;
                        }

                        // Continue holding
                        if (Input.GetMouseButton(0))
                        {
                            holdTimer += Time.deltaTime;

                            float requiredTime = 0f;
                            if (isPlanting)
                            {
                                var data = PlantingSystem.instance.plantDataSO.plantDatas
                                    .Find(p => p.ID == currentPlantID);
                                requiredTime = data != null ? data.PlantingTime : 0.5f;
                                PlayerMovement.Instance.animator.SetInteger("State", 2);
                            }
                            else if (isHarvesting)
                            {
                                var data = PlantingSystem.instance.plantDataSO.plantDatas
                                    .Find(p => p.ID == currentPlantID);
                                requiredTime = data != null ? data.HarvestTime : 0.5f;
                                PlayerMovement.Instance.animator.SetInteger("State", 2);
                            }
                            else if (selectedObjectIndex >= 0)
                            {
                              
                                if (database.placementDatas[selectedObjectIndex].ID == 0)
                                {
                                    PlayerMovement.Instance.animator.SetInteger("State", 1);
                                }
                                else
                                {
                                    PlayerMovement.Instance.animator.SetInteger("State", 2);
                                }
                                requiredTime = database.placementDatas[selectedObjectIndex].buildTime;
                            }
                            progressSlider.value = holdTimer / requiredTime;
                            if (holdTimer >= requiredTime)
                            {
                                if (isPlanting)
                                {
                                    PlantingSystem.instance.PlacePlant(currentPlantID);
                                }
                                else if (isHarvesting)
                                {
                                    PlantingSystem.instance.TryHarvest();
                                }
                                else
                                {
                                    PlaceStructure();
                                }

                                ResetHold();
                            }
                          
                        }
                        else if (Input.GetMouseButtonUp(0))
                        {
                            // Released too early
                            Debug.Log("Failed: released too early");
                            ResetHold();
                            
                        }
                    }
                }
            }
          

            if (!isHarvesting && !isWatering && selectedObjectIndex != 0)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    rotateindex++;
                    if (rotateindex >= 4) rotateindex = 0;
                }
            }
        }
       

       
    }
    private void ResetHold()
    {
        PlayerMovement.Instance.animator.SetInteger("State", 0);
        isHolding = false;
        holdTimer = 0f;
        progressSlider.value = 0;
        progressbar.SetActive(false);
    }
    public void UpdatePlacement()
    {
        Vector3 mousePosition = inputManager.GetSelectedMapPos();
        Vector3Int rawGridPos = grid.WorldToCell(mousePosition + new Vector3(0.5f, 0, 0.5f));
        

        if (isWatering)
        {
            gridPos = ClampGridPos(rawGridPos, 1); // or your watering range
            bool canWater = gridData.placeObjects.TryGetValue(gridPos, out var placementData);

            preview.UpdatePosition(grid.CellToWorld(gridPos), canWater, Vector2Int.one, 0);
         
        }
        else if(isPlanting)
        
        {   gridPos = ClampGridPos(rawGridPos, 1);
            bool placementValid = true;
            if (!gridData.placeObjects.TryGetValue(gridPos, out var placementData))
                placementValid = false;
            else if (placementData.ID != 0 || placementData.HasPlant)
                placementValid = false;
            preview.UpdatePosition(grid.CellToWorld(gridPos), placementValid,new Vector2Int(1,1), rotateindex);
        }
        else if(isHarvesting)
        
        {  gridPos = ClampGridPos(rawGridPos, 1);
            bool placementValid = false;

            if (gridData.placeObjects.TryGetValue(gridPos, out var placementData))
            {
                if (placementData.HasPlant && placementData.PlantRef != null)
                {
                    PlantGrowing crop = placementData.PlantRef;
                    if (crop.Harvestable)
                        placementValid = true;
                }
            }

            preview.UpdatePosition(
                grid.CellToWorld(gridPos), 
                placementValid, 
                new Vector2Int(1,1), 
                rotateindex
            );
        }
        else
        {
            if (selectedObjectIndex == 0)
            {
                gridPos = ClampGridPos(rawGridPos, 1);
            }
            else
            {
                gridPos = ClampGridPos(rawGridPos, 2);
            }
            if(selectedObjectIndex <0)
                return;
            bool placementValid = CheckPlacementValid(gridPos, selectedObjectIndex);
            preview.UpdatePosition(grid.CellToWorld(gridPos), placementValid,
                database.placementDatas[selectedObjectIndex].Size, rotateindex);
           
        }
      
    }
    public Vector3Int ClampGridPos(Vector3Int rawGridPos, int range)
    {
        int minX = playerGridPos.x - range;
        int maxX = playerGridPos.x + range;
        int minZ = playerGridPos.y - range;
        int maxZ = playerGridPos.y + range;

        int clampedX = Mathf.Clamp(rawGridPos.x, minX, maxX);
        int clampedZ = Mathf.Clamp(rawGridPos.z, minZ, maxZ);
        return new Vector3Int(clampedX, 0, clampedZ);
    }
    
}