using State;
using State.StateClass.Base;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    private Vector3 _dir; 
    private CharacterController _cc;
    
    public float speed;   
    
    void Start()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // if (cc.isGrounded)
        {         
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            _dir = new Vector3(h, 0, v) * speed;

            if (_dir != Vector3.zero)
                // 진행 방향으로 캐릭터 회전
                transform.rotation = Quaternion.Euler(0, Mathf.Atan2(h, v) * Mathf.Rad2Deg, 0);

            // Space 바 누르면 점프
            if (Input.GetKeyDown(KeyCode.Space))
                _dir.y = 7.5f;
        }

        // dir.y += Physics.gravity.y * Time.deltaTime;
        _cc.Move(_dir * Time.deltaTime);
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        // StateSystemScene
        if (collision.GetComponent<Collider>().gameObject.CompareTag("Buff"))
        {
            if (collision.gameObject.name == "PoisonSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().State;
                state.AddCondition(ObjectProperty.Poisoned);
            }            
            else if (collision.gameObject.name == "WeakSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().State;
                state.AddCondition(ObjectProperty.Weak);
            }
            else if (collision.gameObject.name == "AntidoteSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().State;
                state.DelCondition(ObjectProperty.Poisoned);
                state.DelCondition(ObjectProperty.Weak);
            }
        }
        // StateSystemScene
        
        
    }
}
