﻿using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class BoxJesterAnimationVFX : NetworkBehaviourEx
    {
        [Header("VFX Properties")]
        [SerializeField] private VisualEffect tpEffect;
        [SerializeField] private VisualEffect darknessAttackEffect;
        [SerializeField] private VisualEffect HandLazerEffect;

        private void Awake()
        {
            StopLazerVFXRPC();
            StopTPVFXRPC();
            StopBreathVFXRPC();
            
            tpEffect.SetFloat("Time", 1.0f);
            darknessAttackEffect.SetFloat("Time", 1.0f);
        }
        
        // [Rpc(RpcSources.All, RpcTargets.All)]
        public void StartTPVFXRPC()
        {
            tpEffect.gameObject.SetActive(true);
            tpEffect.SendEvent("OnPlay");
        }
        
        public void StopTPVFXRPC()
        {
            tpEffect.SendEvent("StopPlay");
            tpEffect.gameObject.SetActive(false);
        }

        // [Rpc(RpcSources.All, RpcTargets.All)]
        public void PlayLazerVFXRPC()
        {
            HandLazerEffect.gameObject.SetActive(true);
            HandLazerEffect.SendEvent("OnPlay");
        }
        
        // [Rpc(RpcSources.All, RpcTargets.All)]
        public void StopLazerVFXRPC()
        {
            HandLazerEffect.SendEvent("StopPlay");
            HandLazerEffect.gameObject.SetActive(false);
        }
        
        // [Rpc(RpcSources.All, RpcTargets.All)]
        public void StartBreathVFXRPC()
        {
            darknessAttackEffect.gameObject.SetActive(true);
            darknessAttackEffect.SendEvent("OnPlay");
        }
        
        public void StopBreathVFXRPC()
        {
            darknessAttackEffect.SendEvent("StopPlay");
            darknessAttackEffect.gameObject.SetActive(false);
        }
    }
}