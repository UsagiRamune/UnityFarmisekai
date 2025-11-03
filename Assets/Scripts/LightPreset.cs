using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "LightPreset", menuName = "Create LightPreset")]
public class LightPreset : ScriptableObject
{
    public Gradient ambient;
    public Gradient directionColor;
    public Gradient fogColor;
}
