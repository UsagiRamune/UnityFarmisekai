using UnityEngine;

public class TestSpawnItem : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickUp;

    public void PickupItem(int id)
    {
        inventoryManager.AddItem(itemsToPickUp[id]);
    }
}
