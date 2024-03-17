using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__.Issue_Test
{
    public class PTest : NetworkBehaviour
    {
        private SimpleKCC s;

        private void Awake()
        {
            s = GetComponent<SimpleKCC>();
        }

        public override void FixedUpdateNetwork()
        {
            s.Move(Vector3.zero);
        }
    }
}
