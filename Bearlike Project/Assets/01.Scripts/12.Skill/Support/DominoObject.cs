using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GamePlay;
using Monster;
using Photon;
using Status;
using UI.Status;
using UnityEngine;

namespace Skill.Support
{
    [RequireComponent(typeof(NetworkTransform))]
    public class DominoObject : NetworkBehaviourEx
    {
        [Networked] public NetworkId SkillId { get; set; }

        [SerializeField] private GameObject damageDominoObject; 
        [SerializeField] private GameObject collisionEffectObject;
        private Animator _animator;
        [SerializeField] private AnimationClip downClip;

        private readonly HashSet<GameObject> _damageMonsterSet = new HashSet<GameObject>(); // 이미 대미지를 입은 대상은 대미지를 다시 입으면 안됨으로 사용
        
        private StatusBase _status;

        private Vector3 _destinationPosition; // 도미노가 닿을 목적지

        private void Awake()
        {
            damageDominoObject.SetActive(false);
            collisionEffectObject.SetActive(false);
            _animator = GetComponent<Animator>();
            _animator.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ColliderStatus cs))
            {
                StatusBase otherStatus = cs.originalStatus;
                // 타격을 입은 몬스터인지 확인
                if (_damageMonsterSet.Contains(otherStatus.gameObject) == false)
                {
                    otherStatus.ApplyDamageRPC(_status.CalDamage(out bool isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, Object.Id);

                    _damageMonsterSet.Add(otherStatus.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var component in GetComponents<Component>())
            {
                Destroy(component);
            }
            
            Destroy(collisionEffectObject, 2f);
            Destroy(damageDominoObject);
        }

        public override void Spawned()
        {
            base.Spawned();
            var skill = Runner.FindObject(SkillId);
            if (skill && 
                skill.TryGetComponent(out _status))
            {
                StartCoroutine(UpdateCoroutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator UpdateCoroutine()
        {
            yield return new WaitForSeconds(downClip.length);
            _animator.enabled = true;

            collisionEffectObject.SetActive(true);
            damageDominoObject.SetActive(true);
            
            Destroy(this);
        }
    }
}