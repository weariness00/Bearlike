using System;
using Inventory;
using Skill;
using UI.Inventory;
using UnityEngine;

namespace Item
{
    public class ItemInventory : InventoryBase<ItemBase, ItemUIHandle>
    {
        public ItemBase ib;
    }
}