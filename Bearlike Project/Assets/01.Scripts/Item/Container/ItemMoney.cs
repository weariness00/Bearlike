using System;
using System.Collections;
using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class ItemMoney : ItemBase
    {
        public StatusValue<int> moneyAmount = new StatusValue<int>();
        
        #region Unity Event Function

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 360f,0);
        }

        public void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player") && other.gameObject.name == "Local Player")
            {
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion
        
        public override void GetItem(GameObject targetObject)
        {
            base.GetItem(targetObject);
            if (targetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.itemList.TryGetValue(id, out var item))
                {
                    item.amount.Current += item.amount.Current;
                }
                else
                {
                    pc.itemList.Add(id, this);
                }
            }
        }

        public override ItemJsonData GetJsonData()
        {
            var json = base.GetJsonData();
            json.iStatusValueDictionary.Add("MoneyAmount", moneyAmount);
            return json;
        }

        public override void SetJsonData(ItemJsonData json)
        {
            base.SetJsonData(json);
            moneyAmount = json.GetStatusValueInt("MoneyAmount");
        }
    }
}

