using System.Collections;
using Data;
using Inventory;
using Player;
using Status;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Item
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemBase : MonoBehaviour, IJsonData<ItemJsonData>, IInventoryItemAdd, IInventoryItemUse
    {
        #region Static
        
        public static string path = $"{Application.dataPath}/Json/Item/";
        
        public static bool SaveJsonData(ItemJsonData json) => IJsonData<ItemJsonData>.SaveJsonData(json, json.name, path);

        #endregion
        
        [HideInInspector] public Rigidbody rigidbody;
        public ItemInfo info;

        #region Info Parameter

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

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();

            gameObject.layer = LayerMask.NameToLayer("Item");
            tag = "Item";
        }

        public virtual void Start()
        {
            RiseUp();
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

        public virtual ItemJsonData GetJsonData()
        {
            var json = info.GetJsonData();
            return json;
        }

        public virtual void SetJsonData(ItemJsonData json)
        {
            info.SetJsonData(json);
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

