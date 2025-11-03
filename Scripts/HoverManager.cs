using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverManager : MonoBehaviour
{
    public static HoverManager Instance;
    private void Awake() => Instance = this;

    private List<HoverableItem> items = new List<HoverableItem>();

    public void Register(HoverableItem item) => items.Add(item);
    public void Unregister(HoverableItem item) => items.Remove(item);

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        foreach (var item in items)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(item.Target, mousePos))
            {
                TooltipManager.Instance.Show(item.ItemName);
                return; // stop at first match
            }
        }

        TooltipManager.Instance.Hide();
    }
}

[System.Serializable]
public class HoverableItem
{
    public RectTransform Target;
    public string ItemName;
}