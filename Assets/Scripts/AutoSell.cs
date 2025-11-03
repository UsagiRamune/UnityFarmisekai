using System;
using TMPro;
using UnityEngine;

public class AutoSell : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public PlantDataSO plantData;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            WorldItem item = other.GetComponent<WorldItem>();
            if (item == null) return;
            if(item.pickedUp) return;
            if (item.selled) return;
            if (item.item.type == ItemType.Crop)
            {
                item.selled = true;

                PlayerMovement.Instance.money += plantData.plantDatas[item.item.iD - 101].BaseSellCost * item.count;
                Destroy(other.gameObject);
            }
        }

    }

    private void Update()
    {
        FaceCamera();
    }
    private void FaceCamera()
    {
        if (Camera.main == null) return;

        // Calculate direction from item to camera
        Text.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
    
}
