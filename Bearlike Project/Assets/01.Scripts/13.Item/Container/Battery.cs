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

        private void OnTriggerEnter(Collider other)
        {
            if (CheckPlayer(other.gameObject, out PlayerController pc))
            {
                foreach (var sphereCollider in GetComponents<Collider>())
                    Destroy(sphereCollider);

                StartCoroutine(MoveTargetCoroutine(other.gameObject));
            }
        }

        #endregion
    }
}