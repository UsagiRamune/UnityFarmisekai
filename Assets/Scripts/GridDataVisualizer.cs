using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class GridDataVisualizer : MonoBehaviour
{
    private GridData gridData;
    [SerializeField] PlacementSystem placementSystem;
    public GameObject cellVisualizerPrefab;

    private List<GameObject> visualizers = new List<GameObject>();
    
    public void Start()
    {
        gridData = placementSystem.gridData;
    }

    public void UpdateVisualization()
    {
        // Clear old visuals
        foreach (var vis in visualizers)
            Destroy(vis);
        visualizers.Clear();

        if (gridData == null) return;
        // Access private dictionary using reflection
        var dictField = typeof(GridData).GetField("placeObjects",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var placeObjects = gridData.placeObjects;
        if (placeObjects == null) return;

        foreach (var kvp in placeObjects)
        {
            Vector3 pos = new Vector3(kvp.Key.x + 0.5f, 0.01f, kvp.Key.z + 0.5f); // center of the cell
            GameObject vis = Instantiate(cellVisualizerPrefab, pos, Quaternion.identity, transform);

            // Set text to show ID and Index
            var text = vis.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"{kvp.Value.ID},{kvp.Value.PlaceObjectIndex}";

            visualizers.Add(vis);
        }
    }

    public void ClearVisualization()
    {
        foreach (var vis in visualizers)
            Destroy(vis);
        visualizers.Clear();
    }

    private void Update()
    {
        UpdateVisualization();
        
    }
}
