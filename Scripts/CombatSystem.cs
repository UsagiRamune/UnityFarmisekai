using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem instance;
    [Header("Visual Feedback")]
    [SerializeField] private GameObject targetIndicator;
    [SerializeField] private float indicatorHeightOffset = 1.5f;
    [Tooltip("ลาก Prefab ของเอฟเฟกต์ฟันดาบมาใส่ตรงนี้")]
    [SerializeField] private GameObject slashVfxPrefab; // << เพิ่มตัวแปรนี้เข้ามา

    // ... (Header อื่นๆ เหมือนเดิม) ...
    [Header("Combat Settings")]
    [SerializeField] private float combatRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float rotationSpeed = 15f;
    
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 0.8f;
    public float attackCooldown;
    public float attackCooltime;

    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;

    public Transform currentTarget;
    public bool isInCombatStance;
    public bool isAttacking;
    private List<Collider> hitEnemies = new List<Collider>();

    void Start()
    {
        instance = this;
    }
    void Update()
    {
        // --- 0. เช็คก่อนเลยว่าเป้าหมายตายรึยัง ---
        if (currentTarget == null && targetIndicator != null && targetIndicator.activeSelf)
        {
            targetIndicator.SetActive(false);
           // Debug.Log("Target was destroyed. Auto-unlocked.");
        }
        
        // --- 1. จัดการการเข้า Combat Stance ---
        
        if (Input.GetMouseButtonDown(0)&&isInCombatStance&&!PlacementSystem.instance.IsMouseOverInventory()&&!PlayerMovement.Instance.inventoryOn)
        {
            if (!isAttacking&& attackCooltime <=0)
            {
                PlacementSystem.instance.weapon.SetActive(true);
                PlacementSystem.instance.weapon.GetComponent<Renderer>().enabled = true;
                isAttacking  = true;
                PlayerMovement.Instance.animator.SetBool("Attack",true);
                PlayerMovement.Instance.animator.SetInteger("State",4);
                attackCooltime = 5;
            }
        }

        if (attackCooltime > 0)
        {
            attackCooltime -= Time.deltaTime;
        }
        else
        {
            attackCooltime = 0;
        }

        if (!isInCombatStance)
        {
            currentTarget = null;
        }
        // --- 4. จัดการการ Lock-on Target (ลูกกลิ้งเมาส์) ---
        if (isInCombatStance && Input.GetMouseButtonDown(1))
        {
            HandleLockOn();
        }

        // --- 5. จัดการการหันหน้า และอัปเดตตำแหน่งตัวชี้เป้า ---
        if (currentTarget != null)
        {
            RotateTowardsTarget();
            if (targetIndicator != null)
            {
                Vector3 indicatorPosition = currentTarget.position + (Vector3.up * indicatorHeightOffset);
                targetIndicator.transform.position = indicatorPosition;
                targetIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }

    public void PerformSlashAttack()
    {
        if (attackPoint == null)
        {
          //  Debug.LogError("Attack Point is not assigned!");
            return;
        }

        if (slashVfxPrefab != null)
        {
            Instantiate(slashVfxPrefab, attackPoint.position, transform.rotation);
        }
    
        hitEnemies.Clear();
        RaycastHit[] hits = Physics.SphereCastAll(attackPoint.position, attackRadius, transform.forward, attackRange, enemyLayer);

        foreach (RaycastHit hit in hits)
        {
            if (!hitEnemies.Contains(hit.collider))
            {
                hitEnemies.Add(hit.collider);

                // --- ส่วนที่แก้ไข ---
                // ลองดึงสคริปต์ AiController จากสิ่งที่ชน
                var aiController = hit.collider.GetComponent<Ai.AiController>();
                if (aiController != null)
                {
                    // ถ้าเจอสคริปต์ (แปลว่านี่คือศัตรูจริงๆ)
                    // ดึงค่าดาเมจจาก GameManager แล้วสั่งให้ศัตรูเจ็บตัว
                    float damageToDeal = GameManager.Instance.playerDamage;
                    aiController.TakeDamage(damageToDeal);
                }
                /*var aiCombat = hit.collider.GetComponent<Ai.AiCombat>();
                if (aiCombat != null)
                {
                    // ถ้าเจอสคริปต์ (แปลว่านี่คือศัตรูจริงๆ)
                    // ดึงค่าดาเมจจาก GameManager แล้วสั่งให้ศัตรูเจ็บตัว
                    float damageToDeal = GameManager.Instance.playerDamage;
                    aiCombat.TakeDamage(damageToDeal);
                }*/
                else
                {
                    // ถ้าไม่เจอสคริปต์ อาจจะชนโดนอย่างอื่นที่ไม่ใช่ AI
                    Debug.Log("Slashed something, but it's not an AI with AiController: " + hit.collider.name);
                }
                // ------------------
            }
        }
    }
    
    // ... (ฟังก์ชัน HandleLockOn และ RotateTowardsTarget เหมือนเดิมเป๊ะ) ...
    private void HandleLockOn()
    {

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        bool unlocked = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, enemyLayer))
        {
            if (Vector3.Distance(transform.position, hit.transform.position) <= combatRadius)
            {
                currentTarget = (currentTarget == hit.transform) ? null : hit.transform;
                unlocked = (currentTarget == null);
            }
        }
        else
        {
            currentTarget = null;
            unlocked = true;
        }

        if (targetIndicator != null)
        {
            bool hasTarget = currentTarget != null;
            targetIndicator.SetActive(hasTarget);
          //  if (hasTarget) Debug.Log("Locked on: " + currentTarget.name);
         //   if (unlocked) Debug.Log("Target Unlocked.");
        }
    }
    private void RotateTowardsTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRadius);
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            Gizmos.DrawWireSphere(attackPoint.position + transform.forward * attackRange, attackRadius);
            Gizmos.DrawLine(attackPoint.position + transform.right * attackRadius, attackPoint.position + transform.forward * attackRange + transform.right * attackRadius);
            Gizmos.DrawLine(attackPoint.position - transform.right * attackRadius, attackPoint.position + transform.forward * attackRange - transform.right * attackRadius);
        }
    }
}