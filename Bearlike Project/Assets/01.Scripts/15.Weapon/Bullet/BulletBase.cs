﻿using Fusion;
using GamePlay;
using Manager;
using Photon;
using Photon.MeshDestruct;
using Player;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Weapon.Bullet
{
    [RequireComponent(typeof(StatusBase))]
    public class BulletBase : NetworkBehaviourEx
    {
        [HideInInspector] public NetworkId ownerId; // 이 총을 쏜 주인의 ID
        public StatusBase status;
        public int penetrateCount = 0; // 관통 가능 횟수

        #region 속성

        public Vector3 destination = Vector3.zero;
        public VisualEffect hitEffect;

        private Vector3 direction;
        public bool bknock = false;

        #endregion

        #region 사정거리

        private Vector3 _oldPosition;
        // public float maxMoveDistance;   // 최대 사정거리 // 이거 대신 status.attackRange 씀

        #endregion

        public void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Bullet");//
            
            status = GetComponent<StatusBase>();
        }

        protected void Start()
        {
            direction = (destination - transform.position).normalized;
            // transform.rotation = Quaternion.LookRotation(destination);

            DebugManager.ToDo("Json으로 moveSpeed받아오도록 수정");
            status.moveSpeed.Max = 50;
            status.moveSpeed.Current = status.moveSpeed.Max;
        }

        public override void Spawned()
        {
            _oldPosition = transform.position;
            Destroy(gameObject, 30f);
        }

        public override void FixedUpdateNetwork()
        {
            transform.position += direction * Runner.DeltaTime * status.moveSpeed;

            // if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
            if (FastDistance(transform.position, _oldPosition) >= status.attackRange.Current) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ColliderStatus colliderStatus))
            {
                StatusBase otherStatus = colliderStatus.originalStatus;
                otherStatus.AddAdditionalStatus(colliderStatus.status);
                
                // player가 건이다.
                otherStatus.ApplyDamageRPC(status.CalDamage(), ownerId);
                otherStatus.RemoveAdditionalStatus(colliderStatus.status);
                
                if (bknock)
                {
                    // TODO : 수정 필요
                    otherStatus.gameObject.transform.Translate(direction);
                }

                // var hitEffectObject = Instantiate(hitEffect.gameObject, transform.position, Quaternion.identity);
                // hitEffectObject.transform.LookAt(gun.transform.position);
                // Destroy(hitEffectObject, 5f);
            }
            else if (other.transform.root.gameObject.TryGetComponent(out PlayerStatus playerStatus))
            {
                playerStatus.ApplyDamageRPC(status.CalDamage(), ownerId);
            }
            else if (other.TryGetComponent(out StatusBase otherStatus))
            {
                otherStatus.ApplyDamageRPC(status.CalDamage(), ownerId);
            }
            // 메쉬 붕괴 객체와 충돌 시
            else if (other.CompareTag("Destruction"))
            {
                var networkObj = other.GetComponent<NetworkObject>();
                NetworkMeshSliceSystem.Instance.SliceRPC(networkObj.Id, Random.onUnitSphere, transform.position, 100f);
            }

            if (penetrateCount-- == 0)//
            {
                Destroy(gameObject);
            }
        }

        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }

        // #region RPC Function
        //
        // [Rpc(RpcSources.All, RpcTargets.All)]
        // public void SetDamageRPC(StatusValueType type, int value)
        // {
        //     switch (type)
        //     {
        //         case StatusValueType.Current:
        //             damage.Current = value;
        //             break;
        //         case StatusValueType.Max:
        //             damage.Max = value;
        //             break;
        //     }
        // }
        //
        // #endregion
    }
}