using System;
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
            if (other.CompareTag("Player"))
            {
                PlayerController pc;
                if (other.TryGetComponent(out pc) || other.transform.parent.TryGetComponent(out pc))
                {
                    pc.simpleKcc.Move(default, jumpPower * 100f * Vector3.up);
                }
            }
            else
            {
                var monster = other.GetComponentInParent<MonsterBase>();
                if (monster)
                {
                    Vector3 dir = Vector3.zero;
                    if (monster.navMeshAgent)
                    {
                        dir = monster.navMeshAgent.velocity.normalized;
                        monster.DisableNavMeshAgent();
                        monster.EnableNavMeshAgent(0.5f);
                    }
                    else if(monster.rigidbody)
                    {
                        dir = monster.rigidbody.velocity;
                        dir.y = 0;
                        dir = dir.normalized;
                    }

                    var force = jumpPower * 100f * monster.rigidbody.mass * Vector3.up  + dir * jumpDirectionPower;
                    monster.rigidbody.AddForce(force);
                    return;
                }
            }
        }
    }
}

