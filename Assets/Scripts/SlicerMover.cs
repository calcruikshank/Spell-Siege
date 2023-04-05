using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlicerMover : MonoBehaviour
{
    public bool is_Touched = false;
    Rigidbody rb;

    int pointerIDTouchingRock;
    float speed = 30f;

    Vector3 startingPosition;
    Quaternion startingRotation;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            is_Touched = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            is_Touched = false;
        }
        if (is_Touched)
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = -10f; //distance of the plane from the camera
            positionOfTouch = Camera.main.ScreenToWorldPoint(screenPoint);
            this.transform.position = new Vector3( -positionOfTouch.x, -positionOfTouch.y + 2, -4f);
        }
    }
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        startingPosition = this.transform.position;
        startingRotation = this.transform.rotation;
    }
    Vector2 directionToMove;
    Vector2 positionOfTouch;
    void FixedUpdate()
    {
        /*if (is_Touched)
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(positionOfTouch);
            directionToMove = new Vector2(worldPosition.x - transform.position.x, worldPosition.y - transform.position.y);

            rb.velocity = directionToMove * speed;
        }
        if (!is_Touched)
        {
            rb.velocity *= .9f;
        }
        rb.angularVelocity *= .9f;*/
    }

}
