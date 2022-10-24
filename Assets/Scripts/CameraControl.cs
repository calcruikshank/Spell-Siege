using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] float minYForCamera = 8f;
    [SerializeField] float maxYForCamera = 20f;

    public float speed =2000;
    float mouseSensitivity = 3.0f;
    private Vector3 lastPosition;

    bool returningHome;
    Vector3 targetPosition;

    public static CameraControl Singleton;
    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(this);
        }
        Singleton = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (returningHome)
        {
            float targetY;
            if (this.transform.position.y < 16)
            {
                targetY = 16;
            }
            else
            {
                targetY = this.transform.position.y;
            }
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(targetPosition.x, targetY, targetPosition.z), 200f * Time.deltaTime);
            if (Vector3.Distance(this.transform.position, new Vector3(targetPosition.x, targetY, targetPosition.z)) < .02f)
            {
                returningHome = false;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            MoveCamera(Input.GetAxis("Mouse ScrollWheel"));
        }
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            lastPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {

            Vector3 delta = Input.mousePosition - lastPosition;
            transform.Translate(-delta.x * mouseSensitivity * Time.fixedDeltaTime, -delta.y * mouseSensitivity * Time.fixedDeltaTime, 0) ;
            lastPosition = Input.mousePosition;
        }
        if (transform.position.y < minYForCamera)
        {
            this.transform.position = new Vector3(transform.position.x, minYForCamera, transform.position.z);
        }
        if (transform.position.y > maxYForCamera)
        {
            this.transform.position = new Vector3(transform.position.x, maxYForCamera, transform.position.z);
        }
    }
    void MoveCamera(float y)
    {
        if (transform.position.y > maxYForCamera && y < 0) return;
        if (transform.position.y < minYForCamera && y > 0) return;
        Vector3 movementAmount = new Vector3(transform.position.x, transform.position.y - (y * speed * Time.fixedDeltaTime), transform.position.z);
        if (movementAmount.y > maxYForCamera)
        {
            movementAmount = new Vector3(transform.position.x, maxYForCamera, transform.position.z);
        }
        if (movementAmount.y < minYForCamera)
        {
            movementAmount = new Vector3(transform.position.x, minYForCamera, transform.position.z);
        }
        transform.position = Vector3.MoveTowards(this.transform.position, movementAmount, 200 * Time.fixedDeltaTime);
    }
    public void ReturnHome(Vector3 returnHomePosition)
    {
        targetPosition = new Vector3( returnHomePosition.x, this.transform.position.y, returnHomePosition.z );
        returningHome = true;
    }
    public void CancelReturnHome()
    {
        returningHome = false;
    }
}
