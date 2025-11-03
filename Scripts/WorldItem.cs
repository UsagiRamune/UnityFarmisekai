using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldItem : MonoBehaviour
{
    public Item item;
    public int count;
    public TextMeshProUGUI countText;
    [SerializeField] Image  image;
    public float pickupDelay = 1f;   // seconds before magnet works
    public float magnetRange = 5f;     // how far player attracts items
    public float magnetSpeed = 10f;    // speed toward player
    private Transform player;
    private float spawnTime;
    public bool pickedUp = false;
    public bool selled = false;
    public void Initialise(Item newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        image.sprite = item.sprite;
        countText.text = count.ToString();
        countText.gameObject.SetActive(count > 1);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnTime = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
         if (pickedUp) return;
         if (selled) return;
        if (Time.time >= spawnTime + pickupDelay && player != null)
        {
            if (other.CompareTag("Player"))
            {
                int added = InventoryManager.Instance.AddItem(item, count);

                if (added > 0)
                {
                    count -= added;
                    countText.text = count.ToString();
                    countText.gameObject.SetActive(count > 1);
                }

                if (count <= 0)
                {
                    pickedUp = true;
                    DonutSpawner.instance.OnEntityDeath(gameObject);
                    Destroy(gameObject);
                }
            }
        }
    }
    private void Update()
    {
        FaceCamera();

        if (pickedUp || selled || player == null) return; // stop if already picked or sold

        // How many can actually fit
        int canFit = InventoryManager.Instance.GetAvailableSpace(item);
        if (canFit <= 0) return; // nothing fits, stop magnet

        if (Time.time >= spawnTime + pickupDelay)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < magnetRange)
            {
                // Move only by the amount that can fit
                float step = magnetSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, player.position, step);
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
