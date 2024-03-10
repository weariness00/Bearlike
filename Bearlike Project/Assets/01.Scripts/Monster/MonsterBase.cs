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
        
        public int id = 0;
        public MonsterStatus status;
        public LootingTable lootingTable;

        public Transform targetTransform;

        public Action DieAction;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
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

