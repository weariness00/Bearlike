using System;
using Script.Manager;
using UnityEngine;

namespace Script.Weapon.Gun
{
    public class GunBase : MonoBehaviour
    {
        public void Update()
        {
            CheckRay();
        }

        public virtual void Shoot()
        {
            
        }

        public void CheckRay()
        {
            Debug.DrawRay(transform.position, transform.forward * int.MaxValue, Color.red);
            if (Physics.Raycast(transform.position, transform.forward, out var hit))
            {
                DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.collider.name}");
            }
        }
    }
}

