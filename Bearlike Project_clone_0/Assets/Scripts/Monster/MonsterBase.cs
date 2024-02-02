using System;
using Fusion;
using Item.Looting;
using Script.Util;
using Scripts.State.GameStatus;
using State.StateClass;
using Unity.VisualScripting;

namespace Script.Monster
{
    public class MonsterBase : NetworkBehaviour
    {
        public int id = 0;
        public MonsterState status;
        public LootingTable lootingTable;

        public Action DieAction;

        private void Awake()
        {
            status = gameObject.GetOrAddComponent<MonsterState>();
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
            base.FixedUpdateNetwork();
            if (status.IsDie)
            {
                DieAction?.Invoke();
            }
        }
    }
}

