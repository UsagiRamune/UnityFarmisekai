using UnityEngine;

namespace Custom_Script
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target to Follow")]
        public Transform target; // ตัวละครที่กล้องจะตาม

        [Header("Camera Settings")]
        [Tooltip("ระยะห่างเริ่มต้นระหว่างกล้องกับตัวละคร")]
        public float distance = 5.0f;
        [Tooltip("ความสูงของกล้องเทียบกับตัวละคร (ระดับคอ, ไหล่)")]
        public float heightOffset = 1.5f; // <<<<<<< เพิ่มตัวนี้เข้ามา
        [Tooltip("ความไวของเมาส์")]
        public float mouseSensitivity = 2.0f;
        [Tooltip("ความสมูทในการเคลื่อนที่ตามเป้าหมาย")]
        public float positionSmoothTime = 0.2f;

        [Header("Camera Limits")]
        [Tooltip("มุมก้มต่ำสุด (องศา)")]
        public float pitchMin = -40.0f;
        [Tooltip("มุมเงยสูงสุด (องศา)")]
        public float pitchMax = 80.0f;

        [Header("Zoom Settings")] // <<<<<<< เพิ่มหัวข้อนี้
        [Tooltip("ระยะซูมใกล้สุด")]
        public float zoomMin = 2.0f;
        [Tooltip("ระยะซูมไกลสุด")]
        public float zoomMax = 10.0f;
        [Tooltip("ความเร็วในการซูม")]
        public float zoomSpeed = 20.0f;

        [Header("Auto-Recentering")]
        [Tooltip("หน่วงเวลากี่วินาทีก่อนกล้องจะเริ่มหันกลับตามหลัง")]
        public float recenterDelay = 1.0f;
        [Tooltip("ความเร็วในการหันกล้องกลับ (ยิ่งเยอะยิ่งเร็ว)")]
        public float recenterSpeed = 2.0f;

        // ตัวแปรภายใน ไม่ต้องยุ่ง
        private float _yaw = 0.0f;
        private float _pitch = 0.0f;
        private float _recenterTimer = 0.0f;
        private Vector3 _currentVelocity = Vector3.zero;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (target)
            {
                _yaw = target.eulerAngles.y;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("ยังไม่ได้ใส่ Target ให้กล้องเลยเพื่อน!");
                return;
            }

            // --- Step 1: จัดการระบบซูม ---
            HandleZoom(); // <<<<<<< เพิ่มฟังก์ชันนี้

            // --- Step 2: รับ Input จากเมาส์เพื่อหมุนกล้อง ---
            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            // --- Step 3: จัดการระบบ Auto-Recenter ---
            HandleAutoRecenter();

            // --- Step 4: คำนวณตำแหน่งและมุมกล้อง ---
            // จุดที่กล้องจะมอง (ไม่ใช่ที่เท้า แต่เป็นระดับความสูงที่เราตั้ง)
            Vector3 lookAtPosition = target.position + Vector3.up * heightOffset; // <<<<<<< แก้ตรงนี้

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            Vector3 targetPosition = lookAtPosition - (rotation * Vector3.forward * distance);

            // --- Step 5: อัปเดตตำแหน่งกล้องแบบสมูทๆ ---
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, positionSmoothTime);
            transform.LookAt(lookAtPosition); // <<<<<<< แก้ตรงนี้ให้มองไปที่จุดใหม่
        }

        private void HandleZoom()
        {
            // รับค่าจากลูกกลิ้งเมาส์ (Scroll Wheel)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            // ปรับค่า distance โดยใช้เครื่องหมายลบเพื่อให้การเลื่อนขึ้นเป็นการซูมเข้า
            distance -= scrollInput * zoomSpeed * Time.deltaTime;
            // ล็อกค่า distance ไม่ให้เกินระยะ min/max ที่ตั้งไว้
            distance = Mathf.Clamp(distance, zoomMin, zoomMax);
        }

        private void HandleAutoRecenter()
        {
            bool isMovingForward = Input.GetAxis("Vertical") > 0.1f;
            bool isMouseMovingHorizontally = Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f;

            if (isMovingForward && !isMouseMovingHorizontally)
            {
                _recenterTimer += Time.deltaTime;
                if (_recenterTimer >= recenterDelay)
                {
                    float targetYaw = target.eulerAngles.y;
                    _yaw = Mathf.LerpAngle(_yaw, targetYaw, recenterSpeed * Time.deltaTime);
                }
            }
            else
            {
                _recenterTimer = 0.0f;
            }
        }
    }
}