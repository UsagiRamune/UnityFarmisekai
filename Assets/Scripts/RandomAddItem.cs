using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct RandomItem
{
    public Item item;
    public int minAmount;   // minimum quantity
    public int maxAmount;   // maximum quantity
}
public class RandomAddItem : MonoBehaviour
{
    public RandomItem[] possibleItems;
    [SerializeField] RectTransform rect;
    public float pickupDelay = 1f;   // seconds before magnet works
    public float magnetRange = 5f;     // how far player attracts items
    public float magnetSpeed = 10f;    // speed toward player
    private Transform player;
    private float spawnTime;
    public bool pickedUp = false;
    public Item item;
    public int count;
    public float speed = 2f;        // how fast it pulses
    public float minScale = 0.8f;   // smallest size
    public float maxScale = 1.2f;   // largest size
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnTime = Time.time;
        if (possibleItems.Length > 0)
        {
            RandomItem chosen = possibleItems[Random.Range(0, possibleItems.Length)];
            item = chosen.item;
            count = Random.Range(chosen.minAmount, chosen.maxAmount + 1);
            
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (pickedUp) return;
        if (Time.time < spawnTime + pickupDelay || player == null) return;

        if (other.CompareTag("Player"))
        {
            pickedUp = true;

            int added = InventoryManager.Instance.AddItem(item, count);
            int leftover = count - added;

            if (leftover <= 0)
            {
                // Everything fit in inventory
                Destroy(gameObject);
            }
            else
            {
                // Some items couldn't fit, keep them in the world item
                count = leftover;
                GetComponentInChildren<TextMeshProUGUI>().text = leftover.ToString();
                pickedUp = false; // allow next pickup attempt
            }
        }
    }
    private void Update()
    {
        
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
        rect.localScale = new Vector3(scale, scale, 1f);
        FaceCamera();
        if (!InventoryManager.Instance.CanAdd(item, count))
        {
            return;
        }
        if (Time.time >= spawnTime + pickupDelay && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < magnetRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    magnetSpeed * Time.deltaTime
                );
            }
        }
    }

    private void FaceCamera()
    {
        if (Camera.main == null) return;

        // Calculate direction from item to camera
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
}
