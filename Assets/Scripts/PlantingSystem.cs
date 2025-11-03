using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantingSystem : MonoBehaviour
{
    public static PlantingSystem instance;
       [SerializeField] private Grid grid;
    [SerializeField] private PlacementSystem placementSystem; // reference to building system
    [SerializeField] private InputManager inputManager;

    [SerializeField] public PlantDataSO plantDataSO;

    private Vector3Int selectedGridPos;
    private Vector2Int playerGridPos;
    public float CurrentWater = 1000;
    public float MaxWater = 1000;
    [SerializeField]private float wateringRate = 20;
    [SerializeField]private float collectWaterRate = 30;
    public bool holdingWater;
    public TextMeshProUGUI waterText;
    public GameObject waterSliderObj;
    Slider waterSlider;
    private DirtTile lastHoveredTile = null;
    private void Start()
    {
        instance = this;
        waterSlider = waterSliderObj.GetComponent<Slider>();
    }

    private void Update()
    {
        UpdatePlayerGridPos();

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (holdingWater)
            {  holdingWater = false;
                PlayerMovement.Instance.animator.SetInteger("State", 0);
            }
        }
        if (placementSystem.isWatering == false)
        {
            holdingWater = false;
           
            waterSliderObj.SetActive(false);
        }
     
       
        if (placementSystem.isWatering)
        {
          waterSliderObj.SetActive(true);
          waterSlider.value = CurrentWater/MaxWater;
            if (placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
            {
                var dirtTile = placementData.DirtTile;
                if (dirtTile != null)
                {
                    if (lastHoveredTile != null &&lastHoveredTile != dirtTile)
                    {
                        lastHoveredTile.canvas.SetActive(false);
                       
                    }
                    lastHoveredTile = dirtTile;
                    dirtTile.canvas.SetActive(true);
                    waterSliderObj.SetActive(true);
                    float waterRatio = dirtTile.water / dirtTile.maxWater;
                 //   waterSlider.value = Mathf.Clamp01(waterRatio);
                }
                else
                {
                  
                  //  waterSliderObj.SetActive(false);
                    if (lastHoveredTile != null)
                    {
                        lastHoveredTile.canvas.SetActive(false);
                        lastHoveredTile = null;
                    }
                }
            }
            else
            {
            //    waterSliderObj.SetActive(false);
                if (lastHoveredTile != null)
                {
                    lastHoveredTile.canvas.SetActive(false);
                    lastHoveredTile = null;
                }
            }

            // Actually water when holding mouse
            if (holdingWater)
            {
                WaterTile();
            }
        }
        else
        {
            // Hide all tiles' fill images when not watering
            foreach (var kvp in placementSystem.gridData.placeObjects)
            {
                if (kvp.Value.DirtTile != null)
                    kvp.Value.DirtTile.canvas.SetActive(false);
            }
            waterSliderObj.SetActive(false);
        }
           

        waterText.text = "Water:"+((int)CurrentWater).ToString();
    }

    private void BeginWatering()
    {
        holdingWater = true;
        placementSystem.wateringCan.GetComponent<Renderer>().enabled = true;
        PlayerMovement.Instance.animator.SetInteger("State", 3);
        waterSliderObj.SetActive(true);
    }

    private void UpdatePlayerGridPos()
    {
        GameObject player = placementSystem.playerRef;
        if (player == null) return;

        playerGridPos = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x),
            Mathf.RoundToInt(player.transform.position.z)
        );
    }
    public bool IsWateringValid(Vector3Int gridPos)
    {
        // Make sure the grid position exists
        if (!placementSystem.gridData.placeObjects.TryGetValue(gridPos, out var placementData))
            return false;

        // Cannot water wells or other non-dirt objects (optional)
        if (placementData.ID == 1)
            return true;

        // Must be dirt
        if (placementData.ID != 0) 
            return false;

        // Must have a DirtTile
        if (placementData.DirtTile == null)
            return false;

        // Optionally: skip if already fully watered
        if (placementData.DirtTile.water >= placementData.DirtTile.maxWater)
            return false;

        return true;
    }
    public void StartPlanting(int plantID)
    {
        placementSystem.StopPlacement();
        placementSystem.isPlacing = true;
        placementSystem.isPlanting = true;
        var plantData = plantDataSO.plantDatas.Find(p => p.ID == plantID);
        placementSystem.currentPlantID = plantID;
        if (plantData == null)
        {
            Debug.LogError($"No plant with ID {plantID}");
            return;
        }
        Vector3 mousePos = inputManager.GetSelectedMapPos();
        Vector3Int rawGridPos = grid.WorldToCell(mousePos + new Vector3(0.5f, 0, 0.5f));
        Vector3Int clampedPos = PlacementSystem.instance.ClampGridPos(rawGridPos, 1); // or 1 if planting
        Vector3 startPos = grid.CellToWorld(clampedPos);
        placementSystem.preview.StartShowingPlacementPreview(plantData.Prefab, new Vector2Int(1,1), startPos,true);
        placementSystem.selectedObjectIndex = -2; 
        
        if (!placementSystem.IsMouseOverInventory()&&!PlayerMovement.Instance.inventoryOn)
        {
        //    inputManager.OnClick += () => PlacePlant(plantID);
        }

      
        inputManager.OnExit += placementSystem.StopPlacement;
    }
    public void StartWatering()
    {
        placementSystem.StopPlacement();
        placementSystem.isPlacing = true;
        placementSystem.isWatering = true;
        placementSystem.selectedObjectIndex = -3; // mark special mode = watering

        if (!placementSystem.IsMouseOverInventory()&&!PlayerMovement.Instance.inventoryOn&&IsWateringValid(placementSystem.gridPos))
        {
            inputManager.OnClick += BeginWatering;
        }
        
      
    }
    private void StopAllPlacementActions()
    {
        placementSystem.StopPlacement();
        holdingWater = false;
    }

    public void StartHarvesting()
    {
        placementSystem.StopPlacement();
        placementSystem.isPlacing = true;
        placementSystem.isHarvesting = true;

        if (!placementSystem.IsMouseOverInventory()&&!PlayerMovement.Instance.inventoryOn)
        {
          //  inputManager.OnClick += TryHarvest;
        }
       
        inputManager.OnExit += placementSystem.StopPlacement;
    }
    public void TryHarvest()
    {
        if (!placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
            return;
       
        
        if (placementData.DirtTile != null && placementData.DirtTile.plant != null)
        {
            PlantGrowing crop = placementData.DirtTile.plant;

            if (crop.Harvestable)
            {
                bool repeatable = crop.data.Repeatable;
                crop.Harvest();

                if (!repeatable) 
                {
                    // Only clear reference if crop is destroyed
                    placementData.DirtTile.plant = null;
                }
            }
        }
    }
    private void WaterTile()
    {
        Vector3Int targetGrid = placementSystem.gridPos;
       
        if (!placementSystem.gridData.placeObjects.TryGetValue(targetGrid, out var placementData))
        {
            PlayerMovement.Instance.animator.SetInteger("State", 0);
            holdingWater = false;
            return;
        }
           
        

        var dirtTile = placementData.DirtTile;
        if (placementData.ID == 1) // well
        {
            CurrentWater += collectWaterRate * Time.deltaTime;
            if (CurrentWater > MaxWater)
                CurrentWater = MaxWater;
            return; // skip watering the soil
        }
        if (dirtTile == null)
            return;

        if (CurrentWater > 0)
        {
            if (dirtTile.water > dirtTile.maxWater)
            {
                dirtTile.water =  dirtTile.maxWater;
            }
            else if(dirtTile.water <= dirtTile.maxWater -0.1f)
            {
                dirtTile.water += wateringRate * Time.deltaTime;
                CurrentWater -= wateringRate * Time.deltaTime;
            }
           
        }
        else
        {
            CurrentWater = 0;
        }
        


    }
    public bool IsPlantingValid(Vector3Int gridPos, int plantID)
    {
        if (!placementSystem.gridData.placeObjects.TryGetValue(gridPos, out var placementData))
            return false;

        // Must be dirt
        if (placementData.ID != 0) 
            return false;

        // Must be empty (no plant already)
        if (placementData.HasPlant)
            return false;

        var plantData = plantDataSO.plantDatas.Find(p => p.ID == plantID);
        if (plantData == null) 
            return false;

        // Prevent planting where player stands if blocked
        if (plantData.BlockPath && 
            gridPos.x == playerGridPos.x && gridPos.z == playerGridPos.y)
            return false;

        return true;
    }
    public bool IsHarvestValid(Vector3Int gridPos)
    {
        if (!placementSystem.gridData.placeObjects.TryGetValue(gridPos, out var placementData))
            return false;

        if (placementData.DirtTile == null) 
            return false;

        var crop = placementData.DirtTile.plant;
        if (crop == null) 
            return false;

        return crop.Harvestable;
    }
    public void PlacePlant(int plantID)
    {
        if (!placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
            return;

        // Only allow planting on dirt
        if (placementData.ID != 0 || placementData.HasPlant)
            return;
        bool slotEmpty = InventoryManager.Instance.TryConsumeSelectedItem(1);
        if (slotEmpty)
        {
            PlacementSystem.instance.StopPlacement(); // stop immediately if no more items
        }
        var dirtTile = placementData.DirtTile;
        var plantData = plantDataSO.plantDatas.Find(p => p.ID == plantID);
        if (plantData.BlockPath)
        {
            if (placementSystem.gridPos.x == playerGridPos.x && placementSystem.gridPos.z == playerGridPos.y)
                return;
        }
 
        GameObject plantGO = Instantiate(
            plantData.Prefab,
            grid.CellToWorld(placementSystem.gridPos),
            Quaternion.identity,
            dirtTile.transform
        );
      

        var crop = plantGO.GetComponent<PlantGrowing>();
        crop.Initialize(plantData, dirtTile);
        placementData.DirtTile.plant = crop;
        placementData.SetPlant(plantGO, plantData);
    }
   

  
}
