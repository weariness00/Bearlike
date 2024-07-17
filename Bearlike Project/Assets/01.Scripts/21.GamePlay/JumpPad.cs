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
                if (monster != null)
                {
                    var monsterObj = monster.gameObject;
                    var monsterAgent = monsterObj.GetComponent<NavMeshAgent>();

                    if(monsterAgent == null)
                    {
                        // 아닐 경우 => 주사위
                        StartCoroutine(DiceJumpCoroutine(monsterObj));
                    }
                }
            }
        }

        IEnumerator DiceJumpCoroutine(GameObject monsterObj)
        {
            Vector3 startPos = monsterObj.transform.position;
            Vector3 endPos = transform.position + _navMeshLink.endPoint;
            
            float duration = (endPos - startPos).magnitude / 5;
            float height = 10f;
            
            float normalizedTime = 0.0f;
            while (normalizedTime < 1.0f)
            {
                float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
                monsterObj.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
                normalizedTime += Time.deltaTime / duration; 
                yield return null;
            }
        }
    }
}

