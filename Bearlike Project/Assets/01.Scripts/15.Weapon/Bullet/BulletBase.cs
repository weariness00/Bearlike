using Fusion;
using Photon;
using Script.Weapon.Gun;
using State.StateClass.Base;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Weapon.Bullet
{
    [RequireComponent(typeof(NetworkTransform))]
    public class BulletBase : NetworkBehaviourEx
    {
        public StatusValue<int> damage = new StatusValue<int>() {Max = 100, Current = 100 };
        public StatusValue<int> speed = new StatusValue<int>(){Max = 100, Current = 100};
        public Vector3 destination = Vector3.zero;

        #region 사정거리

        public GameObject player;
        
        private Vector3 _oldPosition;
        
        public float maxMoveDistance;   // 최대 사정거리

        #endregion

        protected void Start()
        {
            _oldPosition = transform.position;
            
            transform.rotation = Quaternion.LookRotation(destination);
            Destroy(gameObject, 5f);
        }

        public override void FixedUpdateNetwork()
        {
            _oldPosition = transform.position;
            transform.position += transform.forward * speed * Runner.DeltaTime;

            if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            // if (!other.gameObject.CompareTag("Monster") || other.GetComponent<StatusBase>().hp.isMin)
            // {
            //     return;
            // }

            StatusBase otherStatus;
            if (other.TryGetComponent(out otherStatus) || other.transform.root.TryGetComponent(out otherStatus))
            {
                otherStatus.ApplyDamageRPC(damage);
            }
            
            // // player가 건이다.
            // var playerStatus = player.transform.root.GetComponent<StatusBase>();
            // var gun = player.GetComponentInChildren<GunBase>();
            //
            // other.gameObject.GetComponent<StatusBase>().ApplyDamageRPC(playerStatus.damage.Current + gun.attack, (CrowdControl)(playerStatus.property | gun.property));
            // moveDistance = 0.0f;
            Destroy(gameObject);
        }
        
        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetDamageRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Current:
                    damage.Current = value;
                    break;
                case StatusValueType.Max:
                    damage.Max = value;
                    break;
            }
        }

        #endregion
    }
}