using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Inventory를 관리하는 클래스
    /// </summary>
    /// <typeparam name="Item"> HashSet을 통해 관리 됨으로 Item으로 사용될 클래스에 GetHashCode와 Equals비교 함수 만들어줘야함 </typeparam>
    /// <typeparam name="UIHandle"></typeparam>
    [RequireComponent(typeof(Canvas))]
    public class InventoryBase<Item, UIHandle> : MonoBehaviour , IInventoryEditor
        where Item : Component, IInventoryItemAdd
        where UIHandle : Component
    {
        public Canvas canvas;
        public GameObject blockUIPrefab;
        public Transform uiParentTransform;
        
        public HashSet<Item> itemHashSet = new HashSet<Item>();
        public Dictionary<Item, UIHandle> uiHandleDictionary = new Dictionary<Item, UIHandle>();

        // UI를 어떻게 보여주는지에 대한 업데이터 함수
        public Func<UIHandle> UIUpdate;
        
        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public virtual void AddItem(Item item)
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
                item = Instantiate(item.gameObject, canvas.transform).GetComponent<Item>();
                item.gameObject.SetActive(false);
                itemHashSet.Add(item);

                var handle = Instantiate(blockUIPrefab, uiParentTransform).GetComponent<UIHandle>();
                handle.gameObject.SetActive(true);
                
                uiHandleDictionary.Add(item, handle);
                if (handle.TryGetComponent(out IInventoryUIUpdate handleUpdateInterface))
                {
                    handleUpdateInterface.UIUpdateFromItem(item);
                }
            }

            DebugManager.Log($"[{name}] Inventory에 {item.name}을 추가");
        }

        public virtual void UseItem(Item item)
        {
            if (itemHashSet.TryGetValue(item, out var hashItem))
            {
                if (item.TryGetComponent(out IInventoryItemUse itemInterface))
                {
                    itemInterface.UseItem(hashItem, out var isDestroy);
                    DebugManager.Log($"[{name}] Inventory에 {hashItem.name}을 사용");
                    var handle = uiHandleDictionary[hashItem];
                    if (handle.TryGetComponent(out IInventoryUIUpdate handleUpdateInterface))
                    {
                        handleUpdateInterface.UIUpdateFromItem(hashItem);
                    }

                    if (isDestroy)
                    {
                        itemHashSet.Remove(hashItem);
                        uiHandleDictionary.Remove(hashItem);
                    }
                }

            }
        }

        #if UNITY_EDITOR
        #region Editor Function
        public void SetItem(Dictionary<Component, Component> items)
        {
            items.Clear();
            foreach (var (key, value) in uiHandleDictionary)
            {
                items.Add(key, value);
            }
        }

        #endregion
        #endif
    }

#if UNITY_EDITOR

    public interface IInventoryEditor
    {
        public void SetItem(Dictionary<Component, Component> items);
    }
#endif
}