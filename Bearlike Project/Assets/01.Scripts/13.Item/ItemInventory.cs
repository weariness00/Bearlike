using System.Linq;
using Inventory;
using UI.Inventory;

namespace Item
{
    public class ItemInventory : InventoryBase<ItemBase, ItemUIHandle>
    {
        public bool HasItem(int itemId)
        {
            var targetItem = itemHashSet.First(item => item.Id == itemId);
            return targetItem != null;
        }
    }
}