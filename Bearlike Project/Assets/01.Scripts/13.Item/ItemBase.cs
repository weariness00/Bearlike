using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Inventory;
using Player;
using Status;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Item
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemBase : MonoBehaviour, IJsonData<StatusJsonData>, IInventoryItemAdd, IInventoryItemUse
    {
        #region Static

        // Item Info Data 캐싱
        private static Dictionary<int, ItemJsonData> _itemInfoDataCash = new Dictionary<int, ItemJsonData>();
        public static void AddInfoData(int id, ItemJsonData data) => _itemInfoDataCash.TryAdd(id, data);
        public static ItemJsonData GetInfoData(int id) => _itemInfoDataCash.TryGetValue(id, out var data) ? data : new ItemJsonData();
        public static void ClearInfosData() => _itemInfoDataCash.Clear();
        
        // Item Status Data 캐싱
        private static Dictionary<int, StatusJsonData> _itemStatusDataCash = new Dictionary<int, StatusJsonData>();
        public static void AddStatusData(int id, StatusJsonData data) => _itemStatusDataCash.TryAdd(id, data);
        public static StatusJsonData GetStatusData(int id) => _itemStatusDataCash.TryGetValue(id, out var data) ? data : new StatusJsonData();
        public static void ClearStatusData() => _itemStatusDataCash.Clear();

        #endregion
        
        [HideInInspector] public Rigidbody rigidbody;

        #region Info Parameter
        
        public ItemInfo info;
        
        public int Id => info.id;
        public string Name => info.name;
        public Texture2D Icon => info.icon;
        public StatusValue<int> Amount => info.amount;
        public string Explain => info.explain;

        #endregion
        
        #region HashSet Fucntion

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ItemBase itemBase = (ItemBase)obj;
            return info.Equals(itemBase.info);
        }

        public override int GetHashCode()
        {
            return info.GetHashCode();
        }
        #endregion

        #region Unity Event Function

        public virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();

            gameObject.layer = LayerMask.NameToLayer("Item");
            tag = "Item";

            info.SetJsonData(GetInfoData(Id));
            SetJsonData(GetStatusData(Id));
        }

        public virtual void Start()
        {
            RiseUp();
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(MoveTargetCoroutine));
        }

        #endregion
        
        // 처음 드랍할때 솟아오르는 동작
        private void RiseUp()
        {
            var dir = Random.insideUnitSphere;
            dir.y = 1f;
            dir.Normalize();
            dir = rigidbody.mass * 1000f * dir;
            rigidbody.AddForce(dir);
        }
        
        // 타겟을 향해 이동하는 코루틴
        protected IEnumerator MoveTargetCoroutine(GameObject targetObject)
        {
            Transform targetTransform = targetObject.transform;
            while (true)
            {
                var dis = Vector3.Distance(targetTransform.position, transform.position);
                if (dis < 0.1f)
                {
                    break;
                }
                else
                {
                    var dir = targetTransform.position - transform.position;
                    dir = dir.normalized;
                    transform.position += Time.deltaTime * 10f * dir;
                }
                yield return null;
            }
            
            GetItem(targetObject);
            Destroy(gameObject);
        }
        
        public virtual void GetItem(GameObject targetObject)
        {
            PlayerController pc;
            if (targetObject.TryGetComponent(out pc) || targetObject.transform.root.TryGetComponent(out pc))
            {
                pc.itemInventory.AddItem(this);
            }
        }

        #region JsonData Interface

        public virtual StatusJsonData GetJsonData()
        {
            var data = new StatusJsonData
            {
                ID = Id
            };
            return data;
        }

        public virtual void SetJsonData(StatusJsonData json)
        {
            
        }

        #endregion

        #region Inventory Function

        public virtual AddItem AddItem<AddItem>(AddItem addItem)
        {
            if (addItem is ItemBase itemBase)
            {
                Amount.Current += itemBase.Amount.Current;
            }

            return addItem;
        }
        
        public virtual UseItem UseItem<UseItem>(UseItem useItem, out bool isDestroy)
        {
            isDestroy = false;
            if (useItem is ItemBase testItem)
            {
                testItem.info.amount.Current -= 1;

                if (testItem.info.amount.isMin)
                {
                    Destroy(testItem.gameObject);
                    isDestroy = true;
                }
            }

            return useItem;
        }

        #endregion
    }
}

