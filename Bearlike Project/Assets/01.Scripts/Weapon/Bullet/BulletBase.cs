﻿using System;
using Fusion;
using Scripts.State.GameStatus;
using UnityEngine;
using Util;

namespace Weapon.Bullet
{
    public class BulletBase : MonoBehaviour
    {
        public StatusValue<int> speed = new StatusValue<int>(){Max = 100, Current = 100};
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

        private void OnCollisionEnter(Collision other)
        {
            // if (other.gameObject.CompareTag("Destruction"))
            // {
            //     MeshDestruction.Destruction(other.gameObject, PrimitiveType.Cube, other.contacts[0].point, Vector3.one);
            // }
            Destroy(gameObject);
        }
    }
}