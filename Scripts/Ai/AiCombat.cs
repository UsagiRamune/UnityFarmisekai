using UnityEngine;
using System.Collections.Generic;

namespace Ai
{
    [RequireComponent (typeof(AiController))]
    public class AiCombat : MonoBehaviour
    {
        [Header("Effects & UI")]
        [SerializeField] private GameObject damageTextPrefab;
        
        [Tooltip("จุดที่จะให้ Damage Text เกิด")]
        [SerializeField] private Transform textSpawnPoint;

        [SerializeField] private GameObject meleeAttackVfxPrefab;
        [SerializeField] private GameObject rangeAttackVfxPrefab;

        [SerializeField] private Transform attackSpawnPoint;

        public GameObject loot;

        public enum AttackType
        {
            Melee,
            Range
        }
        
        [Header("Combat Status")]
        public AttackType attackType = AttackType.Melee;

        public float attackRange = 1.5f;
        public float cooltimeBeforeAttack = 1.5f;
        public float attackCooldown = 1.5f;
        public float currentHealth;
        
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;

        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float projectileLifeTime = 3f;
        
        // Components
        private Animator _animator;
        private Transform _target;
        private PlayerMovement _playerMovement;
        private DonutSpawner _donutSpawner; // ต้องใช้ตอนตาย
        
        //State
        private bool _isAttacking = false;
        private float _attackTimer; // ตัวนับเวลาง้างตี
        private float _attackCooldownTimer; // ตัวนับ Cooldown

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                currentHealth = GameManager.Instance.enemyMaxHealth;
            }
            else
            {
                currentHealth = 100f;
                Debug.LogWarning("GameManager not found!.");
            }

            if (attackType == AttackType.Range)
            {
                attackRange = 8.0f;
            }
        }
        
        // AiController จะเรียกใช้ฟังก์ชันนี้เพื่อ Update ให้
        public void SetDependencies(Transform target, PlayerMovement player, DonutSpawner spawner)
        {
            _target = target;
            _playerMovement = player;
            _donutSpawner = spawner;
        }
        
        void Update()
        {
            // ถ้า AiContoller ไม่ได้สั่งให้โจมตี ก็ไม่ต้องทำอะไร
            if (!_isAttacking) return;
            
            // นับ Cooldown หลังโจมตี
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
            
            // นับเวลาง้างตี
            if (_attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
                
                // ถ้าง้างเสร็จแล้ว จะโจมตี
                if (_attackTimer <= 0)
                {
                    PerformAttack();

                    _isAttacking = false; // หยุดโจมตี รอ AiController สั่งโจมตีใหม่
                    _attackCooldownTimer = attackCooldown; // เริ่มนับ Cooldown

                    //_animator.SetInteger("State", 1);
                }
            }
        }
        
        // สั่งให้เริ่มการโจมตี
        public void StartAttack()
        {
            // เช็คว่าไม่ติด Cooldown กับไม่ได้กำลังง้างตีอยู่
            if (!_isAttacking && _attackCooldownTimer <= 0 && _attackTimer <= 0)
            {
                _isAttacking = true;
                _attackTimer = cooltimeBeforeAttack; // เริ่มนับเวลาง้างตี

                // สั่ง Animator ให้โจมตี
                _animator.SetInteger("State", 2); //Attack State
                _animator.SetTrigger("Attack");
            }
        }
        
        // สั่งให้หยุดโจมตี
        public void StopAttack()
        {
            if (_isAttacking)
            {
                _isAttacking = false;
                _attackTimer = 0; //หยุดนับเวลาง้างตี
                _animator.SetInteger("State", 1); //กลับไป Idle
            }
        }
        
        // สำหรับ AiController เช็คว่าเป้าหมายอยู่ในระยะโจมตีไหม
        public bool IsTargetInRange(float distance)
        {
            return distance <= attackRange;
        }
        
        // สำหรับ  AiController เช็คว่าพร้อมโจมตีไหม (ไม่ติดง้างตี / ไม่ติด Cooldown)
        public bool IsReadyToAttack()
        {
            return !_isAttacking && _attackCooldownTimer <= 0;
        }
        
        // หันหน้าหาเป้าหมาย
        public void LookAtTarget(Vector3 targetPosition, float angularSpeed)
        {
            var direction = (targetPosition - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * angularSpeed);
        }
        
        // โจมตี
        private void PerformAttack()
        {
            if (_target == null) return;

            var spawnPoint = attackSpawnPoint != null ? attackSpawnPoint : transform;
            var origin = spawnPoint.position;
            var targetPos = _target.position + Vector3.up * 0.5f;
            var dir = (targetPos - origin).normalized;
            
            if (attackType == AttackType.Melee) // Melee
            {
                if (meleeAttackVfxPrefab != null)
                {
                    Instantiate(meleeAttackVfxPrefab, targetPos, Quaternion.identity);
                }

                if (_playerMovement != null)
                {
                    var dmg = (GameManager.Instance != null) ? GameManager.Instance.meleeDamage : 15f;
                    _playerMovement.TakeDamage(dmg);
                }
            }
            else // Range
            {
                if (rangeAttackVfxPrefab != null)
                {
                    Instantiate(rangeAttackVfxPrefab, targetPos, Quaternion.identity);
                }

                if (projectilePrefab != null)
                {
                    GameObject projObject = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
                    AiProjectile projScript = projObject.GetComponent<AiProjectile>();
                    var dmg = (GameManager.Instance != null) ? GameManager.Instance.rangedDamage : 10f;

                    if (projScript != null)
                    {
                        projScript.Initialize(gameObject, dir, projectileSpeed, dmg, projectileLifeTime);
                    }
                    else
                    {
                        Destroy(projObject, projectileLifeTime);
                    }
                }
            }
        }
        
        // รับดาเมจ
        public void TakeDamage(float damageAmount)
        {
            if (currentHealth <= 0) return; // เช็คว่าตายรึยัง

            currentHealth -= damageAmount;

            if (damageTextPrefab != null && textSpawnPoint != null)
            {
                GameObject textInstance = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity);
                var damageTextScript = textInstance.GetComponent<DamageText>();
                if (damageTextScript != null)
                {
                    damageTextScript.SetDamageText(damageAmount);
                }
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        // ตาย
        private void Die()
        {
            Instantiate(loot, transform.position, transform.rotation);

            if (_donutSpawner != null)
            {
                _donutSpawner.OnEntityDeath(gameObject);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Transform point = attackSpawnPoint != null ? attackSpawnPoint : transform;
            Gizmos.DrawWireSphere(point.position, attackRange);
        }
    }
}