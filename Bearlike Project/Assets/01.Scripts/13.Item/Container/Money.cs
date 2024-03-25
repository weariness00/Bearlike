using System;
using System.Collections;
using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class Money : ItemBase
    {
        public StatusValue<int> moneyAmount = new StatusValue<int>();
        
        #region Unity Event Function

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 360f,0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && other.transform.parent.name == "Local Player")
            {
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion

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

