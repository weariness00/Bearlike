using System;
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
            if (other.gameObject.CompareTag("Player") && other.transform.parent.name == "Local Player")
            {
                GetItem(other.gameObject);
                Destroy(gameObject);
            }
        }

        #endregion
    }
}