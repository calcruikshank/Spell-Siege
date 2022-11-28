using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePositionScript : MonoBehaviour
{
    Camera mainCamera;
    Vector3 mousePositionWorldPoint;
    [SerializeField] public LayerMask baseTileMap;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, baseTileMap))
        {
            mousePositionWorldPoint = raycastHit.point;
        }
    }

    public Vector3 GetMousePositionWorldPoint()
    {
        return mousePositionWorldPoint;
    }
}
