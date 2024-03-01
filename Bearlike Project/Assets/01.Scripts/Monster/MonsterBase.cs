using System;
using Fusion;
using Item.Looting;
using Script.Manager;
using State.StateClass;
using Unity.VisualScripting;

namespace Monster
{
    public class MonsterBase : NetworkBehaviour
    {
        public int id = 0;
        public MonsterStatus status;
        public LootingTable lootingTable;

        public Action DieAction;

        private void Awake()
        {
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
        }

        private void Start()
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
                Destroy(gameObject);
                DebugManager.Log($"몬스터[{name}]이 사망했습니다.");
            }
        }
    }
}

