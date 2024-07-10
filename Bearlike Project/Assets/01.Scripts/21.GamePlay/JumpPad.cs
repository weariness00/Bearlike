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
                var monster = other.GetComponentInParent<MonsterBase>();
                if (monster)
                {
                    var monsterObj = monster.gameObject;
                    var monsterAgent = monsterObj.GetComponent<NavMeshAgent>();

                    StartCoroutine(ParabolicMove(monsterAgent));
                }
            }
        }
        
        IEnumerator ParabolicMove(NavMeshAgent agent)
        {
            var agentObj = agent.gameObject;
            
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
            float duration = (endPos - startPos).magnitude / agent.speed;
            float height = agentObj.transform.position.y + 2.0f; // 포물선의 최고점 높이
            float t = 0.0f;

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
        }
    }
}

