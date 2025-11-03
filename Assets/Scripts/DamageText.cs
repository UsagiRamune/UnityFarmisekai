using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("Animation Settings")]
    public float moveSpeed = 1.0f;
    public float lifetime = 1.0f;

    private Color startColor;
    private float lifeTimer;
    private Camera mainCamera; // << เพิ่มตัวแปรสำหรับเก็บกล้อง

    void Awake()
    {
        if (textMesh != null)
        {
            startColor = textMesh.color;
        }
        else
        {
            Debug.LogError("TextMeshPro component is not assigned on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        // --- เพิ่มเข้ามา: ค้นหากล้องหลักตั้งแต่เกิด ---
        // วิธีนี้จะหา Object ที่มี Tag "MainCamera"
        mainCamera = Camera.main;
    }

    // --- เพิ่มฟังก์ชัน LateUpdate เข้ามาใหม่ ---
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // นี่คือหัวใจของ Billboarding:
        // สั่งให้ Rotation ของ Text นี้ เหมือนกับ Rotation ของกล้องเป๊ะๆ
        transform.rotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        // ... (โค้ดส่วน Update เหมือนเดิมเป๊ะ ไม่ต้องแก้) ...
        if (textMesh == null) return;

        // 1. ทำให้ลอยขึ้น
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. ทำให้จางหายไป (Fade out)
        lifeTimer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, lifeTimer / lifetime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        // 3. พอครบอายุขัยก็ทำลายตัวเอง
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamageText(float damage)
    {
        // ... (โค้ดส่วนนี้ก็เหมือนเดิมเป๊ะ ไม่ต้องแก้) ...
        if (textMesh != null)
        {
            textMesh.text = damage.ToString("F0");
        }
    }
}