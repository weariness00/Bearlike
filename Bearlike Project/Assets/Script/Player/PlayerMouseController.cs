using System;
using Fusion;
using UnityEngine;

namespace Script.Player
{
    public class PlayerMouseController : NetworkBehaviour
    {
        public float rotateSpeed = 500.0f;
        float xRotate, yRotate, xRotateMove, yRotateMove;

        public override void FixedUpdateNetwork()
        {
            MouseRotate();
        }

        public void MouseRotate()
        {
            xRotateMove = -Input.GetAxis("Mouse Y") * Time.deltaTime * rotateSpeed;
            yRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed;

            yRotate = transform.eulerAngles.y + yRotateMove;
            xRotate = xRotate + xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
            var angle = new Vector3(xRotate, yRotate, 0);
            
            transform.eulerAngles = angle;
        }
    }
}

