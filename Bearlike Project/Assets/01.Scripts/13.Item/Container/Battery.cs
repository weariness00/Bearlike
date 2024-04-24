using System;
using Player;
using UnityEngine;

namespace Item.Container
{
    /// <summary>
    /// 부상 상태에 있는 다른 플레이어를 살릴 수 있는 아이템
    /// </summary>
    public class Battery : ItemBase
    {
        #region Unity Event Functon

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player") &&
                other.gameObject.TryGetComponent(out PlayerController pc) && pc.HasInputAuthority)
            {
                GetItem(other.gameObject);
                Destroy(gameObject);
            }
        }

        #endregion
    }
}