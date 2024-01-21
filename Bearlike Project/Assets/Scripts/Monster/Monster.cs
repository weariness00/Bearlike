using System;
using Fusion;
using Script.GameStatus;
using Script.Util;
using UnityEngine;

namespace Script.Monster
{
    public class Monster : NetworkBehaviour
    {
        public Status status;

        private void Awake()
        {
            status = ObjectUtil.GetORAddComponet<Status>(gameObject);
        }
    }
}

