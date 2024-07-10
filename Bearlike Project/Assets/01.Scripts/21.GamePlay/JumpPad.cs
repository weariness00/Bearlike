using System;
using System.Collections;
using DG.Tweening;
using Manager;
using Monster;
using Photon;
using Player;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace GamePlay
{
    [RequireComponent(typeof(Rigidbody))]
    public class JumpPad : NetworkBehaviourEx
    {
        private NavMeshLink _navMeshLink;
        
        public float jumpPower = 1f; // 점프에 얼마만큼에 힘을 줄지
        public float jumpDirectionPower = 1f; // 점프 전에 움직이고 있다면 해당 방향으로는 얼마만큼에 힘을 줄지

        private bool _isServer;

        private void Awake()
        {
            _navMeshLink = GetComponent<NavMeshLink>();
            
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
                var monster = other.GetComponentInParent<MonsterBase>();
                if (monster)
                {
                    var monsterObj = monster.gameObject;
                    var monsterAgent = monsterObj.GetComponent<NavMeshAgent>();

                    if(monsterAgent != null)    // navmeshagent로 움직임을 관리하는 몬스터일경우
                        StartCoroutine(ParabolicMove(monsterAgent));
                    else
                    {
                        // 아닐 경우 => 주사위
                        
                    }
                }
            }
        }
        
        IEnumerator ParabolicMove(NavMeshAgent agent)
        {
            var agentPosition = agent.transform.position;

            Vector3 startPos = agentPosition;
            Vector3 endPos = agentPosition + _navMeshLink.endPoint + Vector3.up * (agent.baseOffset + 1);
            
            float duration = (endPos - startPos).magnitude / agent.speed;
            float height = 10.0f; // 포물선의 최고점 높이
            float t = 0.0f; 

            agent.updatePosition = false;
            
            while (t < 1.0f)
            {
                t += Time.deltaTime / duration;
                float parabolicT = t * 2 - 1;
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
                currentPos.y += height * (1 - parabolicT * parabolicT);
                agent.transform.position = currentPos;
                yield return null;
            }

            agent.CompleteOffMeshLink();
            agent.updatePosition = true;
        }
    }
}

