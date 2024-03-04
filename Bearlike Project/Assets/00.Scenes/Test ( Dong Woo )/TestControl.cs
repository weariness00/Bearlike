using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class TestControl : MonoBehaviour
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

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                transform.position += -transform.right * speed* Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position += -transform.forward * speed* Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * speed* Time.deltaTime;
            }

            MouseRotateControl(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
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
