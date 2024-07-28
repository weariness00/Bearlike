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

            if (addItem is Money m)
            {
                var goodsCanvas = m._playerController.uiController.goodsCanvas;
                goodsCanvas.BearCoinUpdate(Amount.Current);
            }
            
            return addItem;
        }

        public override ItemBase UseItem<UseItem>(UseItem useItem, out bool isDestroy)
        {
            isDestroy = false;
            if (useItem is ItemBase item)
            {
                Amount.Current -= item.Amount.Current;
                
                if (useItem is Money m)
                {
                    var goodsCanvas = m._playerController.uiController.goodsCanvas;
                    goodsCanvas.BearCoinUpdate(Amount.Current);
                }

                if (Amount.isMin)
                {
                    Destroy(gameObject);
                    isDestroy = true;
                }
            }

            return this;
        }

        #endregion
    }
}

