using System.Collections.Generic;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField] private float previewYOffset = .06f;
    [SerializeField] public GameObject cellIndicator;
    [SerializeField] private GameObject container;
    public PlacementSystem placementSystem;
    private GameObject previewObject;
    private List<GameObject> previewTiles = new List<GameObject>();
    [SerializeField] private Material previewMaterialsPrefab;
    private Material previewMaterialInstance;
    public PlantDataSO plantData;
    private Renderer[] cellIndicatorRenderers;
    [SerializeField] private GameObject buildingIndicator;
    [SerializeField] private GameObject plantIndicator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previewMaterialInstance = new Material(previewMaterialsPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderers = cellIndicator.GetComponentsInChildren<Renderer>(true);
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size, Vector3 startPos, bool isPlant = false)
    {
        StopShowingPlacementPreview();

        if (prefab != null)
        {
            previewObject = Instantiate(prefab, startPos, Quaternion.identity); // Spawn directly at startPos
            previewObject.name = isPlant ? "PlantPreview" : "Preview";
            PreparePreview(previewObject);
        }

        // Show the cell indicator
        cellIndicator.SetActive(true);
    }

    private void PrepareCursor(Vector2Int size, int rotateIndex)
    {
        container.transform.localRotation = Quaternion.Euler(0, 90 * rotateIndex, 0);
        container.transform.position = cellIndicator.transform.position;
        foreach (var tile in previewTiles)
            Destroy(tile);
        previewTiles.Clear();

        // Create new previews for each occupied cell
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var tile = Instantiate(cellIndicator, transform);
                tile.SetActive(true);
                tile.transform.SetParent(container.transform);
                tile.transform.localPosition = new Vector3(x, 0, y);
                previewTiles.Add(tile);
            }
        }
    }
    public Vector3 GetPreviewPosition()
    {
        return previewObject != null ? previewObject.transform.position : transform.position;
    }
    private void PreparePreview(GameObject prefab)
    {
        SetLayerRecursively(previewObject, LayerMask.NameToLayer("UI"));
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int index = 0; index < materials.Length; index++)
            {
                materials[index] = previewMaterialInstance;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            renderer.materials = materials;
        }

        Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        MonoBehaviour[] scripts = previewObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            script.enabled = false;
        }
    }

    public void StopShowingPlacementPreview()
    {
        cellIndicator.SetActive(false);
        foreach (var tile in previewTiles)
            Destroy(tile);
        previewTiles.Clear();
        Destroy(previewObject);
    }

    public void UpdatePosition(Vector3 position, bool validity, Vector2Int size, int rotate)
    {
        MovePreview(position);
        MoveCursor(position);
        PrepareCursor(size, rotate);
        ApplyFeedback(validity);
        if (previewObject != null)
            previewObject.transform.rotation = Quaternion.Euler(0, 90 * rotate, 0);
    }

    void ApplyFeedback(bool validity)
    {
        if (cellIndicatorRenderers == null || cellIndicatorRenderers.Length == 0)
            return; // nothing to update yet

        Color c = Color.white;

        if (placementSystem.isPlanting)
        {
            buildingIndicator.SetActive(false);
            plantIndicator.SetActive(true);
            bool valid = true;
            if (!placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
                valid = false;
            else if (placementData.ID != 0 || placementData.HasPlant)
                valid = false;
            var data = plantData.plantDatas.Find(p => p.ID == placementSystem.currentPlantID);
            if (data != null && data.BlockPath)
            {
                // Prevent placing on player
                if (placementSystem.gridPos.x == placementSystem.playerGridPos.x &&
                    placementSystem.gridPos.z == placementSystem.playerGridPos.y)
                {
                    valid = false;
                }
            }
            c = valid ? Color.green : Color.red;
        }
        else if (placementSystem.isWatering)
        {
            buildingIndicator.SetActive(false);
            plantIndicator.SetActive(true);

            bool valid = false;
            if (placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
            {
                if (placementData.ID == 0 || placementData.ID == 1) // dirt or well
                    valid = true;
            }
            c = valid ? Color.green : Color.red;
        }
        else if (placementSystem.isHarvesting)
        {
            buildingIndicator.SetActive(false);
            plantIndicator.SetActive(true);

            bool valid = false;
            if (placementSystem.gridData.placeObjects.TryGetValue(placementSystem.gridPos, out var placementData))
            {
                if (placementData.DirtTile != null && placementData.DirtTile.plant != null)
                {
                    PlantGrowing crop = placementData.DirtTile.plant;
                    if (crop.Harvestable)
                        valid = true;
                }
            }

            c = valid ? Color.green : Color.red;
        }
        else
        {
            if (placementSystem.selectedObjectIndex == 0)
            {
                buildingIndicator.SetActive(false);
                plantIndicator.SetActive(true);
            }
            else
            {
                buildingIndicator.SetActive(true);
                plantIndicator.SetActive(false);
            }
           
            bool buildable = placementSystem.CheckPlacementValid(placementSystem.gridPos, placementSystem.selectedObjectIndex);
            c = buildable ? Color.green : Color.red;
        }

        c.a = 0.5f;

        foreach (Renderer r in cellIndicatorRenderers)
        {
            foreach (Material mat in r.materials)
            {
                mat.color = c;
            }
        }
    }

    void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    void MovePreview(Vector3 position)
    {
        if (previewObject != null)
            previewObject.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
    }

    // Update is called once per frame
    void Update()
    {
    }
}