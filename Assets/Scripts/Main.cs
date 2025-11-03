using Data;
using Manager;
using UnityEngine;

public class Main : MonoBehaviour
{
    private UserData userData;

    private void Start()
    {
        // userData = SaveManager.Load();
        //
        // userData.username = "Achiku";
        // userData.inventory.items.Add("Corn Seed");
        // userData.inventory.items.Add("Axe");
        //
        // SaveManager.Save(userData);
        //
        // Debug.Log($"Hello {userData.userName}, you have {userData.inventory.items.Count} items.");
    }
}
