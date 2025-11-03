using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public enum SlotType
{
   Any,
   Equipment
}

public class InventorySlot : MonoBehaviour, IPointerDownHandler
{   
   public int slotIndex = -1;
   public SlotType slotType = SlotType.Any;
   public string itemName = "None";
   public int count;
   private InventoryItem _currentItem;
   public InventoryItem currentItem
   {
      get => _currentItem;
      set
      {
         _currentItem = value;
         UpdateSlotData();
      }
   }

   [SerializeField] private Image slotImage;
   public void OnPointerDown(PointerEventData eventData)
   {
      if (!PlayerMovement.Instance.inventoryOn)
      {
         // Inventory is closed → clicking selects the hotbar
         if(!PlacementSystem.instance.isHolding && !PlantingSystem.instance.holdingWater&&!CombatSystem.instance.isAttacking)
         InventoryManager.Instance.selectedIndex = slotIndex;
         InventoryManager.Instance.UpdateSelectionHighlight();
         return;
      }

      if (InventoryManager.Instance.itemBeingHeld != null)
      {
         if (eventData.button == PointerEventData.InputButton.Left)
            InventoryManager.Instance.PlaceItem(this, false);
         else if (eventData.button == PointerEventData.InputButton.Right)
            InventoryManager.Instance.PlaceItem(this, true);
         return;
      }
      if (currentItem == null) return;

      if (eventData.button == PointerEventData.InputButton.Left)
         InventoryManager.Instance.PickUpItem(currentItem, false);
      else if (eventData.button == PointerEventData.InputButton.Right)
         InventoryManager.Instance.PickUpItem(currentItem, true);
     
   }
   private void Update()
   {
      // toggle slot background visibility
      
   }
   public void UpdateSlotData()
   {
      if (slotImage != null)
         slotImage.enabled = (currentItem == null);
      if (_currentItem != null)
      {
         itemName = _currentItem.item.itemname;
         count = _currentItem.count;
      }
      else
      {
         itemName = "None";
         count = 0;
      }
   }

   
}
