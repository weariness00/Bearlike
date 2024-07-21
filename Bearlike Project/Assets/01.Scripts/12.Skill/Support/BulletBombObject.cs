using System.Collections.Generic;
using Fusion;
using GamePlay;
using Photon;
using Skill.Container;
using Status;
using UI.Status;
using UnityEngine;
using Weapon.Bullet;

namespace Skill.Support
{
    [RequireComponent(typeof(StatusBase))]
    public class BulletBombObject : NetworkBehaviourEx
    {
        // Bullet Status의 Gun Status를 추가하기때문에 Gun Status를 Bomb에 추가해준다. 왜냐하면 Bullet은 Damage를 0으로 만들기 떄문이다.
        [Networked] public NetworkId BulletIsBombId { get; set; } // 스킬의 ID
        [Networked] public NetworkId BulletOwnerId { get; set; }
        [SerializeField] private GameObject explodeEffect;
        private NetworkId _playerID;
        private StatusBase status;

        private HashSet<GameObject> damageMonsterSet = new HashSet<GameObject>(); // 이미 대미지를 입은 몬스터인지

        #region Unity Evnet Function

        private void Awake()
        {
            status = GetComponent<StatusBase>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ColliderStatus cs))
            {
                StatusBase otherStatus = cs.originalStatus;
                // 타격을 입은 몬스터인지 확인
                if (damageMonsterSet.Contains(otherStatus.gameObject) == false)
                {
                    status.AddAdditionalStatus(cs.status);
                    otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, _playerID);
                    status.RemoveAdditionalStatus(cs.status);

                    damageMonsterSet.Add(otherStatus.gameObject);
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            var bulletObject = Runner.FindObject(BulletOwnerId);
            if (bulletObject)   
            {
                if (bulletObject.TryGetComponent(out BulletBase bullet))
                {
                    _playerID = bullet.OwnerId;
                    status.AddAdditionalStatus(bullet.status);
                }
            }

            var bulletIsBombObj = Runner.FindObject(BulletIsBombId);
            if (bulletIsBombObj)
            {
                if (bulletIsBombObj.TryGetComponent(out BulletIsBomb bib))
                {
                    status.AddAdditionalStatus(bib.status); // 스킬의 스테이터스 추가 총알하고 겹칠 위험이 있지만 그대로 사용한다.
                }
            }
            
            explodeEffect.SetActive(true);
            
            Destroy(gameObject, 1f);
        }

        #endregion
    }
}