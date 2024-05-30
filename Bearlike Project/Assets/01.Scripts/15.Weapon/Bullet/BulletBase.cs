using System.Collections;
using Fusion;
using GamePlay;
using Manager;
using Photon;
using Photon.MeshDestruct;
using Player;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Weapon.Bullet
{
    [RequireComponent(typeof(StatusBase))]
    public class BulletBase : NetworkBehaviourEx
    {
        #region Member Variable
        
        [Networked] public NetworkId OwnerId { get; set; } // 이 총을 쏜 주인의 ID
        [Networked] public NetworkId OwnerGunId { get; set; }
        public StatusBase status;

        private IWeaponHitEffect _hitEffect;
        private IWeaponHitSound _hitSound;

        private Vector3 direction;
        public Vector3 destination = Vector3.zero;
        public int penetrateCount = 0; // 관통 가능 횟수
        [Networked] public int KnockBack { get; set; }

        #endregion

        public void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Bullet");
            
            status = GetComponent<StatusBase>();
        }

        protected void Start()
        {
            direction = (destination - transform.position).normalized;

            DebugManager.ToDo("Json으로 moveSpeed받아오도록 수정");
            status.moveSpeed.Max = 50;
            status.moveSpeed.Current = status.moveSpeed.Max;
        }

        public override void Spawned()
        {
            Destroy(gameObject, 30f);

            var ownerObj = Runner.FindObject(OwnerId);
            if (ownerObj)
            {
                _hitSound = ownerObj.GetComponent<IWeaponHitSound>();
            }

            var ownerGunObj = Runner.FindObject(OwnerGunId);
            if (ownerGunObj)
            {
                if(ownerGunObj.TryGetComponent(out StatusBase s)) status.AddAdditionalStatus(s); 
                ownerGunObj.TryGetComponent(out _hitEffect);
            }
        }

        public override void FixedUpdateNetwork()
        {
            transform.position += direction * Runner.DeltaTime * status.moveSpeed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;
            
            if (other.TryGetComponent(out ColliderStatus colliderStatus))
            {
                StatusBase otherStatus = colliderStatus.originalStatus;
                otherStatus.AddAdditionalStatus(colliderStatus.status);
                
                otherStatus.ApplyDamageRPC(status.CalDamage(), OwnerId);//
                otherStatus.RemoveAdditionalStatus(colliderStatus.status);
                _hitEffect?.OnWeaponHitEffect(transform.position);
                _hitSound?.PlayWeaponHit();
                
                if (KnockBack > 0)
                {
                    var parent = otherStatus.gameObject;
                        
                    Vector3 knockbackDirection = parent.transform.position - transform.position;
                    knockbackDirection.y = 0;
                    knockbackDirection.Normalize();

                    otherStatus.KnockBackRPC(knockbackDirection, KnockBack);
                }
            }
            else if (other.transform.root.gameObject.TryGetComponent(out PlayerStatus playerStatus))
            {
                playerStatus.ApplyDamageRPC(status.CalDamage(), OwnerId);
            }
            else if (other.TryGetComponent(out StatusBase otherStatus))
            {
                otherStatus.ApplyDamageRPC(status.CalDamage(), OwnerId);
            }
            // 메쉬 붕괴 객체와 충돌 시
            else if (other.CompareTag("Destruction"))
            {
                var networkObj = other.GetComponent<NetworkObject>();
                NetworkMeshSliceSystem.Instance.SliceRPC(networkObj.Id, Random.onUnitSphere, transform.position, 0f);
            }

            if (penetrateCount-- == 0)
            {
                Destroy(gameObject);
            }
        }

        IEnumerator RestartNavAgentCorutine(NavMeshAgent _nav)
        {
            yield return new WaitForSeconds(0.5f);
            if(_nav != null) _nav.enabled = true;
        }
        
        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }
    }
}