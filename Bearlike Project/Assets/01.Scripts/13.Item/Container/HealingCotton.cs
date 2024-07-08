using System;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Item.Container
{
    public class HealingCotton : ItemBase
    {
        private int _healingAmount; // 힐의 양
        private float _healingMultiple = 0f; // 최대 체력에 비례해 얼마만큼 회복 시켜줄 것인지 0~1 값

        public override void Awake()
        {
            base.Awake();

            var statusData = GetStatusData(Id);
            _healingAmount = statusData.GetInt("Healing Amount");
            _healingMultiple = statusData.GetInt("Healing Multiple");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CheckPlayer(other.gameObject, out PlayerController pc))
            {
                if(pc.status.hp.isMax || pc.status.isInjury || pc.status.isRevive) return;

                var colliders = GetComponentsInChildren<Collider>();
                foreach (var c in colliders)
                    Destroy(c);
                
                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }
        
        public override void GetItem(GameObject targetObject)
        {
            PlayerController pc;
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                var maximumProportionalRecoveryHp = (int)(pc.status.hp.Max * _healingMultiple); // 최대 체력 비례 회복량
                pc.status.ApplyHealRPC(_healingAmount + maximumProportionalRecoveryHp, pc.Object.Id);
                
                pc.soundController.PlayItemHeal();
            }
        }
    }
}