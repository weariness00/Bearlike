using System;
using System.Collections.Generic;
using Data;
using Fusion;
using GamePlay.DeadBodyObstacle;
using Item.Looting;
using Manager;
using Photon;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Monster
{
    [RequireComponent(typeof(MonsterStatus), typeof(LootingTable), typeof(DeadBodyObstacleObject))]
    public class MonsterBase : NetworkBehaviourEx, IJsonData<MonsterJsonData>
    {
        #region Static

        // Info Data 캐싱
        private static readonly Dictionary<int, MonsterJsonData> InfoDataCash = new Dictionary<int, MonsterJsonData>();
        public static void AddInfoData(int id, MonsterJsonData data) => InfoDataCash.TryAdd(id, data);
        public static MonsterJsonData GetInfoData(int id) => InfoDataCash.TryGetValue(id, out var data) ? data : new MonsterJsonData();
        public static void ClearInfosData() => InfoDataCash.Clear();
        
        // Status Data 캐싱
        private static readonly Dictionary<int, StatusJsonData> StatusDataChasing = new Dictionary<int, StatusJsonData>();
        public static void AddStatusData(int id, StatusJsonData data) => StatusDataChasing.TryAdd(id, data);
        public static StatusJsonData GetStatusData(int id) => StatusDataChasing.TryGetValue(id, out var data) ? data : new StatusJsonData();
        public static void ClearStatusData() => StatusDataChasing.Clear();
        
        // Looting Data 캐싱
        private static readonly Dictionary<int, LootingJsonData> LootingDataChasing = new Dictionary<int, LootingJsonData>();
        public static void AddLootingData(int id, LootingJsonData data) => LootingDataChasing.TryAdd(id, data);
        public static LootingJsonData GetLootingData(int id) => LootingDataChasing.TryGetValue(id, out var data) ? data : new LootingJsonData();
        public static void ClearLootingData() => LootingDataChasing.Clear();

        #endregion
        
        [HideInInspector] public Rigidbody rigidbody;
        [HideInInspector] public NetworkMecanimAnimator networkAnimator;
        [HideInInspector] public MonsterStatus status;
        [HideInInspector] public LootingTable lootingTable;
        private DeadBodyObstacleObject _deadBody;
        public Transform pivot; // Pivot이 메쉬 가운데가 아닌 다리에 위치할 떄가 있다. 그때 진짜 pivot으로 사용할 변수
        
        [Header("Monster 정보")]
        public int id = 0;
        public string explain;
        public string type;
        
        public Transform targetTransform;
        public LayerMask targetMask;
        
        public Action DieAction;

        #region Unity Evenet Function

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            _deadBody = GetComponent<DeadBodyObstacleObject>();
            if (pivot == null) pivot = transform;
            
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();

            gameObject.layer = LayerMask.NameToLayer("Monster");
        }
        
        public virtual void Start()
        {
            lootingTable.CalLootingItem(GetLootingData(id).LootingItems);
            DieAction += () => _deadBody.OnDeadBodyRPC();
            DieAction += lootingTable.SpawnDropItem;
            
            status.SetJsonData(GetStatusData(id));
        }

        private void OnDestroy()
        {
            Destroy(lootingTable);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                if (status.IsDie)
                {
                    DieRPC();
                }
            }
        }
        
        #endregion

        #region Member Function

        /// <summary>
        /// Target과의 직선 거리를 알려주는 함수
        /// 장애물이 있는 경우 float.MaxValue를 반환
        /// </summary>
        /// <param name="targetPosition"> Target의 위치 </param>
        public float StraightDistanceFromTarget(Vector3 targetPosition)
        {
            var dir = targetPosition - transform.position;
            var excludeLayer = 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Weapon");
            var layer = Int32.MaxValue & ~excludeLayer;
            if (Physics.Raycast(transform.position, dir.normalized, out var hit, float.MaxValue, layer))
            {
                if (hit.transform.CompareTag("Player") == false)
                {
                    return float.MaxValue;
                }
            }
            
            return Vector3.Distance(transform.position, targetPosition);
        }

        /// <summary>
        /// NavMesh에 따른 Target과의 거리를 알려주는 함수
        /// </summary>
        /// <param name="targetPosition"> Target의 위치 </param>
        /// <returns></returns>
        public float NavMeshDistanceFromTarget(Vector3 targetPosition)
        {
            var path = new NavMeshPath();
            var dis = 0f;
            if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    dis += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
            }

            return dis;
        }
        
        #endregion

        #region Json Data Interface

        public MonsterJsonData GetJsonData()
        {
            MonsterJsonData data = new MonsterJsonData()
            {
                ID = id,
                Name = name,
                Explain = explain,
                Type = type,
            };
            return data;
        }

        public void SetJsonData(MonsterJsonData json)
        {
            name = json.Name;
            explain = json.Explain;
            type = json.Type;
        }        

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DieRPC()
        {
            DieAction?.Invoke();
            Destroy(this);
            DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
        }
        
        #endregion
    }
}

