using System;
using System.Collections;
using DG.Tweening;
using Manager;
using Monster;
using Photon;
using Player;
using UnityEngine;
using UnityEngine.AI;

namespace GamePlay
{
    [RequireComponent(typeof(Rigidbody))]
    public class JumpPad : NetworkBehaviourEx
    {
        public float jumpPower = 1f; // 점프에 얼마만큼에 힘을 줄지
        public float jumpDirectionPower = 1f; // 점프 전에 움직이고 있다면 해당 방향으로는 얼마만큼에 힘을 줄지

        private bool _isServer;

        private void Awake()
        {
            if(TryGetComponent(out Collider c))
                c.isTrigger = true;

            if (TryGetComponent(out Rigidbody rb))
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                PlayerController pc;
                
                if (other.TryGetComponent(out pc) || other.transform.parent.TryGetComponent(out pc))
                {
                    pc.simpleKcc.Move(Vector3.zero, jumpPower * 10f * Vector3.up);
                }
            }
            else
            {
                // var monster = other.GetComponentInParent<MonsterBase>();
                // if (monster)
                // {
                //     Vector3 dir = Vector3.zero;
                //     if (monster.navMeshAgent)
                //     {
                //         dir = monster.navMeshAgent.velocity.normalized;
                //         monster.DisableNavMeshAgent();
                //         monster.EnableNavMeshAgent();
                //     }
                //     else if(monster.rigidbody)
                //     {
                //         dir = monster.rigidbody.velocity;
                //         dir.y = 0;
                //         dir = dir.normalized;
                //     }
                //
                //     var force = jumpPower * 50f * monster.rigidbody.mass * Vector3.up  + dir * 100f * jumpDirectionPower;
                //     monster.rigidbody.AddForce(force);
                //     return;
                // }
            }
        }
    }
}

