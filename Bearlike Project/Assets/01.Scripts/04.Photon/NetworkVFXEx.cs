using System;
using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.VFX;

namespace _04.Photon
{
    public class NetworkVFXEx : NetworkBehaviourEx
    {
        public VisualEffect vfx;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetFloatRPC(string paraName, float value) => vfx.SetFloat(paraName, value);
    }
}