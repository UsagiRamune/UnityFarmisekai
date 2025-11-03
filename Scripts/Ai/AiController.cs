using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Ai
{
    [RequireComponent(typeof(NavMeshAgent), typeof(AiPerception))]
    public class AiController : MonoBehaviour
    {
        [Header("Effects & UI")] 
        [SerializeField] private GameObject damageTextPrefab;
        public Animator animator;
        [Tooltip("จุดที่จะให้ Damage Text เกิด (สร้าง Empty Object ไปวางไว้บนหัว)")] 
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

        [Header("Combat Stats")] 
        public AttackType attackType = AttackType.Melee;
        public float attackRange = 1.5f;
        public float cooltimeBeforeAttack = 1.5f;
        public float attackCooldown = 1.5f;
        private float attackCooldownTimer;
        public float currentHealth;
        private float attackTimer;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float attackRadius = 0.8f;
        private List<Collider> hitplayers = new List<Collider>();

        [Header("Projectile ")] [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float projectileLifeTime = 3f;


        [Header("Components")] 
        public AiPerception perception;

        [Header("Stats")] 
        public float patrolSpeed = 2.2f;
        public float chaseSpeed = 4.0f;
        public float stoppingDistance = 1.2f;
        public float lostSightChaseTime = 3f;

        [Tooltip("รัศมีการเดิมสุ่ม")] 
        public float patrolRadius = 20f;


        // โหมดปัจจุบันของ AI
        private AiMode Mode { get; set; } = AiMode.Patrol;
        [Header("Current Mode")] public AiMode currentMode;

        private NavMeshAgent _agent;
        private AiSpawnPoint _homeSpawnPoint;

        // เวลาและตำแหน่งที่เห็นล่าสุด
        private float lastSeenTime = -999f;
        private Vector3 lastSeenPos;
        private float _nextPatrolTime = 0f;

        // โหมดก่อนหน้าที่จะเข้า Chase (สำหรับกลับไปโหมดเดิมหลังหลุดสายตา)
        private AiMode previousMode = AiMode.Patrol;
        private Transform _target;
        private PlayerMovement _playerMovement;

        [Header("Patrol Area (Donut)")] public DonutSpawner donutArea;

        // Log
        public bool isAttacking = false;
        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            perception = GetComponent<AiPerception>();

            attackTimer = cooltimeBeforeAttack;

            if (attackType == AttackType.Range)
            {
                attackRange = 8.0f;
            }
        }

        void Start()
        {
            // ดึงค่าเลือดสูงสุดจาก GameManager มาตั้งเป็นเลือดเริ่มต้น
            if (GameManager.Instance != null)
            {
                currentHealth = GameManager.Instance.enemyMaxHealth;
            }
            else
            {
                // ถ้าหา GameManager ไม่เจอ ให้ใช้ค่าสำรองไปก่อน
                currentHealth = 100f;
                Debug.LogWarning("GameManager not found! Using default health for AI.");
            }
        }
        private void UpdateAnimatorState()
        {
            if (Mode == AiMode.AttackMelee || Mode == AiMode.AttackRange)
            {
                if (!isAttacking&&attackCooldownTimer <=0)
                {
                    isAttacking = true;
                    attackTimer = cooltimeBeforeAttack;
                    animator.SetInteger("State", 2); // Attack
                    animator.SetTrigger("Attack");
                }
                
            }
            if(!isAttacking)
            if (_agent.velocity.sqrMagnitude > 0.1f)
            {
                animator.SetInteger("State", 0); // Moving
            }
            else
            {
                animator.SetInteger("State", 1); // Idle
            }
        }
        public void Initialize(AiSpawnPoint spawnPoint)
        {
            _homeSpawnPoint = spawnPoint;

            // เชื่อม Ai เข้ากับ FlockManager
            /*if (FlockingManager.Instance != null)
            {
                if (donutArea != null)
                {
                    FlockingManager.Instance.AddAgentToFlock(donutArea, this);
                }
                /*FlockingManager.Instance.AddAgentToFlock(_homeSpawnPoint, this);#1#
            }*/
        }

        private void OnDestroy()
        {
            /*if (FlockingManager.Instance != null)
            {
                if (donutArea != null)
                {
                    FlockingManager.Instance.RemoveAgentFromFlock(donutArea, this);
                }
            }*/
        }

        void Update()
        {
            UpdateAnimatorState();
            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -=  Time.deltaTime;
            }
            else
            {
                attackCooldownTimer = 0;
            }
            if (isAttacking)
            {
                if (attackTimer > 0f)
                {
                    attackTimer -= Time.deltaTime;
                }
            }
         

            if (_target == null && perception.target != null)
            {
                _target = perception.target;
                _playerMovement = _target.GetComponent<PlayerMovement>();
            }

            if (_target == null)
            {
                return;
            }

            currentMode = Mode;

            if (EventManager.Instance.IsEventActive() && Mode != AiMode.Rallying &&
                !EventManager.Instance.HasAgentReachedRally(this))
            {
                Mode = AiMode.Rallying;
            }
            else if (EventManager.Instance.HasAgentReachedRally(this))
            {
                Mode = AiMode.Patrol;
            }

            // State Machine
            switch (Mode)
            {
                case AiMode.Patrol:
                    HandlePatrolState();
                    break;
                case AiMode.Chase:
                    HandleChaseState();
                    break;
                // case AiMode.Search:
                //     HandleSearchState();
                //     break;
                case AiMode.Rallying:
                    HandleRallyingState();
                    break;
                case AiMode.AttackMelee:
                    HandleAttackMeleeState();
                    break;
                case AiMode.AttackRange:
                    HandleAttackRangeState();
                    break;
            }

            if (Mode == AiMode.AttackMelee)
            {
                LookAtTarget(_target.position);
                if (attackTimer <= 0)
                {
                    PerformAttack();
                    isAttacking = false;
                    animator.SetInteger("State", 1);
                    attackTimer = cooltimeBeforeAttack;
                    attackCooldownTimer = attackCooldown;
                }
            }

            if (Mode == AiMode.AttackRange)
            {
                LookAtTarget(_target.position);
                if (attackTimer <= 0)
                {
                    LookAtTarget(_target.position);
                  
                    PerformAttack();
                    isAttacking = false;
                    animator.SetInteger("State", 1);
                    attackTimer = cooltimeBeforeAttack;
                    attackCooldownTimer = attackCooldown;
                }
            }
        }

        private void HandlePatrolState()
        {
            _agent.speed = patrolSpeed;
            _agent.stoppingDistance = 0f;

            // การเดินสุ่ม
            if (Time.time > _nextPatrolTime && (!_agent.hasPath || _agent.remainingDistance < 0.5f))
            {
                var patrolPoint = Vector3.zero;
            
                // 1. สุ่มทิศทางและระยะทางจาก *ตำแหน่งปัจจุบัน*
                Vector2 randomCircle = Random.insideUnitCircle * patrolRadius; // ใช้ patrolRadius ใหม่
                Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // 2. หาจุดที่ใกล้ที่สุดบน NavMesh ภายในรัศมีที่กำหนด
                if (NavMesh.SamplePosition(randomPoint, out var hit, patrolRadius, NavMesh.AllAreas))
                {
                    patrolPoint = hit.position;
                    _agent.SetDestination(patrolPoint);
                }
                else
                {
                    // ถ้าหาจุดไม่ได้ (เช่น ตกขอบแมพ หรืออยู่ในที่แคบ) ให้ลองกลับไปที่จุดเกิด
                    if (_homeSpawnPoint != null)
                    {
                        _agent.SetDestination(_homeSpawnPoint.transform.position);
                    }
                }
            
                // 3. รีเซ็ตไทม์เมอร์
                _nextPatrolTime = Time.time + Random.Range(3f, 6f);
            }

            // ถ้ามองเห็น Player ให้ไล่ล่าทันที
            if (perception.CanSeeTarget)
            {
                // บันทึกโหมดก่อนหน้าเพื่อใช้ตอนกลับ (ยกเว้นถ้าอยู่ในโหมด Chase/Search อยู่แล้ว)
                // if (Mode != AiMode.Chase && Mode != AiMode.Search)
                // {
                //     previousMode = Mode;
                // }
                if (Mode != AiMode.Chase)
                {
                    previousMode = Mode;
                }

                lastSeenTime = Time.time;
                lastSeenPos = perception.LastSeenPosition;

                Mode = AiMode.Chase;
            }
        }

        private void HandleChaseState()
        {
            // // อัปเดตเวลาและตำแหน่งที่เห็นล่าสุด
            // lastSeenTime = Time.time;
            // lastSeenPos = perception.LastSeenPosition;

            _agent.speed = chaseSpeed;
            //agent.stoppingDistance = stoppingDistance;

            var distanceToTarget = Vector3.Distance(transform.position, _target.position);

            if (distanceToTarget <= attackRange)
            {
                _agent.isStopped = true;


                Mode = (attackType == AttackType.Melee) ? AiMode.AttackMelee : AiMode.AttackRange;
                return;
            }

            if (perception.CanSeeTarget)
            {
                lastSeenTime = Time.time;
                lastSeenPos = perception.LastSeenPosition;
                _agent.isStopped = false;
                _agent.SetDestination(perception.LastSeenPosition);
            }
            else
            {
                _agent.isStopped = false;
                _agent.SetDestination(lastSeenPos);
                Mode = AiMode.Patrol;

                // ถ้าหลุดสายตานานเกินโควต้า ⇒ เข้า Search
                // if (Time.time - lastSeenTime > lostSightChaseTime)
                // {
                //     Mode = AiMode.Search;
                // }
                // if (Time.time - lastSeenTime > lostSightChaseTime)
                // {
                //     Mode = AiMode.Search;
                // }
                // else
                // {
                //     agent.SetDestination(lastSeenPos);
                // }
            }
        }

        private void HandleAttackMeleeState()
        {
            // เช็คว่าเป้าหมายยังอยู่ในระยะโจมตีหรือไม่
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);

            if (distanceToTarget > attackRange)
            {
                // ถ้าอยู่นอกระยะ ให้กลับไปไล่ล่า
                if (!isAttacking)
                {
                 
                    Mode = AiMode.Chase;
                }
               
            }
        }

        private void HandleAttackRangeState()
        {
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);

            // ถ้าระยะห่างน้อยกว่าที่ควรจะเป็น ให้ถอย
            /*if (distanceToTarget < attackRange - 0.5f)
            {
                agent.isStopped = false;
                Vector3 retreatDirection = (transform.position - _target.position).normalized;
                agent.SetDestination(transform.position + retreatDirection * 1.5f);
            }*/

            // ถ้าระยะห่างมากเกินไป ให้กลับไปไล่ล่า
            if (distanceToTarget > attackRange)
            {
                if (!isAttacking)
                {
                    Mode = AiMode.Chase;
                }
            }
        }

        // private void HandleSearchState()
        // {
        //     // ถ้าหมดเวลาค้นหาแล้ว ให้กลับไปโหมดก่อนหน้า
        //     if (Time.time - lastSeenTime > lostSightChaseTime)
        //     {
        //         // กลับไปโหมดก่อนหน้า หรือ Rallying ถ้า Event ยังทำงานอยู่และยังไม่ได้ไปถึง rally point
        //         if (EventManager.Instance.IsEventActive() && !EventManager.Instance.HasAgentReachedRally(this))
        //         {
        //             Mode = AiMode.Flocking;
        //         }
        //         else
        //         {
        //             Mode = previousMode;
        //         }
        //         return;
        //     }

        //     // ยังไม่หมดเวลา ให้ไปตำแหน่งล่าสุดที่เห็น
        //     agent.speed = chaseSpeed;
        //     //agent.stoppingDistance = 0f;
        //     agent.SetDestination(lastSeenPos);
        // }


        private void HandleRallyingState()
        {
            _agent.speed = chaseSpeed;
            _agent.stoppingDistance = 1f;

            // ไปยังจุด rally point ที่กำหนดใน EventManager
            Transform rallyPoint = EventManager.Instance.GetRallyPoint();
            if (rallyPoint != null)
            {
                _agent.SetDestination(rallyPoint.position);

                // เมื่อถึงจุดรวมพลแล้ว
                if (!_agent.pathPending && _agent.remainingDistance <= 1.5f)
                {
                    // แจ้ง EventManager ว่าตัวนี้ไปถึงแล้ว
                    EventManager.Instance.OnAgentReachedRallyPoint(this);

                    // หยุดการรวมพลและกลับไป Patrol ทันที
                    Mode = AiMode.Patrol;
                    Debug.Log($"Agent reached rally point, returning to patrol");
                }
            }
            else
            {
                // ถ้าไม่มี rally point ให้กลับไป spawn point
                _agent.SetDestination(_homeSpawnPoint.transform.position);

                if (!_agent.pathPending && _agent.remainingDistance <= 1.5f)
                {
                    // แจ้ง EventManager ว่าตัวนี้ไปถึงแล้ว
                    EventManager.Instance.OnAgentReachedRallyPoint(this);

                    Mode = AiMode.Patrol;
                    Debug.Log($"Agent reached spawn point, returning to patrol");
                }
            }
        }


        /*private void HandleFlockingState()
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 1f;

            Transform rallyPoint = EventManager.Instance.GetRallyPoint();
            if (rallyPoint != null && EventManager.Instance.IsEventActive())
            {
                // เมื่อถึงจุด Rally Point แล้ว
                if (!agent.pathPending && Vector3.Distance(transform.position, rallyPoint.position) <= 1.5f)
                {
                    EventManager.Instance.OnAgentReachedRallyPoint(this);
                    Mode = AiMode.Patrol;
                    Debug.Log($"Agent reached rally point.");
                }
                else
                {
                    // คำนวณทิศทาง Flocking และรวมกับแรงเข้าหา Rally Point
                    Vector3 flockDirection = FlockingManager.Instance.GetFlockDirection(donutArea, this);

                    // ใช้ NavMeshAgent เพื่อเคลื่อนที่ไปในทิศทางนั้น
                    if (flockDirection.sqrMagnitude > 0.01f)
                    {
                        agent.SetDestination(transform.position +
                                             flockDirection.normalized *
                                             5f); // 5f เป็นค่าที่ใช้ในการคำนวณจุดหมายปลายทางชั่วคราว
                    }
                }
            }
            else
            {
                Mode = AiMode.Patrol;
                animator.SetInteger("State", 0);
            }
        }*/

        public void PerformAttack()
        {
            if (_target == null)
                return;

        //    Debug.Log($"{gameObject.name} is attacking {_target.name}!");

            Transform spawnPoint = attackSpawnPoint != null ? attackSpawnPoint : transform;
            Vector3 origin = spawnPoint.position; // slightly above AI's base

            // Target position (center of player)
            Vector3 targetPos = _target.position + Vector3.up * 0.5f;
            Vector3 dir = (targetPos - origin).normalized;
            float distanceToPlayer = Vector3.Distance(origin, targetPos);

            if (attackType == AttackType.Melee)
            {
                if (distanceToPlayer <= attackRange)
                {
                    // Play melee VFX
                    if (meleeAttackVfxPrefab != null)
                        Instantiate(meleeAttackVfxPrefab, targetPos, Quaternion.identity);

                    // Directly deal damage
                    PlayerMovement player = _target.GetComponent<PlayerMovement>();
                    if (player != null)
                    {
                        float dmg = (GameManager.Instance != null) ? GameManager.Instance.meleeDamage : 15f;
                        player.TakeDamage(dmg);
                      //  Debug.Log($"[{name}] melee hit {player.name} for {dmg}");
                    }
                }
            }
            else // Range
            {
                // Play range VFX
                if (rangeAttackVfxPrefab != null)
                    Instantiate(rangeAttackVfxPrefab, origin, Quaternion.identity);

                if (projectilePrefab != null)
                {
                    GameObject projObj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
                    AiProjectile projScript = projObj.GetComponent<AiProjectile>();

                    float dmg = (GameManager.Instance != null) ? GameManager.Instance.rangedDamage : 10f;

                    if (projScript != null)
                        projScript.Initialize(gameObject, dir, projectileSpeed, dmg, projectileLifeTime);
                    else
                    {
                        Rigidbody rb = projObj.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.linearVelocity = dir * projectileSpeed;
                            Destroy(projObj, projectileLifeTime);
                        }
                        else
                        {
                            Destroy(projObj, projectileLifeTime);
                        }
                    }
                }
                else
                {
                    // fallback: directly damage if no projectile prefab
                    if (distanceToPlayer <= attackRange)
                    {
                        PlayerMovement player = _target.GetComponent<PlayerMovement>();
                        if (player != null)
                        {
                            float dmg = (GameManager.Instance != null) ? GameManager.Instance.rangedDamage : 10f;
                            player.TakeDamage(dmg);
                            Debug.Log($"[{name}] ranged hit {player.name} for {dmg}");
                        }
                    }
                }
            }
        }

        private void LookAtTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _agent.angularSpeed);
        }

        public void TakeDamage(float damageAmount)
        {
            currentHealth -= damageAmount;
            // Debug.Log($"{gameObject.name} took {damageAmount} damage, remaining health: {currentHealth}"); // << ปิดอันเก่าไปเลย หรือจะลบก็ได้

            // --- ส่วนที่เพิ่มเข้ามา: สร้าง Damage Text ---
            if (damageTextPrefab != null && textSpawnPoint != null)
            {
                // 1. สร้าง Prefab ขึ้นมาในโลก
                GameObject textInstance = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity);

                // 2. ดึงสคริปต์ DamageText ออกมา
                var damageTextScript = textInstance.GetComponent<DamageText>();
                if (damageTextScript != null)
                {
                    // 3. สั่งให้มันแสดงผลดาเมจ
                    damageTextScript.SetDamageText(damageAmount);
                }
            }
            
            // หันเมื่อโดนตี
            bool isNotInCombat = (Mode != AiMode.Chase && Mode != AiMode.AttackMelee && Mode != AiMode.AttackRange);
            
            if (isNotInCombat && _target != null)
            {
                Mode = AiMode.Chase;
                lastSeenTime = Time.time;
                lastSeenPos = _target.position; 
            
                // --- 3. หยุดเส้นทาง Patrol เก่าทันที (สำคัญมาก!) ---
                _agent.isStopped = true; // หยุดตัวก่อน
                _agent.ResetPath();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
       //     Debug.Log($"{gameObject.name} has died.");

            Instantiate(loot, transform.position, transform.rotation);
            DonutSpawner.instance.OnEntityDeath(gameObject);
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 10);
            if (attackSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(attackSpawnPoint.position, attackRadius);
                Gizmos.DrawWireSphere(attackSpawnPoint.position + transform.forward * attackRange, attackRadius);
                Gizmos.DrawLine(attackSpawnPoint.position + transform.right * attackRadius,
                    attackSpawnPoint.position + transform.forward * attackRange + transform.right * attackRadius);
                Gizmos.DrawLine(attackSpawnPoint.position - transform.right * attackRadius,
                    attackSpawnPoint.position + transform.forward * attackRange - transform.right * attackRadius);
            }
        }
    }
}