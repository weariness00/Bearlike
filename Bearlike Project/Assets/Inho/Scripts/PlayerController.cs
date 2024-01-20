using System.Collections;
using System.Collections.Generic;
using Inho.Scripts.State.StateClass.Pure;
using Inho.Scripts.State.StateSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 dir; 
    CharacterController cc;
    
    public float speed;   
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // if (cc.isGrounded)
        {         
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            dir = new Vector3(h, 0, v) * speed;

            if (dir != Vector3.zero)
                // 진행 방향으로 캐릭터 회전
                transform.rotation = Quaternion.Euler(0, Mathf.Atan2(h, v) * Mathf.Rad2Deg, 0);

            // Space 바 누르면 점프
            if (Input.GetKeyDown(KeyCode.Space))
                dir.y = 7.5f;
        }

        // dir.y += Physics.gravity.y * Time.deltaTime;
        cc.Move(dir * Time.deltaTime);
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        // StateSystemScene
        if (collision.GetComponent<Collider>().gameObject.CompareTag("Buff"))
        {
            if (collision.gameObject.name == "PoisonSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().GetState();
                state.AddCondition((int)eCondition.Poisoned);
            }            
            else if (collision.gameObject.name == "WeakSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().GetState();
                state.AddCondition((int)eCondition.Weak);
            }
            else if (collision.gameObject.name == "AntidoteSphere")
            {
                var state = transform.parent.gameObject.GetComponent<StateSystem>().GetState();
                state.DelCondition((int)eCondition.Poisoned);
                state.DelCondition((int)eCondition.Weak);
            }
        }
        // StateSystemScene
        
        
    }
}