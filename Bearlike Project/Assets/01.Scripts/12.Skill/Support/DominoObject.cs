using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GamePlay;
using Manager;
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
        [SerializeField] private AudioSource wallDownAudio;

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
            DebugManager.Log("충돌 도미노");
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

        public override void Spawned()
        {
            base.Spawned();
            transform.position -= transform.forward * transform.localScale.magnitude * 2f;
            
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
            _animator.enabled = true;
            wallDownAudio.Play();
            yield return new WaitForSeconds(downClip.length * 2f);

            collisionEffectObject.SetActive(true);
            damageDominoObject.SetActive(true);
            
            yield return new WaitForSeconds(2f);
            
            Destroy(gameObject);
        }
    }
}