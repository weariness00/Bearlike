using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Photon;
using Status;
using UI.Status;
using Unity.VisualScripting;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAttackObject : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        public MonsterStatus status;

        public float damage;
        
        private HashSet<GameObject> damagePlayerSet = new HashSet<GameObject>(); // 이미 대미지를 입은 플레이어인지
        private Dictionary<GameObject, Coroutine> damageCoroutines = new Dictionary<GameObject, Coroutine>();
        
        private void Awake()
        {
            status = gameObject.GetOrAddComponent<MonsterStatus>();

            status.damage.Max = (int)(damage);
            status.damage.Current = (int)(damage);
        }
        
        public override void Spawned()
        {
            // 지속 데미지임
            Destroy(gameObject, 2.0f);
        }

        IEnumerator ContinuousDamageCoroutine(StatusBase otherStatus)
        {
            while (true)
            {
                status.AddAdditionalStatus(otherStatus);
                        
                // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                otherStatus.ApplyDamageRPC(1, DamageTextType.Normal, OwnerId);
                        
                status.RemoveAdditionalStatus(otherStatus);
                
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (true == other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    if (damagePlayerSet.Contains(otherStatus.gameObject) == false)
                    {
                        Coroutine coroutine = StartCoroutine(ContinuousDamageCoroutine(otherStatus));
                        damageCoroutines[otherStatus.gameObject] = coroutine;
                        
                        damagePlayerSet.Add(otherStatus.gameObject);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            StatusBase otherStatus = null;
            
            if (true == other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    if (damagePlayerSet.Contains(otherStatus.gameObject) == true)
                    {
                        if (damageCoroutines.TryGetValue(otherStatus.gameObject, out Coroutine coroutine))
                        {
                            StopCoroutine(coroutine);
                            damageCoroutines.Remove(otherStatus.gameObject);
                        }
                        
                        damagePlayerSet.Remove(otherStatus.gameObject);
                    }
                }
            }
        }
    }
}