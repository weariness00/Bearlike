using System.Linq;
using DG.Tweening;
using Fusion;
using Manager;
using Player;
using UI;
using UnityEngine;

namespace Item.Container
{
    public class Money : ItemBase
    {
        private PlayerController _playerController;
        
        #region Unity Event Function

        public void Update()
        {
            transform.Rotate(0, Time.deltaTime * 90f,0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CheckPlayer(other.gameObject, out PlayerController pc))
            {
                _playerController = pc;
                ;
                rigidbody.isKinematic = true;
                
                foreach (var sphereCollider in GetComponents<SphereCollider>())
                {
                    Destroy(sphereCollider);
                }

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion

        #region Inventory Interface

        public override AddItem AddItem<AddItem>(AddItem addItem)
        {
            base.AddItem(addItem);

            var goodsCanvas = gameObject.GetComponentInParent<PlayerController>().uiController.goodsCanvas;
            goodsCanvas.BearCoinUpdate(Amount.Current);
            
            return addItem;
        }

        public override ItemBase UseItem<UseItem>(UseItem useItem, out bool isDestroy)
        {
            base.UseItem(useItem, out isDestroy);
            
            var goodsCanvas = gameObject.GetComponentInParent<PlayerController>().uiController.goodsCanvas;
            goodsCanvas.BearCoinUpdate(Amount.Current);

            return this;
        }

        #endregion
    }
}

