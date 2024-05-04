using Manager;
using UI.Inventory;
using Unity.VisualScripting;

namespace Skill
{
    public class SkillInventory : InventoryBase<SkillBase, SkillUIHandle>
    {
        // 스킬은 이미 객체가 생성된 상태로 존재하는것을 보내주기에 프리펩을 따로 안만들게 한다.
        public override void AddItem(SkillBase item)
        {
            if (itemHashSet.TryGetValue(item, out var hashItem))
            {
                if (hashItem is IInventoryItemAdd inventoryItemAdd)
                {
                    inventoryItemAdd.AddItem(item);
                }
                var handle = uiHandleDictionary[hashItem];
                if (handle.TryGetComponent(out IInventoryUIUpdate handleUpdateInterface))
                {
                    handleUpdateInterface.UIUpdateFromItem(hashItem);
                }

                item = hashItem;
            }
            else
            {
                itemHashSet.Add(item);

                var handle = Instantiate(blockUIPrefab, uiParentTransform).GetComponent<SkillUIHandle>();
                handle.gameObject.SetActive(true);
                if (handle.TryGetComponent(out IInventoryUIUpdate handleUpdateInterface))
                {
                    handleUpdateInterface.UIUpdateFromItem(item);
                }
                
                uiHandleDictionary.Add(item, handle);
            }

            DebugManager.Log($"[{name}] Inventory에 {item.name}을 추가");
        }
    }
}

