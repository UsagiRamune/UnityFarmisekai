using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlantGrowing : MonoBehaviour
{
    public int PlantID { get; private set; }
    public PlantDataSO.PlantData data { get; private set; }
    public DirtTile DirtTile { get; private set; }

    public float growthProgress = 0f;
    public int currentStage = 0;
    public GameObject[] stageObjects;
    public bool Harvestable;
    public GameObject cropPrefab;
    public Item crop;
    public Image harvestImage;

    public void Initialize(PlantDataSO.PlantData plantData, DirtTile dirtTile)
    {
        data = plantData;
        PlantID = plantData.ID;
        DirtTile = dirtTile; // store reference to water, soil info, etc.
        crop = plantData.yield;
        growthProgress = 0f;
        stageObjects[currentStage].SetActive(true);
        foreach (var stage in stageObjects)
            stage.SetActive(false);

        // Start at seed stage
        currentStage = 0;
        stageObjects[currentStage].SetActive(true);
    }
    private void Update()
    {
        if (DirtTile == null) return;
        if (Harvestable)
        {
            harvestImage.enabled = true;
            FaceCamera();
        }
        else
        {
            harvestImage.enabled = false;
        }
        DirtTile.elevationT = 100;
        if (currentStage < data.ProgressTarget.Length)
        {
            if (DirtTile.water > 0)
                growthProgress += 0.5f * Time.deltaTime;
            if (growthProgress >= data.ProgressTarget[currentStage])
            {
                growthProgress = 0f;
                stageObjects[currentStage].SetActive(false);
                currentStage++;
                stageObjects[currentStage].SetActive(true);
                if (currentStage == data.ProgressTarget.Length)
                {
                    Harvestable = true;
                    return;
                }
            }

            if (DirtTile.water > 0)
            {
                if (data.DifferentWater)
                {
                    DirtTile.water -= data.DrainWaters[currentStage] * Time.deltaTime;
                }
                else
                {
                    DirtTile.water -= data.DrainWater * Time.deltaTime;
                }
            }
            else
            {
                DirtTile.water = 0;
            }
        }
        else
        {
            if (DirtTile.water > 0)
            {
                if (data.DifferentWater)
                {
                    DirtTile.water -= (0.5f + (Calculator.FindAverage(data.DrainWaters) * 0.5f)) * Time.deltaTime;
                }
                else
                    DirtTile.water -= (0.5f + (data.DrainWater * 0.5f)) * Time.deltaTime;
            }
            else
                DirtTile.water = 0;
        }
    }
    public void Harvest()
    {  
        float radius = 1f; // adjust how far items can scatter
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 dropPosition = transform.position + new Vector3(randomCircle.x, 1f, randomCircle.y);
        if (data.FixedAmount)
        {
          //  PlayerMovement.money += data.Amount * data.BaseSellCost;
            GameObject cropGO = Instantiate(cropPrefab, dropPosition, Quaternion.identity);
            WorldItem worldItem = cropGO.GetComponentInChildren<WorldItem>();
            worldItem.Initialise(crop, data.Amount);
        }
        else
        {
            
          //  PlayerMovement.money += Random.Range(data.MinAmount, data.MaxAmount + 1) * data.BaseSellCost;
          GameObject cropGO = Instantiate(cropPrefab, dropPosition, Quaternion.identity);
          WorldItem worldItem = cropGO.GetComponentInChildren<WorldItem>();
          worldItem.Initialise(crop, Random.Range(data.MinAmount, data.MaxAmount + 1));
        }

        if (data.Repeatable)
        {
            stageObjects[currentStage].SetActive(false);
            currentStage = data.BackToStage;
            stageObjects[currentStage].SetActive(true);

            growthProgress = 0f;
            Harvestable = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void FaceCamera()
    {
        if (Camera.main == null) return;

        // Calculate direction from item to camera
        harvestImage.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
}