using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Photon;
using Player;
using Status;
using UnityEngine;
using UnityEngine.VFX;

namespace Skill.Support
{
    public class BoostFlag : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerSkillId { get; set; }
        [Networked] public float BoostRange { get; set; } // 장판 범위
        [Networked] public float BoostDuration { get; set; } // 지속시간
        [Networked] public int HealingAmount { get; set; } // 회복양
        [Networked] public float HealingTime { get; set; } // 회복까지 걸린 시간

        [SerializeField] private VisualEffect sphereEffect;
        
        private StatusBase status;

        private TickTimer _healingTimer;
        private List<PlayerController> _playerControllerList = new List<PlayerController>(); // 장판의 효과를 받고 있는 플레이어들

        #region Unity Event Function

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerController.CheckPlayer(other.gameObject, out var pc))
            {
                pc.status.AddAdditionalStatus(status);
                _playerControllerList.Add(pc);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (PlayerController.CheckPlayer(other.gameObject, out var pc))
            {
                pc.status.RemoveAdditionalStatus(status);
                _playerControllerList.Remove(pc);
            }
        }

        private void OnDestroy()
        {
            _healingTimer = TickTimer.None;
            
            foreach (var pc in _playerControllerList)
                pc.status.RemoveAdditionalStatus(status);

            _playerControllerList = null;
        }

        public override void Spawned()
        {
            base.Spawned();
            
            if (TryGetComponent(out SphereCollider sc))
            {
                sc.radius = BoostRange;
            }

            if (HasStateAuthority)
            {
                Destroy(gameObject, BoostDuration);
            }

            if (Runner.TryFindObject(OwnerSkillId, out var ownerSkillObj))
            {
                ownerSkillObj.TryGetComponent(out status);
            }
            _healingTimer = TickTimer.CreateFromSeconds(Runner, HealingTime);
            StartCoroutine(StartEffect());
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (_healingTimer.Expired(Runner))
            {
                _healingTimer = TickTimer.CreateFromSeconds(Runner, HealingTime);
                foreach (var pc in _playerControllerList)
                    pc.status.ApplyHealRPC(HealingAmount, Object.Id);
            }
        }

        #endregion

        #region Member Function

        private IEnumerator StartEffect()
        {
            float endTime = 0.5f;
            sphereEffect.SetFloat("Radius", 0);

            float curTime = 0;
            while (curTime < endTime)
            {
                curTime += Time.deltaTime;
                float value = curTime / endTime;
                sphereEffect.SetFloat("Radius", Mathf.Lerp(0f, BoostRange, value));

                yield return null;
            }
        }

        #endregion
    }
}