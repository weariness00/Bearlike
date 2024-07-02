using System.Collections;
using Aggro;
using Fusion;
using GamePlay;
using Manager;
using Photon;
using Photon.MeshDestruct;
using Status;
using UI.Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
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
        private AggroTarget _aggroTarget;

        private IWeaponHitEffect _hitEffect;
        private IWeaponHitSound _hitSound;
        private IWeaponHit _hitInterface;

        private Vector3 direction;
        public Vector3 destination = Vector3.zero;
        [Networked] public int PenetrateCount { get; set; } // 관통 가능 횟수
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

            // DebugManager.ToDo("Json으로 moveSpeed받아오도록 수정");
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
                _aggroTarget = ownerObj.GetComponent<AggroTarget>();
            }

            var ownerGunObj = Runner.FindObject(OwnerGunId);
            if (ownerGunObj)
            {
                if(ownerGunObj.TryGetComponent(out StatusBase s)) status.AddAdditionalStatus(s); 
                ownerGunObj.TryGetComponent(out _hitEffect);
                ownerGunObj.TryGetComponent(out _hitInterface);
                if (ownerGunObj.TryGetComponent(out IWeaponHitSound hs)) _hitSound = hs;
            }
        }

        public override void FixedUpdateNetwork()
        {
            transform.position += direction * Runner.DeltaTime * status.GetMoveSpeed();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;

            if (_aggroTarget)
            {
                var point = other.ClosestPoint(transform.position);
                var hits = Physics.SphereCastAll(point, 6f, Vector3.zero, 0f);
                foreach (var hit in hits)
                {
                    if(hit.collider.TryGetComponent(out AggroController aggroController) && aggroController.HasTarget() == false) aggroController.ChangeAggroTarget(_aggroTarget);
                }
            }

            StatusBase otherStatus = null;
            if (other.TryGetComponent(out ColliderStatus colliderStatus))
            {
                otherStatus = colliderStatus.originalStatus;
                status.AddAdditionalStatus(colliderStatus.status);
                _hitInterface?.BeforeHitAction?.Invoke(gameObject, otherStatus.gameObject);
                
                otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                status.RemoveAdditionalStatus(colliderStatus.status);
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
                
                _hitInterface?.AfterHitAction?.Invoke(gameObject, otherStatus.gameObject);
            }
            else if (other.TryGetComponent(out otherStatus) || other.transform.root.gameObject.TryGetComponent(out otherStatus))
            {
                _hitInterface?.BeforeHitAction?.Invoke(gameObject, otherStatus.gameObject);

                otherStatus.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                
                _hitInterface?.AfterHitAction?.Invoke(gameObject, otherStatus.gameObject);
            }
            // 메쉬 붕괴 객체와 충돌 시
            else if (other.CompareTag("Destruction"))
            {
                var networkObj = other.GetComponent<NetworkObject>();
                NetworkMeshSliceSystem.Instance.SliceRPC(networkObj.Id, Random.onUnitSphere, transform.position, 0f);
            }

            if (PenetrateCount-- == 0)
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