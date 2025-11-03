using UnityEngine;
using UnityEngine.UI;

public class DirtTile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float elevationT = 100;
    public float elevationC;
    public float water;
    public float maxWater = 100;
    public GameObject dirt;
    public PlantGrowing plant;
    public Renderer rend;
    public GameObject canvas;
    public Image fillImage;
    void Start()
    {
        dirt = transform.Find("Dirt").gameObject;
        
    }

    // Update is called once per frame
    void Update()
    {
        FaceCamera();
        fillImage.fillAmount = water/maxWater;
        float targetNormalized = elevationT / 100f;
        elevationT -= 2 * Time.deltaTime;
        // Smooth current normalized value
        elevationC = Mathf.Lerp(elevationC, targetNormalized, Time.deltaTime * 5);
        float worldY = Mathf.Lerp(-0.05f, 0.05f, elevationC);
        dirt.transform.localPosition = new Vector3(dirt.transform.localPosition.x, worldY, dirt.transform.localPosition.z);

        if (elevationC <= 0)
        {
            PlacementSystem placement = FindFirstObjectByType<PlacementSystem>();
            placement.RemovePlacedTile(this.gameObject);

        }

        if (plant == null)
        {
            if(water >0)
            water -= 0.25f * Time.deltaTime;
            else 
                water = 0;
        }
        float wetness = Mathf.Clamp01(water / maxWater); // 0 to 1
        Color baseColor = new Color(0.5f, 0.3f, 0.1f); // dry dirt
        Color wetColor = new Color(0.2f, 0.15f, 0.1f); // darker wet dirt

        rend.material.color = Color.Lerp(baseColor, wetColor, wetness);
    }
    private void FaceCamera()
    {
        if (Camera.main == null) return;

        // Calculate direction from item to camera
        canvas.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
}
