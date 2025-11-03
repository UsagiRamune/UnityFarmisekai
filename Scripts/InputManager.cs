using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Camera sceneCamera;
    private Vector3 lastPosition;
    [SerializeField] private LayerMask placementLayer;
    public event Action OnClick, OnExit ;
    public void ClearEvents()
    {
        OnClick = null;
        OnExit = null;
    }

    public Vector3 GetSelectedMapPos()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, placementLayer))
        {
            lastPosition = hit.point;
        }
        else
        {
            // Raycast missed, project point to default Y=0 plane
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                lastPosition = ray.GetPoint(enter);
            }
        }

        return lastPosition;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnClick?.Invoke();

    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();
}