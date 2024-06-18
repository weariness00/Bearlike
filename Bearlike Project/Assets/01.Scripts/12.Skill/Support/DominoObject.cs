using System;
using System.Collections.Generic;
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

        private readonly HashSet<GameObject> _damageMonsterSet = new HashSet<GameObject>(); // 이미 대미지를 입은 대상은 대미지를 다시 입으면 안됨으로 사용
        
        private StatusBase _status;

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

        public override void Spawned()
        {
            base.Spawned();
            var skill = Runner.FindObject(SkillId);
            if (skill && 
                skill.TryGetComponent(out _status))
            {
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}