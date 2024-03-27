using Script.Weapon.Gun;
using State.StateClass.Base;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapon.Bullet
{
    public class BulletBase : MonoBehaviour
    {
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

        protected void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            // if (other.gameObject.CompareTag("Destruction"))
            // {
            //     MeshDestruction.Destruction(other.gameObject, PrimitiveType.Cube, other.contacts[0].point, Vector3.one);
            // }
            
            if (!other.gameObject.CompareTag("Monster") || other.GetComponent<StatusBase>().hp.isMin)
            {
                return;
            }
            
            // player가 건이다.
            var playerStatus = player.transform.root.GetComponent<StatusBase>();
            var gun = player.GetComponentInChildren<GunBase>();
            
            other.gameObject.GetComponent<StatusBase>().ApplyDamageRPC(playerStatus.attack.Current + gun.attack, (CrowdControl)(playerStatus.property | gun.property));

            Destroy(gameObject);
        }
        
        [BurstCompile]
        public static float FastDistance(float3 pointA, float3 pointB)
        {
            return math.distance(pointA, pointB);
        }
    }
}