using System;
using System.Collections;
using Script.Data;
using Status;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Item
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemBase : MonoBehaviour, IJsonData<ItemJsonData>
    {
        [HideInInspector] public Rigidbody rigidbody;
        
        public static string path = $"{Application.dataPath}/Json/Item/";

        public int id;
        public string itemName;
        
        public Sprite icon; // 아이템 이미지

        public StatusValue<int> amount; // 아이템 총 갯수
        public string explain; // 아이템 설명

        #region Static Method

        public static bool SaveJsonData(ItemJsonData json) => IJsonData<ItemJsonData>.SaveJsonData(json, json.name, path);

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
            StartCoroutine(RiseUp());
        }

        #endregion
        
        // 처음 드랍할때 솟아오르는 동작
        private IEnumerator RiseUp()
        {
            float time = 0f;
            var dir = Random.insideUnitSphere;
            dir.y = 1f;
            dir.Normalize();
            while (true)
            {
                time += Time.deltaTime;
                if (time > 2f)
                    break;

                rigidbody.AddForce(Time.deltaTime * 2f * dir);
                yield return null;
            }
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
            
        }

        #region JsonData Interface

        public virtual ItemJsonData GetJsonData()
        {
            ItemJsonData json = new ItemJsonData();
            json.name = "Item";
            json.amount = amount;
            json.explain = explain;
            return json;
        }

        public virtual void SetJsonData(ItemJsonData json)
        {
            amount.Current = json.amount;
            explain = json.explain;
        }

        #endregion
    }
}

