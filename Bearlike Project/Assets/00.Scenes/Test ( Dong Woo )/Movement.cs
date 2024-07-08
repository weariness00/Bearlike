using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 500.0f;

    // Start is called before the first frame update
    void Start()
    {
        var cameraObj = Camera.main.gameObject;
        cameraObj.transform.SetParent(transform);
        cameraObj.transform.position = transform.position;
        cameraObj.transform.rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Middle))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        if (Cursor.lockState == CursorLockMode.None)
            return;
        
        Move();
        float axisX = Input.GetAxis("Mouse X");
        float axisY = Input.GetAxis("Mouse Y");
        MouseRotateControl(new Vector2(axisX, axisY));
    }

    public void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * Time.deltaTime * moveSpeed;
        }
    }
    
    float xRotate, yRotate, xRotateMove, yRotateMove;
    public void MouseRotateControl(Vector2 mouseAxis = default)
    {
        xRotateMove = mouseAxis.y * Time.deltaTime * rotateSpeed;
        yRotateMove = mouseAxis.x * Time.deltaTime * rotateSpeed;

        yRotate += yRotateMove;
        xRotate += xRotateMove;

        xRotate = Mathf.Clamp(xRotate, -45, 45); // 위, 아래 제한 
        transform.rotation = Quaternion.Euler(new Vector3(-xRotate, yRotate, 0));
    }
}
