using Fusion;
using Manager;
using Photon;
using Script.Weapon.Gun;
using State.StateClass.Base;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.VFX;

namespace Weapon.Bullet
{
    [RequireComponent(typeof(NetworkTransform))]
    public class BulletBase : NetworkBehaviourEx
    {
        public StatusValue<int> damage = new StatusValue<int>() {Max = 100, Current = 100 };
        public StatusValue<float> speed = new StatusValue<float>(){Max = 100.0f, Current = 50.0f};
        public Vector3 destination = Vector3.zero;

        public VisualEffect hitEffect;

        private Vector3 direction;
        public bool bknock = false;

        #region 사정거리

        public GameObject player;
        
        private Vector3 _oldPosition;
        
        public float maxMoveDistance;   // 최대 사정거리

        #endregion

        protected void Start()
        {
            _oldPosition = transform.position;

            direction = (destination - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(destination);
            
            Destroy(gameObject, 5f);
        }
        
        public override void FixedUpdateNetwork()
        { 
            transform.position += direction * Runner.DeltaTime * speed;
            // transform.position += transform.forward * Runner.DeltaTime * speed;
            transform.Rotate(new Vector3(0, 90, 0), Space.Self);

            if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus;
            if (other.TryGetComponent(out otherStatus) || other.transform.root.TryGetComponent(out otherStatus))
            {
                // player가 건이다.
                var playerStatus = player.transform.root.GetComponent<StatusBase>();
                var gun = player.GetComponentInChildren<GunBase>();

                otherStatus.ApplyDamageRPC(
                    (int)(((100.0f + (playerStatus.damage.Current)) / 100.0f) + (gun.attack.Current)), 
                    (CrowdControl)(playerStatus.property | gun.property));
                    
                if (bknock)
                {
                    otherStatus.gameObject.transform.Translate(direction);
                }
                
                var hitEffectObject = Instantiate(hitEffect.gameObject, transform.position, Quaternion.identity);
                hitEffectObject.transform.LookAt(gun.transform.position);
                Destroy(hitEffectObject, 5f);
            }
            // 메쉬 붕괴 객체와 충돌 시
            else if (other.CompareTag("Destruction"))
            {
                NetworkMeshDestructSystem.Instance.DestructRPC(other.GetComponent<NetworkObject>().Id,PrimitiveType.Cube, transform.position, Vector3.one * 2, transform.forward);
            }
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