using System;
using Status;
using UnityEngine;

namespace GamePlay
{
    /// <summary>
    /// 하위 객체마다 데미지가 다름
    /// 예를 들면 머리는 무조건 치명타
    /// </summary>
    [RequireComponent(typeof(StatusBase))]
    public class ColliderStatus : MonoBehaviour
    {
        public StatusBase originalStatus; // 부모가 가지고 있는 것
        public StatusBase status; // 자신이 가지고 있는 것

        private void Awake()
        {
            if (!originalStatus) originalStatus = GetComponentInParent<MonsterStatus>();
            status = GetComponent<StatusBase>();
        }
    }
}

