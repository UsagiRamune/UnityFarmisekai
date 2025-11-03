using UnityEngine;

namespace Ai
{
    // การทำงาน
    // สคริปต์นี้มีหน้าที่เช็คว่า AI มองเห็นเป้าหมายหรือไม่
    // เงื่อนไข: อยู่ในระยะ, อยู่ในมุมมอง, และไม่มีสิ่งกีดขวางสายตา (Raycastไม่ชนสิ่งกีดขวาง)
    // ใช้ร่วมกับ AIController เพื่อสั่งไล่ตามเมื่อเห็น
    public class AiPerception : MonoBehaviour
    {
        // Tag ของเป้าหมาย (เช่น "Player") > เผื่อสร้าง Tag ใหม่ๆแล้วไม่อยากลากวางก็เขียนข้างล่างนี่
        public string targetTag = "Player";

        // ถ้าเซ็ตไว้จะใช้เป้านี้ทันที (เช่น ลาก Player มาวาง)
        public Transform target;

        // รัศมีการมองเห็น
        public float viewRadius = 15f;

        // องศามุมมองรวม 120 = ซ้าย 60 ขวา 60
        [Range(0f, 360f)]
        public float viewAngle = 120f;

        // Layer ที่ถือว่าเป็นสิ่งกีดขวางสายตา (เช่น กำแพง, พื้น, เสา)
        public LayerMask obstacleMask;

        // ความสูงจุดยิงเรย์ (เช่นระดับตา) จากตำแหน่งฐานของ Ai
        public float eyeHeight = 1.6f;

        // สถานะ ตอนนี้เห็นเป้าหมายไหม
        public bool CanSeeTarget { get; private set; }

        // ตำแหน่งล่าสุดที่เคยเห็น (ใช้กรณีหลุดสายตาแล้วยังจะตามต่อ)
        public Vector3 LastSeenPosition { get; private set; }

        void Start()
        {

            if (target == null)
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) target = go.transform;
            }
        }

        void Update()
        {
            // รีเซ็ตสถานะทุกเฟรม
            CanSeeTarget = false;

            // ถ้าไม่มีเป้าหมาย ก็ออกเลย
            if (!target) return;

            // Vector จาก AI ไปหาเป้าหมาย + ระยะ
            Vector3 toTarget = target.position - transform.position;
            float dist = toTarget.magnitude;

            // 1) เช็คระยะก่อน (ขอเช็คระยะหน่อย!!!)
            if (dist > viewRadius) return;

            // 2) เช็คมุมมอง (เทียบกับทิศที่ AI หันหน้าอยู่)
            float halfFov = viewAngle * 0.5f;
            float angle = Vector3.Angle(transform.forward, toTarget);
            if (angle > halfFov) return;

            // 3) ยิง Raycast จากระดับตาไปหาเป้า เพื่อตรวจว่ามีสิ่งบังหรือไม่ (Raycast ไว้สำหรับกัน Ai มองทะลุสิ่งกีดขวาง)
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 dir = toTarget.normalized;

            // ถ้า Ray(Raycast) ชนอะไรใน obstacleMask ก่อนถึงเป้าหมาย = มีสิ่งบัง
            bool blocked = Physics.Raycast(eye, dir, dist, obstacleMask);
            if (blocked) return;

            // ผ่านครบทุกเงื่อนไข > เห็น
            CanSeeTarget = true;
            LastSeenPosition = target.position;
        }

        // เอาไว้ Debug จะได้มองเห็นระยะของ Ai ในหน้า Scene
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, viewRadius);

            Gizmos.color = Color.green;
            Vector3 left = DirFromAngle(-viewAngle * 0.5f);
            Vector3 right = DirFromAngle(+viewAngle * 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
            Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
        }

        // ใช้คู่กับ Gizmos เพื่อวาดเส้นกรวยให้มองง่ายในหน้า Scene //Debug
        Vector3 DirFromAngle(float angleDeg)
        {
            float rad = (transform.eulerAngles.y + angleDeg) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        }
    }
}