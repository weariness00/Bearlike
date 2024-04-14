using System.Collections.Generic;
using Data;
using Fusion;
using Manager;
using Photon;
using Status;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Weapon.Bullet
{
    [RequireComponent(typeof(StatusBase))]
    public class BulletBase : NetworkBehaviourEx, IJsonData<BulletJsonData>
    {
        [HideInInspector] [Networked] public NetworkId OwnerId { get; set; } // 이 총을 쏜 주인의 ID
        public StatusBase status;
        public int penetrateCount = 0; // 관통 가능 횟수

        // Info Data 캐싱
        private static readonly Dictionary<int, BulletJsonData> InfoDataCash = new Dictionary<int, BulletJsonData>();
        public static void AddInfoData(int id, BulletJsonData data) => InfoDataCash.TryAdd(id, data);
        public static BulletJsonData GetInfoData(int id) => InfoDataCash.TryGetValue(id, out var data) ? data : new BulletJsonData();
        public static void ClearInfosData() => InfoDataCash.Clear();
        
        // Status Data 캐싱
        private static readonly Dictionary<int, StatusJsonData> StatusDataChasing = new Dictionary<int, StatusJsonData>();
        public static void AddStatusData(int id, StatusJsonData data) => StatusDataChasing.TryAdd(id, data);
        public static StatusJsonData GetStatusData(int id) => StatusDataChasing.TryGetValue(id, out var data) ? data : new StatusJsonData();
        public static void ClearStatusData() => StatusDataChasing.Clear();
        
        #region 속성
        
        public Vector3 destination = Vector3.zero;
        public VisualEffect hitEffect;
        
        private Vector3 direction;
        public bool bknock = false;
        
        public int id = 0;
        public string explain;
        
        #endregion
        
        #region 사정거리
        private Vector3 _oldPosition;
        // public float maxMoveDistance;   // 최대 사정거리 // 이거 대신 status.attackRange 씀

        #endregion

        public void Awake()
        {
            status = GetComponent<StatusBase>();
        }

        protected void Start()
        {

            direction = (destination - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(destination);
            
            status.SetJsonData(GetStatusData(id));
            
            DebugManager.ToDo("Json으로 moveSpeed받아오도록 수정");
            status.moveSpeed.Max = 50;
            status.moveSpeed.Current = status.moveSpeed.Max;
        }

        public override void Spawned()
        {
            _oldPosition = transform.position;
        }

        public override void FixedUpdateNetwork()
        { 
            transform.position += direction * Runner.DeltaTime * status.moveSpeed;
            // transform.position += transform.forward * Runner.DeltaTime * speed;
            transform.Rotate(new Vector3(0, 10, 0), Space.Self);

            // if (FastDistance(transform.position, _oldPosition) >= maxMoveDistance) Destroy(gameObject);
            if (FastDistance(transform.position, _oldPosition) >= status.attackRange.Current) Destroy(gameObject);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus;
            if (other.TryGetComponent(out otherStatus) || other.transform.root.TryGetComponent(out otherStatus))
            {
                // player가 건이다.
                otherStatus.ApplyDamageRPC(status.CalDamage(), OwnerId);
                    
                if (bknock)
                {
                    otherStatus.gameObject.transform.Translate(direction);
                }
                
                // var hitEffectObject = Instantiate(hitEffect.gameObject, transform.position, Quaternion.identity);
                // hitEffectObject.transform.LookAt(gun.transform.position);
                // Destroy(hitEffectObject, 5f);
            }
            // 메쉬 붕괴 객체와 충돌 시
            else if (other.CompareTag("Destruction"))
            {
                NetworkMeshDestructSystem.Instance.DestructRPC(other.GetComponent<NetworkObject>().Id,PrimitiveType.Cube, transform.position, Vector3.one * 2, transform.forward);
            }

            if (penetrateCount-- == 0)
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
        
        public BulletJsonData GetJsonData()
        {
            throw new System.NotImplementedException();
        }

        public void SetJsonData(BulletJsonData json)
        {
            name = json.Name;
            explain = json.Explain;
        }
    }
}