using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay.DeadBodyObstacle;
using Item.Looting;
using Manager;
using Photon;
using Player;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Monster
{
    [RequireComponent(typeof(MonsterStatus), typeof(LootingTable))]
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

        protected static readonly float ForceMagnitude = 13f;
        
        #endregion
        
        [HideInInspector] public Rigidbody rigidbody;
        [HideInInspector] public NetworkMecanimAnimator networkAnimator;
        [HideInInspector] public MonsterStatus status;
        [HideInInspector] public LootingTable lootingTable;
        private DeadBodyObstacleObject _deadBody;
        public Transform pivot; // Pivot이 메쉬 가운데가 아닌 다리에 위치할 떄가 있다. 그때 진짜 pivot으로 사용할 변수
        protected NavMeshAgent navMeshAgent;
        
        protected PlayerController[] players;
        
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

            // 하위 콜라이더들 재설정
            var colliders = GetComponentsInChildren<Collider>();
            var collideExcludeMask= 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Weapon"); 
            foreach (var col in colliders)
            {
                col.excludeLayers = collideExcludeMask;
                col.gameObject.layer = LayerMask.NameToLayer("Monster");
                col.gameObject.tag = "Monster";
            }
            
            gameObject.layer = LayerMask.NameToLayer("Monster");
        }
        
        public virtual void Start()
        {
            if(rigidbody) rigidbody.drag = 0.6f;
            
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent)
            {
                navMeshAgent.enabled = false;
                if (NavMesh.SamplePosition(transform.position, out var hit, 10.0f, NavMesh.AllAreas))
                    transform.position = hit.position; // NavMesh 위치로 이동
                navMeshAgent.enabled = true;
            }

            lootingTable.CalLootingItem(GetLootingData(id).LootingItems);
            DieAction += () =>
            {
                if (_deadBody) _deadBody.OnDeadBodyRPC();
                lootingTable.SpawnDropItem();
            };
            
            status.SetJsonData(GetStatusData(id));
        }

        private void OnDestroy()
        {
            Destroy(lootingTable);
        }

        public override void Spawned()
        {
            players = FindObjectsOfType<PlayerController>(); // 접속한 플레이어들 저장
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

        #region Default BT Function

        protected INode.NodeState FindTarget()
        {
            DebugManager.ToDo("어그로 시스템이 없어 가장 가까운 적을 인식하도록 함" +
                              "어그로 시스템을 만들어 인식된 적들중 어그로가 높은 적을 인식하도록 바꾸기");
            
            if (targetTransform == null)
            {
                // 직선 거리상 인식 범위 내에 있는 플레이어 탐색
                var targetPlayers = players.Where(player => !player.status.isInjury && !player.status.isRevive && !player.status.IsDie && StraightDistanceFromTarget(player.transform.position) <= status.attackRange.Current + 10f).ToList();
                if (targetPlayers.Count != 0)
                {
                    // 인식범위 내에 있는 아무 플레이어를 Target으로 지정
                    targetTransform = targetPlayers[Random.Range(0, targetPlayers.Count)].transform;
                }
            }
            else
            {
                // Target대상이 인식 범위내에 벗어나면 Target을 풀어주기
                var dis = StraightDistanceFromTarget(targetTransform.position);
                if (dis > status.attackRange.Current +20f)
                {
                    targetTransform = null;
                }
            }

            return INode.NodeState.Success;
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
            DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
        }
        
        #endregion
    }
}

