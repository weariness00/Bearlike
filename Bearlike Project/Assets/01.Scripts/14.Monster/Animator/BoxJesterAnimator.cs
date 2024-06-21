using Fusion;
using Photon;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAnimator : NetworkBehaviourEx
    {
        [Header("Animator")]
        [SerializeField] private NetworkMecanimAnimator networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        public AnimationClip tptClip;
        
        private static readonly int Teleport = Animator.StringToHash("tTeleport");
        
        private TickTimer IdleTimer { get; set; }
        private TickTimer TeleportTimer { get; set; }
        
        public bool IdleTimerExpired => IdleTimer.Expired(Runner);
        public bool TeleportTimerExpired => IdleTimer.Expired(Runner);
        
        private void Awake()
        {
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            
            IdleTimer = TickTimer.CreateFromTicks(Runner, 0);
            TeleportTimer = TickTimer.CreateFromTicks(Runner, 0);
        }
        
        public void PlayIdle()
        {
            IdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
        }

        public void PlayTeleport()
        {
            TeleportTimer = TickTimer.CreateFromSeconds(Runner, tptClip.length);
            networkAnimator.SetTrigger(Teleport);
        }
    }
}