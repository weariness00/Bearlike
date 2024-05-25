using System;
using System.Collections;
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
using UnityEngine.Serialization;
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
        
        public PlayerController targetPlayer; // 나중에 어그로 시스템 생기면 바꾸기
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
                Destroy(this);
            };

            var statusData = GetStatusData(id);
            status.SetJsonData(statusData);
            if (statusData.HasFloat("Rigidbody Mass")) rigidbody.mass = statusData.GetFloat("Rigidbody Mass");
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

        public void RotateTarget()
        {
            Vector3 dir = (targetPlayer.transform.position - transform.position).normalized;
            dir.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Runner.DeltaTime);
        }

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
        
        public bool CheckStraightDis(float checkDis)
        {
            if (targetPlayer == null)
                return false;
            
            var dis = StraightDistanceFromTarget(targetPlayer.transform.position);
            return dis < checkDis;
        }

        /// <summary>
        /// 경로에 Nav Mesh Link가 포함되어있는지 확인
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public bool IsIncludeLink(Vector3 targetPosition)
        {
            if (navMeshAgent == null) return false;
            
            // 경로 계산
            NavMeshPath path = new NavMeshPath();
            if (navMeshAgent.CalculatePath(targetPosition, path))
            {
                // 경로에 네비메쉬 링크가 포함되어 있는지 확인
                NavMeshHit hit;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    if (NavMesh.Raycast(path.corners[i], path.corners[i + 1], out hit, NavMesh.AllAreas))
                    {
                        if (hit.hit && hit.mask == NavMesh.GetAreaFromName("OffMeshLink"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        
        Vector3 SetAgentDestination(Vector3 targetPosition)
        {
            if (!navMeshAgent) return targetPosition;
            
            // 목표 위치 근처의 가장 가까운 네비메쉬 표면 위치를 찾기
            if (NavMesh.SamplePosition(targetPosition, out var hit, 5f, NavMesh.AllAreas))
                return hit.position;
            
            DebugManager.LogWarning("목표 위치 근처에 유효한 네비메쉬 표면을 찾을 수 없습니다.");
            return targetPosition;
        }
        
        /// <summary>
        /// 타겟과의 거리를 판단하여 인자로 넣은 값과 비교해 거리가 인자보다 낮으면 true
        /// </summary>
        /// <param name="checkDis">이 거리보다 낮으면 True, 높으면 False</param>
        /// <returns></returns>
        public bool CheckNavMeshDis(float checkDis)
        {
            if (targetPlayer == null)
            {
                return false;
            }
            var dis = NavMeshDistanceFromTarget(targetPlayer.transform.position);
            return dis < checkDis;
        }

        public void UpdateNavMeshAgent()
        {
            StopCoroutine(nameof(UpdateNavMeshAgentCoroutine));
            StartCoroutine(UpdateNavMeshAgentCoroutine());
        }

        private IEnumerator UpdateNavMeshAgentCoroutine()
        {
            if (!navMeshAgent)
                yield break;
            
            rigidbody.isKinematic = false;
            while (true)
            {
                if (navMeshAgent.isOnNavMesh)
                {
                    rigidbody.isKinematic = true;
                    break;
                }

                yield return null;
            }
        }
        
        #endregion

        #region Default BT Function

        protected INode.NodeState FindTarget()
        {
            DebugManager.ToDo("어그로 시스템이 없어 가장 가까운 적을 인식하도록 함" +
                              "어그로 시스템을 만들어 인식된 적들중 어그로가 높은 적을 인식하도록 바꾸기");
            
            // target이 쓰러진 상태면 Targeting 풀기
            if (!targetPlayer ||
                targetPlayer.status.isInjury ||
                targetPlayer.status.isRevive ||
                targetPlayer.status.IsDie)
            {
                targetPlayer = null;
            }
            
            if (!targetPlayer && players.Length != 0)
            {
                // 직선 거리상 인식 범위 내에 있는 플레이어 탐색
                var targetPlayers = players.Where(player => 
                    !player.status.isInjury &&
                    !player.status.isRevive &&
                    !player.status.IsDie &&
                    StraightDistanceFromTarget(player.transform.position) <= status.attackRange.Current + 10f
                    ).ToList();

                // 인식범위 내에 있는 아무 플레이어를 Target으로 지정
                targetPlayer = targetPlayers.Count != 0 ? targetPlayers[Random.Range(0, targetPlayers.Count)] : null;
            }
            else
            {
                // Target대상이 인식 범위내에 벗어나면 Target을 풀어주기
                var dis = StraightDistanceFromTarget(targetPlayer.transform.position);
                if (dis > status.attackRange.Current + 12f)
                {
                    targetPlayer = null;
                }
            }

            return INode.NodeState.Success;
        }
        
        /// <summary>
        /// NavMesh 상의 거리를 판단해 Target을 찾아준다.
        /// </summary>
        /// <returns></returns>
        public INode.NodeState FindTargetFromNavMesh()
        {
            // target이 쓰러진 상태면 Targeting 풀기
            if (!targetPlayer ||
                targetPlayer.status.isInjury ||
                targetPlayer.status.isRevive ||
                targetPlayer.status.IsDie)
            {
                targetPlayer = null;
            }
            
            if (targetPlayer == null && players.Length != 0)
            {
                var targetPlayers = players.Where(player => 
                    !player.status.isInjury &&
                    !player.status.isRevive &&
                    !player.status.IsDie &&
                    NavMeshDistanceFromTarget(player.transform.position) <= status.attackRange.Current + 10f
                ).ToList();
                
                targetPlayer = targetPlayers.Count != 0 ? targetPlayers[Random.Range(0, targetPlayers.Count)] : null;
            }
            else
            {
                // Target대상이 인식 범위내에 벗어나면 Target을 풀어주기
                var dis = NavMeshDistanceFromTarget(targetPlayer.transform.position);
                if (dis > status.attackRange.Current + 12f)
                {
                    targetPlayer = null;
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
            DieAction?.Invoke();//
            DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
        }
        
        #endregion
    }
}

