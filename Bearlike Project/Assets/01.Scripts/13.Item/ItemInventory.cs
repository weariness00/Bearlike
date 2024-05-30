using System.Linq;
using Fusion;
using UI.Inventory;

namespace Item
{
    public class ItemInventory : InventoryBase<ItemBase, ItemUIHandle>
    {
        public bool HasItem(int itemId)
        {
            var targetItem = itemHashSet.FirstOrDefault(item => item.Id == itemId);
            return targetItem != null;
        }
        
        #region Rpc Function

        [Rpc(RpcSources.All,RpcTargets.All)]
        public void AddItemRPC(NetworkItemInfo itemInfo)
        {
            var item = ItemObjectList.GetFromId(itemInfo.Id);
            var obj = Instantiate(item.gameObject);
            var objItem = obj.GetComponent<ItemBase>();
            objItem.info = item.info;
            objItem.Amount.Current = itemInfo.amount;
            AddItem(objItem);
            Destroy(objItem.gameObject);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void UseItemRPC(NetworkItemInfo itemInfo)
        {
            var item = ItemObjectList.GetFromId(itemInfo.Id);
            item.Amount.Current = itemInfo.amount;
            UseItem(item);
        }

        #endregion
    }
}