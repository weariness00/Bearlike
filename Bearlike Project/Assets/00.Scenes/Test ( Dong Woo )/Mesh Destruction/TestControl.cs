using Photon;
using UnityEngine;

namespace Test
{
    public class TestControl : NetworkBehaviourEx
    {
        public float speed;
        public float mouseSpeed;
        // Start is called before the first frame update
        void Start()
        {
            var c = Camera.main;
            c.transform.position = transform.position;
            c.transform.rotation = transform.rotation;
            c.transform.SetParent(transform);
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out TsetServer.TestInputData data))
            {
                if (data.Cursor)
                {
                    Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                }
                if (data.MoveRight)
                {
                    transform.position += transform.right * speed * Time.deltaTime;
                }
                else if (data.MoveLeft)
                {
                    transform.position += -transform.right * speed* Time.deltaTime;
                }
                else if (data.MoveBack)
                {
                    transform.position += -transform.forward * speed* Time.deltaTime;
                }
                else if (data.MoveForward)
                {
                    transform.position += transform.forward * speed* Time.deltaTime;
                }
                MouseRotateControl(data.MouseAxis);
            }
        }

        float xRotate, yRotate, xRotateMove, yRotateMove;
        public void MouseRotateControl(Vector2 mouseAxis = default)
        {
            if (mouseAxis == Vector2.zero)
            {
                return;
            }
            
            xRotateMove = mouseAxis.y * Time.deltaTime * mouseSpeed;
            yRotateMove = mouseAxis.x * Time.deltaTime * mouseSpeed;

            yRotate = transform.eulerAngles.y + yRotateMove;
            xRotate += xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
            var angle = new Vector3(-xRotate, yRotate, 0);
            
            transform.eulerAngles = angle;
        }
    }
}
