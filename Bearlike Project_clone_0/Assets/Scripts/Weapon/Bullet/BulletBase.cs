using System;
using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;

namespace Weapon.Bullet
{
    public class BulletBase : MonoBehaviour
    {
        public StatusValue<int> speed = new StatusValue<int>(){Max = 900, Current = 900};
        public Vector3 destination = Vector3.zero;

        protected void Start()
        {
            transform.LookAt(destination);
            Destroy(gameObject, 5f);
        }

        protected void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}