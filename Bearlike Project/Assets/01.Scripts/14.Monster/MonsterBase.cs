using System;
using Fusion;
using Item.Looting;
using Manager;
using State.StateClass;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Monster
{
    [RequireComponent(typeof(MonsterStatus), typeof(LootingTable), typeof(Rigidbody))]
    public class MonsterBase : NetworkBehaviour
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
            if (LootingSystem.Instance.monsterLootingItemDictionary.TryGetValue(id, out var lootingItems))
            {
                lootingTable.CalLootingItem(lootingItems);
                DieAction += lootingTable.SpawnDropItem;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (status.IsDie)
            {
                DieAction?.Invoke();
                // Destroy(gameObject);
                gameObject.SetActive(false);
                DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
            }
        }
    }
}

