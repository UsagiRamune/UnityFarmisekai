using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera instance;
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float smoothSpeed = 10f;
    public float rotationSpeed = 90f; // degrees per second
    public float snapDelay = 2f;       // seconds after no input before snapping
    public float snapSpeed = 2f;
    private float lastInputTime;

    void Start()
    {
        instance = this;
        transform.rotation = Quaternion.Euler(45f, transform.rotation.eulerAngles.y, 0f);
        lastInputTime = Time.time;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Update camera rotation continuously
        float rotateDirection = 0f;
        if (!PlayerMovement.Instance.inventoryOn)
        {
            if (Input.GetKey(KeyCode.Q)) rotateDirection += -1f;
            if (Input.GetKey(KeyCode.E)) rotateDirection += 1f;

        }
      
        if (rotateDirection != 0f)
        {
            // Rotate around the player
            transform.RotateAround(target.position, Vector3.up, rotateDirection * rotationSpeed * Time.deltaTime);
            lastInputTime = Time.time; 
        }
        else
        {
            // If idle for too long → auto snap to nearest 8 direction
            if (Time.time - lastInputTime > snapDelay)
            {
                float currentY = transform.eulerAngles.y;
                float targetY = Mathf.Round(currentY / 45f) * 45f; // nearest multiple of 45
                float newY = Mathf.LerpAngle(currentY, targetY, Time.deltaTime * snapSpeed);

                transform.rotation = Quaternion.Euler(45f, newY, 0f);
            }
        }
        // Always keep a top-down-ish angle
        Vector3 direction = transform.rotation * Vector3.back;
        direction.y = 0;
        direction.Normalize();

        float radius = new Vector2(offset.x, offset.z).magnitude;
        Vector3 desiredPosition = target.position + direction * radius + Vector3.up * offset.y;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
  

}