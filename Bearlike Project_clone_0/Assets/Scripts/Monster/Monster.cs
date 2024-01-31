using System;
using Fusion;
using Item.Looting;
using Script.Util;
using Scripts.State.GameStatus;
using Unity.VisualScripting;

namespace Script.Monster
{
    public class Monster : NetworkBehaviour
    {
        public Status status;
        public LootingTable lootingTable;
        public int id = 0;

        private void Awake()
        {
            status = gameObject.GetOrAddComponent<Status>();
            lootingTable = gameObject.GetOrAddComponent<LootingTable>();
        }

        private void Start()
        {
            if (LootingSystem.Instance.monsterLootingItemDictionary.TryGetValue(id, out var lootingItems))
            {
                lootingTable.CalLootingItem(lootingItems);
            }
        }
    }
}

