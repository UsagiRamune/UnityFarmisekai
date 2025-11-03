using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    public bool isCheat;
    public bool isImmortal;
    public float maxHealth;
    public float currentHealth;
    public float maxRegenRate;
    float currentRegenRate;
    public float regenRateRate;
    public float afterHitCooldown;
    float afterHitCoolTime;
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform textSpawnPoint;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    public Transform cameraTransform;

    private Vector3 moveDirection;
    public TextMeshProUGUI speedText, moneyText,healthText;
    public float gamespd = 1;
    public int money;
    public bool inventoryOn = false;
    public GameObject inventory;
    private Vector3 buildFaceDirection;
    public float buildRotationSpeed = 100;
    public Transform buildPoint;
    public Animator animator;
    public AnimatorStateInfo stateInfo ;
    public float iFrameDuration = 0.5f; // how long AI is invincible after getting hit
    private bool isInvincible = false;
    public GameObject playermodel;
    void Start()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        speedText.text = "x" + gamespd.ToString();
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (isCheat)
            isImmortal = true;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (PlacementSystem.instance.isHolding||PlantingSystem.instance.holdingWater||CombatSystem.instance.isAttacking||inventoryOn)
        {
            // Stop movement immediately
            moveDirection = Vector3.zero;
            rb.linearVelocity = Vector3.zero; // also reset Rigidbody velocity
            animator.SetBool("IsWalking", false);
        }
        else
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            animator.SetBool("IsWalking", false);
            Vector3 input = new Vector3(moveX, 0, moveZ);
            if (input != Vector3.zero)
            {
                animator.SetBool("IsWalking", true);
            }
            if (input.magnitude > 1)
            {
                input.Normalize();
                
            }
                
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();
            moveDirection = cameraForward * input.z + cameraRight * input.x;
        }
        if (PlacementSystem.instance.isHolding|| PlantingSystem.instance.holdingWater)
        {
            // Get the position of the preview object
            Vector3 direction = buildPoint.transform.position - transform.position;
            direction.y = 0; // ignore vertical
            if (direction.sqrMagnitude > 0.01f)
                buildFaceDirection = direction.normalized;
        }

        if (afterHitCoolTime <= 0)
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += Time.deltaTime * currentRegenRate;
            }
            if(currentRegenRate < maxRegenRate)
                currentRegenRate += Time.deltaTime *  regenRateRate;
        }
        else
        {
            afterHitCoolTime -= Time.deltaTime;
        }
       
        if (isCheat)
        {
            speedText.enabled = true;
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (gamespd > 0.015625f)
                {
                    gamespd /= 2f;
                    Time.timeScale = gamespd;
                    speedText.text = "x" + gamespd.ToString();
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (gamespd < 64f)
                {
                    gamespd *= 2f;
                    Time.timeScale = gamespd;
                    speedText.text = "x" + gamespd.ToString();
                }
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
               isImmortal = !isImmortal;
            }
        }
        else
        {
            speedText.enabled = false;
        }
    

        healthText.text = "Health:"  + Mathf.RoundToInt(currentHealth);
        moneyText.text = "Money:" + money.ToString();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInventory();
           
        }
        // Example: press 1 to plant first crop
        // if (Input.GetKeyDown(KeyCode.Keypad1))
        // {
        //     PlantingSystem.instance.StartPlanting(1);
        // }
        //
        // // Example: press 2 to water
        // if (Input.GetKeyDown(KeyCode.Keypad2))
        // {
        //     PlantingSystem.instance.StartPlanting(2);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Y))
        // {
        //     PlantingSystem.instance.StartWatering();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.H))
        // {
        //     PlantingSystem.instance.StartHarvesting();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     PlacementSystem.instance.StartPlacement(0);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     PlacementSystem.instance.StartPlacement(1);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     PlacementSystem.instance.StopPlacement();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     PlacementSystem.instance.StartPlacement(2);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     PlacementSystem.instance.StartPlacement(3);
        // }
    }
    
    void FixedUpdate()
    {
        // Move the player
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb.linearVelocity.y; // keep vertical velocity intact (gravity, jumps)
        rb.linearVelocity = velocity;

        // Rotate player smoothly toward movement direction
        if (!PlacementSystem.instance.isHolding && moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else if (PlacementSystem.instance.isHolding && buildFaceDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(buildFaceDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, buildRotationSpeed * Time.fixedDeltaTime);
        }
        else if (PlantingSystem.instance.holdingWater && buildFaceDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(buildFaceDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, buildRotationSpeed * Time.fixedDeltaTime);
        }
    }
    public void TakeDamage(float damageAmount)
    {
        if(isImmortal)  return;
        if (isInvincible) return; // Ignore if still invincible

        currentHealth -= damageAmount;
        afterHitCoolTime = afterHitCooldown;
        currentRegenRate = 0;
        if (inventoryOn)
        {
            inventoryOn = false;
            inventory.SetActive(false);
            InventoryItem held = InventoryManager.Instance.itemBeingHeld;
            if (held != null)
            {
                if (!InventoryManager.Instance.TryAutoPlace(held))
                {
                    held.transform.SetParent(InventoryManager.Instance.cursorSlot.transform);
                    held.transform.localPosition = Vector3.zero;
                    held.image.raycastTarget = true;
                    InventoryManager.Instance.cursorSlot.currentItem = held;
                }
                InventoryManager.Instance.itemBeingHeld = null;
            }
        }
          
        // Spawn damage text
        if (damageTextPrefab != null && textSpawnPoint != null)
        {
            GameObject textInstance = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity);
            var damageTextScript = textInstance.GetComponent<DamageText>();
            if (damageTextScript != null)
            {
                damageTextScript.SetDamageText(damageAmount);
            }
        }

        // Trigger i-frame
        StartCoroutine(Invincibility());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private IEnumerator Invincibility()
    {
        isInvincible = true;

        // (Optional) Flash effect to show i-frames
        // Example: disable/enable renderer quickly
        
        for (int i = 0; i < 5; i++)
        {
            foreach (var smr in playermodel.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.enabled = false;
            }
            foreach (var renderer in playermodel.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(0.1f);
            foreach (var smr in playermodel.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.enabled = true;
            }
            foreach (var renderer in playermodel.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
        

        yield return new WaitForSeconds(iFrameDuration);
        isInvincible = false;
    }
    public void Die()
    {
        Debug.Log("Player has died! Game Over.");
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleInventory()
    {
        if (!PlacementSystem.instance.isHolding && !PlantingSystem.instance.holdingWater)
        {
            inventoryOn = !inventoryOn;
            if (!inventoryOn)
            {
                RecipeManager.Instance.currentRecipe = 0;
            }
            inventory.SetActive(inventoryOn);
            RecipeManager.Instance.GenerateRecipeUI(RecipeType.Default);
            InventoryItem held = InventoryManager.Instance.itemBeingHeld;
            if (held != null)
            {
                if (!InventoryManager.Instance.TryAutoPlace(held))
                {
                    // held.transform.SetParent(InventoryManager.Instance.cursorSlot.transform);
                    // held.transform.localPosition = Vector3.zero;
                    // held.image.raycastTarget = true;
                    // InventoryManager.Instance.cursorSlot.currentItem = held;
                    InventoryManager.Instance.DropHeldItem();
                }
                InventoryManager.Instance.itemBeingHeld = null;
                InventoryManager.Instance.CraftPrevent.SetActive(false);
            }
            
        }
    }
}