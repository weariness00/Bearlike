using Fusion;
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
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void PlayLazerVFXRPC()
        {
            HandLazerEffect.gameObject.SetActive(true);
            HandLazerEffect.SendEvent("OnPlay");
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void StopLazerVFXRPC()
        {
            HandLazerEffect.SendEvent("StopPlay");
            HandLazerEffect.gameObject.SetActive(false);
        }
    }
}