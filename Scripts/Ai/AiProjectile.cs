using UnityEngine;
using System.Collections;

public class AiProjectile : MonoBehaviour
{
    Rigidbody _rb;
    GameObject _owner;
    float _damage;
    float _lifeTime = 3f;
    bool _initialized = false;

    int _enemyLayer;
    int _projectileLayer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _projectileLayer = LayerMask.NameToLayer("EnemyProjectile");
        if (_rb != null)
        {
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            Debug.LogWarning($"{name} missing Rigidbody (AiProjectile).");
        }
    }

    /// <summary>
    /// เรียกเพื่อกำหนดเจ้าของ, ทิศทาง, ความเร็ว, ดาเมจ และอายุของกระสุน
    /// </summary>
    public void Initialize(GameObject owner, Vector3 direction, float speed, float damage, float lifeTime = 3f)
    {
        _owner = owner;
        _damage = damage;
        _lifeTime = lifeTime;
        _initialized = true;

        transform.rotation = Quaternion.LookRotation(direction);

        if (_rb != null)
        {
            _rb.linearVelocity = direction.normalized * speed;
        }
        else
        {
            // fallback: ถ้าไม่มี Rigidbody ให้เคลื่อนที่ด้วย Transform
            StartCoroutine(MoveByTransform(direction.normalized * speed));
        }

        // กำหนดลบตัวเองหลังหมดอายุ
        StartCoroutine(AutoDestroy());
    }

    IEnumerator MoveByTransform(Vector3 velocity)
    {
        while (true)
        {
            transform.position += velocity * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(_lifeTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_initialized) { Destroy(gameObject); return; }

        // เมินเจ้าของ
        if (_owner != null && other.transform.root == _owner.transform) return;

        // เมินศัตรูฝั่งเดียวกัน (ให้ทะลุ)
        if (other.gameObject.layer == _enemyLayer) return;
        if (other.gameObject.layer == _projectileLayer) return;
        // ถ้าโดน Player -> ดาเมจ แล้วลบกระสุน
        var player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        // ถ้าโดนอย่างอื่น (กำแพง/พื้น/วัตถุทั่วไป) -> ลบทันที
        Destroy(gameObject);
    }
    void FixedUpdate()
    {
        
        if (_rb != null)
        {
            _rb.AddForce(Vector3.down * 9.81f * _rb.mass * 0.1f, ForceMode.Force);
            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                // Rotate to face the movement direction
                transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
            }
        }
    }
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (!_initialized)
    //     {
    //         // ถ้ายังไม่ถูก Initialize ให้ทำลายตัวเองเพื่อไม่ค้าง
    //         Destroy(gameObject);
    //         return;
    //     }

    //     var other = collision.gameObject;

    //     // อย่าให้โดนเจ้าของตัวเอง
    //     if (other == _owner) return;

    //     // ถ้าโดน Player (มีสคริปต์ PlayerMovement) -> ทำดาเมจ
    //     var player = other.GetComponent<PlayerMovement>();
    //     if (player != null)
    //     {
    //         player.TakeDamage(_damage);
    //         // VFX หรือ SFX ตรงนี้
    //     }
    //     else
    //     {
    //         // ถ้าโดนสิ่งอื่น ก็สามารถเช็ค tag/layer เพิ่มได้
    //     }

    //     // เมื่อชนอะไรแล้ว หายทันที
    //     Destroy(gameObject);
    // }
}