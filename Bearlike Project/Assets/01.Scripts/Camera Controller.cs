using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;  // 플레이어 이동 속도
    public float turnSpeed = 720f;  // 플레이어 회전 속도 (각도/초)
    
    void Update()
    {
        Move();
        Turn();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    void Turn()
    {
        float turn = Input.GetAxis("Horizontal");
        if (turn != 0)
        {
            Quaternion turnRotation = Quaternion.Euler(0f, turn * turnSpeed * Time.deltaTime, 0f);
            transform.rotation = transform.rotation * turnRotation;
        }
    }
}
