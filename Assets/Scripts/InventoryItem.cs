using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI countText;
    public Item item;
    public int count = 1;
    public Transform CurrentSlot;
    private void Start()
    {
        RefreshCount();
    }
    
    void OnDisable() => HoverManager.Instance.Unregister(new HoverableItem { Target = image.rectTransform, ItemName = item.itemname });
    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.sprite;
        RefreshCount();
        HoverManager.Instance.Register(new HoverableItem
        {
            Target = image.rectTransform,
            ItemName = item.itemname
        });
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        countText.gameObject.SetActive(count > 1);
    }
}
