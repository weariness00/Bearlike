using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aggro;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay;
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
    [RequireComponent(typeof(MonsterStatus), typeof(LootingTable), typeof(AggroController))]
    public abstract class MonsterBase : NetworkBehaviourEx, IJsonData<MonsterJsonData>
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
        [HideInInspector] public Collider collider;
        [HideInInspector] public NetworkMecanimAnimator networkAnimator;
        [HideInInspector] public MonsterStatus status;
        [HideInInspector] public LootingTable lootingTable;
        private DeadBodyObstacleObject _deadBody;
        public Transform pivot; // Pivot이 메쉬 가운데가 아닌 다리에 위치할 떄가 있다. 그때 진짜 pivot으로 사용할 변수
        [HideInInspector] public AggroController aggroController;
        [HideInInspector] public NavMeshAgent navMeshAgent;
        
        protected BehaviorTreeRunner behaviorTreeRunner;
        
        [Header("Monster 정보")]
        public int id = 0;
        public string explain;
        public string type;
        
        public CrowdControl crowdControlType;
        public LayerMask targetMask;
        
        public Action DieAction;
        
        [Header("기본 Effect")]
        [SerializeField] protected NetworkPrefabRef dieEffectRef;

        #region Unity Evenet Function

        public virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
            if (collider) collider.enabled = false;
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            _deadBody = GetComponent<DeadBodyObstacleObject>();
            if (pivot == null) pivot = transform;
            
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
            aggroController = GetComponent<AggroController>();

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
            DieAction += OnDieAction;

            var statusData = GetStatusData(id);
            status.SetJsonData(statusData);
            if (statusData.HasFloat("Rigidbody Mass")) rigidbody.mass = statusData.GetFloat("Rigidbody Mass");

            SetDifficultStatus();
        }
        
        private void OnDestroy()
        {
            Destroy(lootingTable);
        }

        public override void Spawned()
        {
            behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
            aggroController.AddTarget(FindObjectsOfType<AggroTarget>());// 접속한 플레이어들 저장
        }

        public override void FixedUpdateNetwork()
        {
            behaviorTreeRunner.Operator();
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

        private void SetDifficultStatus()
        {
            status.hp.Max = (int)(status.hp.Max * Difficult.MonsterHpRate);
            status.hp.SetMax();
            status.damage.Current = (int)(status.damage.Current * Difficult.MonsterHpRate);
        }
        
        private async void OnDieAction()
        {
            if (_deadBody) _deadBody.OnDeadBodyRPC();
            lootingTable.SpawnDropItem();
            Destroy(this);

            // Effect
            if (dieEffectRef != NetworkPrefabRef.Empty)
            {
                var obj = await Runner.SpawnAsync(dieEffectRef, pivot.position, pivot.rotation);
                Destroy(obj.gameObject, 2f);
            }
        }

        public void RotateToTarget()
        {
            if (!aggroController.HasTarget()) return;
            
            Vector3 dir = (aggroController.GetTarget().transform.position - transform.position).normalized;
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
                // Target이 NavMesh가 없는 곳에 있을때는 직선거리로 판단한다.
                if (path.corners.Length == 0)
                    dis = StraightDistanceFromTarget(targetPosition);
                
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    dis += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
            }
            else
                dis = StraightDistanceFromTarget(targetPosition);

            return dis;
        }
        
        public bool CheckStraightDis(float checkDis)
        {
            if (!aggroController.HasTarget())
                return false;
            
            var dis = StraightDistanceFromTarget(aggroController.GetTarget().transform.position);
            return dis < checkDis;
        }

        /// <summary>
        /// 경로에 Nav Mesh Link가 포함되어있는지 확인
        /// </summary>
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
        
        public Vector3 SetAgentDestination(Vector3 targetPosition)
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
            if (!aggroController.HasTarget())
                return false;
            
            var dis = NavMeshDistanceFromTarget(aggroController.GetTarget().transform.position);
            return dis < checkDis;
        }

        public void DisableNavMeshAgent(bool isIncludeCollider = true, bool isGravity = true)
        {
            if (!navMeshAgent)
                return;
            
            navMeshAgent.enabled = false;
            rigidbody.useGravity = isGravity;
            rigidbody.isKinematic = false;

            if (isIncludeCollider)
            {
                collider.includeLayers = 1 << LayerMask.NameToLayer("Default");
                collider.excludeLayers = 1 << LayerMask.NameToLayer("Ignore Nav Mesh");
                collider.enabled = true;
            }
        }
        
        /// <summary>
        /// Agent가 Surface위에 있다면 다시 활성화
        /// 활성화 방법은 RigidBody의 isKinematic을 활성화 해주면 된다.
        /// </summary>
        /// <param name="duration"> 이 시간만큼 뒤에 동작한다.</param>
        public void EnableNavMeshAgent(float duration = 0f)
        {
            StopCoroutine(nameof(EnableNavMeshAgentCoroutine));
            StartCoroutine(EnableNavMeshAgentCoroutine(duration));
        }

        private IEnumerator EnableNavMeshAgentCoroutine(float duration)
        {
            if (!navMeshAgent)
                yield break;

            if (duration != 0)
                yield return new WaitForSeconds(duration);
            
            LayerMask mask = 1 << LayerMask.NameToLayer("Default");
            var originPivot = new Vector3(0, 0.1f, 0);
            while (true)
            {
                yield return null;
                DebugManager.DrawRay(transform.position + originPivot, -transform.up * 0.3f, Color.blue, 1f);
                if (Runner.LagCompensation.Raycast(transform.position + originPivot, -transform.up, 0.3f, Runner.LocalPlayer, out var hit) || 
                    Physics.Raycast(transform.position + originPivot, -transform.up, out var phit, 0.3f))
                {
                    navMeshAgent.enabled = true;
                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;
                    collider.enabled = false;
                    break;
                }
            }
        }
        
        #endregion

        #region Default BT Function

        public abstract INode InitBT();

        protected INode.NodeState FindTarget()
        {
            aggroController.SetRange(status.attackRange.Current + 30f);
            aggroController.CheckTargetAggro();
            if (!aggroController.HasTarget())
            {
                aggroController.FindAggroTarget();
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

