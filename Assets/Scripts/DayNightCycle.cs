using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

enum TimeState
{
    Morning,
    Afternoon,
    Evening,
    Night
}
public class DayNightCycle : MonoBehaviour
{
    [SerializeField] Light directionalLight;
    [SerializeField] private LightPreset preset;
    public float dayLength = 360f; //6min

    public float currentHour = 6f;
    public UnityEvent<float> onHourChanged;
    public TextMeshProUGUI clockText;
    private float blinkTimer;
    private bool showColon = true;
    float timePercent;

    private void OnValidate()
    {
        if (directionalLight != null)
        {
            return;
        }

        if (RenderSettings.sun != null)
        {
            directionalLight =  RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentHour += (24f/ dayLength) * Time.deltaTime;
        timePercent = currentHour / 24f;
        if (currentHour >= 24f) currentHour -= 24f;
        if (Mathf.Abs(currentHour % 1f) < (Time.deltaTime * 24f / dayLength))
        {
            HourChange(currentHour);

        }
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= 0.5f)
        {
            showColon = !showColon;
            blinkTimer = 0f;
        }
        UpdateClockText();
        UpdateLight();
    }

    void HourChange(float hour)
    {
        onHourChanged?.Invoke(hour);
    }
    void UpdateClockText()
    {
        int hour = Mathf.FloorToInt(currentHour);

        string colon = showColon ? ":" : " ";
        clockText.text = $"{hour:00}{colon}00";
    }

    void UpdateLight()
    {
        
        RenderSettings.ambientLight = preset.ambient.Evaluate(timePercent);
        RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = preset.directionColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f)-90f,-90f,0f));
        }
    }
}
