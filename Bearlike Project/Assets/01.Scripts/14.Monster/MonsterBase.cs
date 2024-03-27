using System;
using Fusion;
using Item.Looting;
using Manager;
using Photon;
using State.StateClass;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Monster
{
    [RequireComponent(typeof(MonsterStatus), typeof(LootingTable), typeof(Rigidbody))]
    public class MonsterBase : NetworkBehaviourEx
    {
        [HideInInspector] public Rigidbody rigidbody;
        [HideInInspector] public NetworkMecanimAnimator networkAnimator;
        public Transform pivot; // Pivot이 메쉬 가운데가 아닌 다리에 위치할 떄가 있다. 그때 진짜 pivot으로 사용할 변수
        
        [Header("Monster 정보")]
        public int id = 0;
        public MonsterStatus status;
        public LootingTable lootingTable;
        
        public Transform targetTransform;
        public LayerMask targetMask;
        
        public Action DieAction;

        #region Unity Evenet Function
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            if (pivot == null) pivot = transform;
            
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();

            gameObject.layer = LayerMask.NameToLayer("Monster");
        }
        
        public virtual void Start()
        {
            lootingTable.CalLootingItem(LootingSystem.MonsterTable(id));
            DieAction += lootingTable.SpawnDropItem;
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

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DieRPC()
        {
            DieAction?.Invoke();
            gameObject.SetActive(false);
            DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
        }
        
        #endregion
    }
}

