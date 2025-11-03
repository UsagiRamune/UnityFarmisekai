using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

[System.Serializable]
public struct StarterItem
{
    public Item item;
    public int amount;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public Transform DragParent;
    public InventorySlot cursorSlot;
    public InventorySlot[] slots;
    public GameObject inventoryItemPrefab;
    public InventoryItem itemBeingHeld;
    public GameObject worldItemPrefab; // prefab for world items
    public Transform dropPoint; // assign in Inspector (child of Player)
    public RectTransform[] Rect;
    public InventorySlot trashSlot;
    public int selectedIndex = 0;
    public RectTransform selectionHighlight;
    float rightClickTimer = 0f;
    float pickInterval = 0.5f; // starts slow
    float pickAcceleration = 0.75f; // interval decreases over time
    public int inventorySize;
    public StarterItem[] starterItems;
    public GameObject CraftPrevent;

    public delegate void InventoryChanged();

    public event InventoryChanged OnInventoryChanged;


    private void Start()
    {
        Instance = this;
        foreach (var starter in starterItems)
        {
            if (starter.item != null && starter.amount > 0)
                AddItem(starter.item, starter.amount);
        }

        UpdateSelectionHighlight();
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private void Update()
    {
        if (!PlacementSystem.instance.isHolding && !PlantingSystem.instance.holdingWater &&
            !CombatSystem.instance.isAttacking)
        {
            HandleHotbarInput();
        }

        if (itemBeingHeld != null)
        {
            itemBeingHeld.transform.position = Input.mousePosition;
            TooltipManager.Instance.suppressTooltip = true;
            CraftPrevent.SetActive(true);
        }
        else
        {
            TooltipManager.Instance.suppressTooltip = false;
            CraftPrevent.SetActive(false);
        }
        if (itemBeingHeld != null && Input.GetMouseButton(1)) // right-click hold
        {
            rightClickTimer += Time.deltaTime;
            if (rightClickTimer >= pickInterval)
            {
                InventoryItem underMouse = GetItemUnderMouse();
                if (underMouse != null && underMouse.item.stackable)
                {
                    PickUpItem(underMouse, true);
                }

                rightClickTimer = 0f;
                if (pickInterval >= 0.01f)
                    pickInterval *= pickAcceleration;
            }
        }

        if (itemBeingHeld != null && Input.GetMouseButtonDown(1))
        {
            if (!IsMouseOverInventory())
            {
                DropHeldItem();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            rightClickTimer = 0f;
            pickInterval = 0.5f;
        }
    }

    public bool IsMouseOverInventory()
    {
        Vector2 mousePos = Input.mousePosition;
        foreach (var rect in Rect)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, null))
                return true;
        }

        return false;
    }

    public void DropHeldItem()
    {
        if (itemBeingHeld == null) return;
        OnInventoryChanged?.Invoke();
        // Spawn world item prefab
        float radius = 1f; // adjust how far items can scatter
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 dropPosition = dropPoint.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // Spawn world item prefab
        GameObject dropped = Instantiate(worldItemPrefab, dropPosition, Quaternion.identity);
        WorldItem worldItem = dropped.GetComponentInChildren<WorldItem>();
        worldItem.Initialise(itemBeingHeld.item, itemBeingHeld.count);

        // Clear cursor
        Destroy(itemBeingHeld.gameObject);
        itemBeingHeld = null;
        cursorSlot.currentItem = null;
    }

    private bool IsItemAllowedInSlot(Item item, InventorySlot slot)
    {
        switch (slot.slotType)
        {
            case SlotType.Any:
                return true;
            case SlotType.Equipment:
                return item.type == ItemType.Equipment;
            default:
                return false;
        }
    }

    public void PickUpItem(InventoryItem clickedItem, bool rightClick = false)
    {
        if (clickedItem == null) return;

        InventorySlot originalSlot = clickedItem.CurrentSlot.GetComponent<InventorySlot>();
        OnInventoryChanged?.Invoke();

        // Right-click on same stack (merge into cursor)
        if (rightClick && itemBeingHeld != null && itemBeingHeld.item == clickedItem.item && clickedItem.item.stackable)
        {
            if (itemBeingHeld.count < itemBeingHeld.item.maxStack)
            {
                itemBeingHeld.count++;
                itemBeingHeld.RefreshCount();

                clickedItem.count--;
                clickedItem.CurrentSlot.GetComponent<InventorySlot>().count = clickedItem.count;
                clickedItem.RefreshCount();

                if (clickedItem.count <= 0)
                {
                    InventorySlot slot = clickedItem.CurrentSlot.GetComponent<InventorySlot>();
                    if (slot != null)
                    {
                        slot.currentItem = null;
                        slot.count = 0;
                    }

                    ClearSlot(clickedItem.CurrentSlot);
                }
            }

            return;
        }

        // Left-click → pick full stack
        if (!rightClick)
        {
            itemBeingHeld = clickedItem;
            MoveToCursor(itemBeingHeld);

            if (originalSlot != null)
            {
                originalSlot.currentItem = null;
                originalSlot.count = 0;
            }
        }
        else // Right-click → pick partial stack
        {
            if (clickedItem.count > 1)
            {
                // Spawn temporary cursor item
                itemBeingHeld = SpawnTempItem(clickedItem, 1);

                clickedItem.count--;
                clickedItem.CurrentSlot.GetComponent<InventorySlot>().count = clickedItem.count;
                clickedItem.RefreshCount();
            }
            else
            {
                itemBeingHeld = clickedItem;
                MoveToCursor(itemBeingHeld);

                if (originalSlot != null)
                {
                    originalSlot.currentItem = null;
                    originalSlot.count = 0;
                }

                if (clickedItem.count <= 0)
                {
                    ClearSlot(clickedItem.CurrentSlot);
                }
            }
        }
    }

    public bool TryAutoPlace(InventoryItem held)
    {
        if (held.item.stackable)
        {
            for (int i = 0; i < inventorySize + 10; i++) // limit
            {
                InventorySlot slot = slots[i];
                if (slot.currentItem != null &&
                    slot.currentItem.item == held.item &&
                    slot.currentItem.count < slot.currentItem.item.maxStack)
                {
                    int space = slot.currentItem.item.maxStack - slot.currentItem.count;
                    int transfer = Mathf.Min(space, held.count);

                    slot.currentItem.count += transfer;
                    slot.currentItem.RefreshCount();

                    held.count -= transfer;
                    held.RefreshCount();

                    if (held.count <= 0)
                    {
                        Destroy(held.gameObject);
                        return true;
                    }
                }
            }
        }

        // 2. Place in first empty slot within inventorySize
        for (int i = 0; i < inventorySize + 10; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.currentItem == null)
            {
                held.transform.SetParent(slot.transform);
                held.transform.localPosition = Vector3.zero;
                held.image.raycastTarget = true;

                slot.currentItem = held;
                slot.count = held.count;
                held.CurrentSlot = slot.transform;

                return true;
            }
        }

        return false; // no space within unlocked slots
    }

    public void PlaceItem(InventorySlot targetSlot, bool rightClick = false)
    {
        if (itemBeingHeld == null || targetSlot == null) return;
        InventoryItem slotItem = targetSlot.currentItem;
        OnInventoryChanged?.Invoke();
        if (!IsItemAllowedInSlot(itemBeingHeld.item, targetSlot))
            return;
        if (!rightClick && targetSlot == trashSlot)
        {
            // If slot already has an item → destroy it
            if (slotItem != null)
            {
                Destroy(slotItem.gameObject);
            }

            // Place new item in trash
            targetSlot.currentItem = itemBeingHeld;
            targetSlot.count = itemBeingHeld.count;
            itemBeingHeld.CurrentSlot = targetSlot.transform;
            itemBeingHeld.transform.SetParent(targetSlot.transform);
            itemBeingHeld.transform.localPosition = Vector3.zero;
            itemBeingHeld.image.raycastTarget = true;

            // Clear cursor
            itemBeingHeld = null;
            cursorSlot.currentItem = null;
            return;
        }


        // Right-click → pick 1 from slot to cursor
        if (rightClick)
        {
            if (slotItem != null && slotItem.item == itemBeingHeld.item && slotItem.item.stackable)
            {
                PickUpItem(slotItem, true);
            }

            return;
        }

        // Left-click → empty slot
        if (slotItem == null)
        {
            targetSlot.currentItem = itemBeingHeld;
            targetSlot.count = itemBeingHeld.count;
            itemBeingHeld.CurrentSlot = targetSlot.transform;
            itemBeingHeld.transform.SetParent(targetSlot.transform);
            itemBeingHeld.transform.localPosition = Vector3.zero;
            itemBeingHeld.image.raycastTarget = true;
            itemBeingHeld = null;
            return;
        }

        // Merge same stack
        if (slotItem.item == itemBeingHeld.item && itemBeingHeld.item.stackable)
        {
            int space = slotItem.item.maxStack - slotItem.count;
            int transfer = Mathf.Min(space, itemBeingHeld.count);

            slotItem.count += transfer;
            targetSlot.count = slotItem.count;
            slotItem.RefreshCount();

            itemBeingHeld.count -= transfer;
            itemBeingHeld.RefreshCount();

            if (itemBeingHeld != null && itemBeingHeld.count <= 0)
            {
                Destroy(itemBeingHeld.gameObject);
                itemBeingHeld = null;
            }

            return;
        }

        // Swap different items
        InventoryItem temp = slotItem;
        targetSlot.currentItem = itemBeingHeld;
        targetSlot.count = itemBeingHeld.count;
        itemBeingHeld.CurrentSlot = targetSlot.transform;
        itemBeingHeld.transform.SetParent(targetSlot.transform);

        itemBeingHeld.transform.localPosition = Vector3.zero;
        itemBeingHeld.image.raycastTarget = true;


        itemBeingHeld = temp;
        itemBeingHeld.transform.SetParent(DragParent);
        itemBeingHeld.CurrentSlot = cursorSlot.transform;
        itemBeingHeld.image.raycastTarget = false;
        cursorSlot.currentItem = itemBeingHeld;
    }

    private void MoveToCursor(InventoryItem item)
    {
        item.CurrentSlot = cursorSlot.transform;
        cursorSlot.currentItem = item;
        item.transform.SetParent(DragParent);
        item.image.raycastTarget = false;
    }

    private void ClearSlot(Transform slot)
    {
        InventorySlot invSlot = slot.GetComponent<InventorySlot>();
        if (invSlot != null)
        {
            invSlot.currentItem = null;
            invSlot.count = 0;
            invSlot.UpdateSlotData();
            foreach (Transform child in slot)
            {
                // Skip trash icon
                if (invSlot == trashSlot && child.name == "TrashIcon")
                    continue;

                Destroy(child.gameObject);
            }
        }
    }

    private InventoryItem SpawnTempItem(InventoryItem original, int amount)
    {
        GameObject obj = Instantiate(inventoryItemPrefab, DragParent);
        InventoryItem temp = obj.GetComponent<InventoryItem>();
        temp.InitialiseItem(original.item);
        temp.count = amount;
        temp.RefreshCount();
        temp.CurrentSlot = cursorSlot.transform;


        temp.image.raycastTarget = false;
        cursorSlot.currentItem = temp;
        return temp;
    }

    public InventoryItem GetItemUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            InventoryItem item = result.gameObject.GetComponent<InventoryItem>();
            if (item != null)
                return item;
        }

        return null;
    }

    public bool HasItem(Item item, int amount)
    {
        int totalCount = 0;

        foreach (var slot in slots)
        {
            if (slot.currentItem != null && slot.currentItem.item == item)
            {
                totalCount += slot.currentItem.count;
                if (totalCount >= amount)
                    return true; // enough found
            }
        }

        return false; // not enough
    }

    public bool TryConsumeItemFromInventory(Item item, int amount)
    {
        // Step 1: Check first — make sure there’s enough total
        if (!HasItem(item, amount))
            return false;

        // Step 2: Consume across slots
        int remaining = amount;

        foreach (var slot in slots)
        {
            if (slot.currentItem != null && slot.currentItem.item == item)
            {
                int available = slot.currentItem.count;
                int toConsume = Mathf.Min(available, remaining);

                slot.currentItem.count -= toConsume;
                slot.count = slot.currentItem.count;
                slot.currentItem.RefreshCount();

                if (slot.currentItem.count <= 0)
                    ClearSlot(slot.transform);

                remaining -= toConsume;
                if (remaining <= 0)
                    break;
            }
        }

        OnInventoryChanged?.Invoke();

        return true;
    }

    public List<InventorySlot> ConsumeItemAndReturnFreedSlots(Item item, int amount)
    {
        List<InventorySlot> freedSlots = new List<InventorySlot>();
        int remaining = amount;

        foreach (var slot in slots)
        {
            if (slot.currentItem != null && slot.currentItem.item == item)
            {
                int toConsume = Mathf.Min(slot.currentItem.count, remaining);
                slot.currentItem.count -= toConsume;
                slot.count = slot.currentItem.count;
                slot.currentItem.RefreshCount();

                if (slot.currentItem.count <= 0)
                {
                    ClearSlot(slot.transform);
                    freedSlots.Add(slot);
                }

                remaining -= toConsume;
                if (remaining <= 0) break;
            }
        }

        return freedSlots;
    }
    public int AddItem(Item item, int amount = 1)
    {
        int remaining = amount;
        int maxSlotsToCheck = Mathf.Min(slots.Length, inventorySize + 10);

        // Fill existing stacks
        if (item.stackable)
        {
            for (int i = 0; i < maxSlotsToCheck; i++)
            {
                InventorySlot slot = slots[i];
                if (slot.currentItem != null && slot.currentItem.item == item)
                {
                    int space = item.maxStack - slot.currentItem.count;
                    int toAdd = Mathf.Min(space, remaining);
                    if (toAdd > 0)
                    {
                        slot.currentItem.count += toAdd;
                        slot.currentItem.RefreshCount();
                        remaining -= toAdd;
                    }
                    if (remaining <= 0) break;
                }
            }
        }

        // Fill empty slots
        for (int i = 0; i < maxSlotsToCheck; i++)
        {
            InventorySlot slot = slots[i];
            if (remaining <= 0) break;

            if (slot.currentItem == null)
            {
                int toAdd = Mathf.Min(item.stackable ? item.maxStack : 1, remaining);
                SpawnNewItem(item, slot, toAdd);
                remaining -= toAdd;
            }
        }

        OnInventoryChanged?.Invoke();
        return amount - remaining;
    }

    public int GetTotalItemCount(Item item)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.currentItem != null && slot.currentItem.item == item)
                total += slot.currentItem.count;
        }

        return total;
    }
    public int GetAvailableSpace(Item item)
    {
        int remainingSpace = 0;

        // Only consider the unlocked slots (inventorySize)
        for (int i = 0; i < inventorySize+10; i++)
        {
            InventorySlot slot = slots[i];

            // Space in existing stack
            if (slot.currentItem != null && slot.currentItem.item == item)
            {
                remainingSpace += item.maxStack - slot.currentItem.count;
            }
            // Empty slot
            else if (slot.currentItem == null)
            {
                remainingSpace += item.stackable ? item.maxStack : 1;
            }
        }

        return remainingSpace;
    }
    public bool CanAdd(Item item, int amount = 1)
    {
        int remaining = amount;
        int maxSlotsToCheck = Mathf.Min(slots.Length, inventorySize + 10);

        // Existing stacks
        for (int i = 0; i < maxSlotsToCheck; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.currentItem != null && slot.currentItem.item == item)
            {
                int space = item.maxStack - slot.currentItem.count;
                remaining -= Mathf.Min(space, remaining);
                if (remaining <= 0) return true;
            }
        }

        // Empty slots
        for (int i = 0; i < maxSlotsToCheck; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.currentItem == null)
            {
                int canFit = item.stackable ? item.maxStack : 1;
                remaining -= Mathf.Min(canFit, remaining);
                if (remaining <= 0) return true;
            }
        }

        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot, int count)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
        inventoryItem.count = count;
        inventoryItem.RefreshCount();
        inventoryItem.CurrentSlot = slot.transform;
        slot.currentItem = inventoryItem;
    }

    private void HandleHotbarInput()
    {
        // Scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f)
            selectedIndex = (selectedIndex + 1) % 10;
        else if (scroll > 0f)
            selectedIndex = (selectedIndex - 1 + 10) % 10;

        // Number keys (Alpha1 = key "1", etc.)
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedIndex = i;
            }
        }

        UpdateSelectionHighlight();
    }

    public void UpdateSelectionHighlight()
    {
        if (selectionHighlight != null && slots.Length > selectedIndex)
        {
            selectionHighlight.transform.position = (slots[selectedIndex].transform.position);
        }

        UseSelectedItem();
    }

    public bool TryConsumeSelectedItem(int amount = 1)
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length) return false;

        InventorySlot slot = slots[selectedIndex];
        if (slot.currentItem == null || slot.currentItem.count < amount) return false;

        slot.currentItem.count -= amount;
        slot.count = slot.currentItem.count;

        bool becameEmpty = false;

        if (slot.currentItem.count <= 0)
        {
            ClearSlot(slot.transform);
            becameEmpty = true;
        }
        else
        {
            slot.currentItem.RefreshCount();
        }

        // Fire event once after consumption
        OnInventoryChanged?.Invoke();

        return becameEmpty;
    }

    public void UseSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length) return;

        InventoryItem currentItem = slots[selectedIndex].currentItem;
        if (currentItem == null)
        {
            PlacementSystem.instance.StopPlacement();
            CombatSystem.instance.isInCombatStance = false;
            return;
        }

        Item item = currentItem.item;
        switch (item.action)
        {
            case ItemAction.Plant:
                PlantingSystem.instance.StartPlanting(item.iD);
                CombatSystem.instance.isInCombatStance = false;
                break;
            case ItemAction.Water:
                PlantingSystem.instance.StartWatering();
                CombatSystem.instance.isInCombatStance = false;
                break;
            case ItemAction.Harvest:
                PlantingSystem.instance.StartHarvesting();
                CombatSystem.instance.isInCombatStance = false;
                break;
            case ItemAction.Build:
                if (item.iD == 1)
                {
                    PlacementSystem.instance.StartPlacement(0);
                }
                else
                    PlacementSystem.instance.StartPlacement(item.iD - 300);

                CombatSystem.instance.isInCombatStance = false;
                break;
            case ItemAction.Attack:
                PlacementSystem.instance.StopPlacement();
                CombatSystem.instance.isInCombatStance = true;
                break;
            case ItemAction.None:
                PlacementSystem.instance.StopPlacement();
                CombatSystem.instance.isInCombatStance = false;
                break;
            default:
                // do nothing
                break;
        }
    }
}