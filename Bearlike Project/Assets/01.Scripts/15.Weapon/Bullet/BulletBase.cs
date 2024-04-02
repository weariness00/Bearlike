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
        public StatusValue<float> speed = new StatusValue<float>(){Max = 100.0f, Current = 50.0f};
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

        // public override void FixedUpdateNetwork()
        // {
        //     _oldPosition = transform.position;
        //     transform.position += transform.forward * Runner.DeltaTime * speed;
        //     
        //     if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
        // }

        public override void FixedUpdateNetwork()
        {
            Debug.Log("FixedUpdateNetwork");    
            _oldPosition = transform.position;
            transform.position += transform.forward * Runner.DeltaTime * speed;
                
            if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
            
        }
        
        // public void Update()
        // {
        //     _oldPosition = transform.position;
        //     transform.position += transform.forward * Time.deltaTime * speed;
        //     
        //     // Debug.Log(destination);
        //     Debug.Log("Update");
        //     
        //     if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
        // }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Bullet"))
            {
                // // player가 건이다.
                var playerStatus = player.transform.root.GetComponent<StatusBase>();
                var gun = player.GetComponentInChildren<GunBase>();

                StatusBase otherStatus;
                if (other.TryGetComponent(out otherStatus) || other.transform.root.TryGetComponent(out otherStatus))
                {
                    otherStatus.ApplyDamageRPC(playerStatus.damage.Current + gun.attack,
                        (CrowdControl)(playerStatus.property | gun.property));
                }

                // other.gameObject.GetComponent<StatusBase>().ApplyDamageRPC(playerStatus.damage.Current + gun.attack, (CrowdControl)(playerStatus.property | gun.property));
                Destroy(gameObject);
            }
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