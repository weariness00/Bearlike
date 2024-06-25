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
        [SerializeField] private AnimationClip tptClip;
        [SerializeField] private AnimationClip hideClip;
        [SerializeField] private AnimationClip appearClip;
        [SerializeField] private AnimationClip punchReadyClip;
        // [SerializeField] private AnimationClip cloneClip;
        [SerializeField] private AnimationClip ShieldClip;
        [SerializeField] private AnimationClip ReverseShieldClip;
        // [SerializeField] private AnimationClip hatClip;
        [SerializeField] private AnimationClip handLazerClip;
        [SerializeField] private AnimationClip throwBoomClip;
        [SerializeField] private AnimationClip slapClip;
        
        private static readonly int Teleport = Animator.StringToHash("tTeleport");
        private static readonly int Hide = Animator.StringToHash("tHide");
        private static readonly int Appear = Animator.StringToHash("tAppear");
        private static readonly int Punch = Animator.StringToHash("tPunch");
        private static readonly int Shield = Animator.StringToHash("tShield");
        private static readonly int ReverseShield = Animator.StringToHash("tReverseShield");
        
        private TickTimer IdleTimer { get; set; }
        private TickTimer TeleportTimer { get; set; }
        private TickTimer HideTimer { get; set; }
        private TickTimer AppearTimer { get; set; }
        private TickTimer SmokeTimer { get; set; }
        private TickTimer MaskChangeTimer { get; set; }
        private TickTimer PunchReadyTimer { get; set; }
        private TickTimer PunchTimer { get; set; }
        // private TickTimer CloneTimer { get; set; }
        private TickTimer ShieldTimer { get; set; }
        // private TickTimer HatTimer { get; set; }
        private TickTimer HandLazerTimer { get; set; }
        private TickTimer ThrowBoomTimer { get; set; }
        private TickTimer SlapTimer { get; set; }
        
        public bool IdleTimerExpired => IdleTimer.Expired(Runner);
        public bool TeleportTimerExpired => TeleportTimer.Expired(Runner);
        public bool HideTimerExpired => HideTimer.Expired(Runner);
        public bool AppearTimerExpired => AppearTimer.Expired(Runner);
        public bool SmokeTimerExpired => SmokeTimer.Expired(Runner);
        public bool MaskChangeTimerExpired => MaskChangeTimer.Expired(Runner);
        public bool PunchReadyTimerExpired => PunchReadyTimer.Expired(Runner);
        public bool PunchTimerExpired => PunchTimer.Expired(Runner);
        // public bool CloneTimerExpired => CloneTimer.Expired(Runner);
        public bool ShieldTimerExpired => ShieldTimer.Expired(Runner);
        // public bool HatTimerExpired => HatTimer.Expired(Runner);
        public bool HandLazerTimerExpired => HandLazerTimer.Expired(Runner);
        public bool ThrowBoomTimerExpired => ThrowBoomTimer.Expired(Runner);
        public bool SlapTimerExpired => SlapTimer.Expired(Runner);
        
        private void Awake()
        {
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            
            IdleTimer = TickTimer.CreateFromTicks(Runner, 0);
            TeleportTimer = TickTimer.CreateFromTicks(Runner, 0);
            HideTimer = TickTimer.CreateFromTicks(Runner, 0);
            AppearTimer = TickTimer.CreateFromTicks(Runner, 0);
            SmokeTimer = TickTimer.CreateFromTicks(Runner, 0);
            MaskChangeTimer = TickTimer.CreateFromTicks(Runner, 0);
            PunchReadyTimer = TickTimer.CreateFromTicks(Runner, 0);
            PunchTimer = TickTimer.CreateFromTicks(Runner, 0);
            // CloneTimer = TickTimer.CreateFromTicks(Runner, 0);
            ShieldTimer = TickTimer.CreateFromTicks(Runner, 0);
            // HatTimer = TickTimer.CreateFromTicks(Runner, 0);
            HandLazerTimer = TickTimer.CreateFromTicks(Runner, 0);
            ThrowBoomTimer = TickTimer.CreateFromTicks(Runner, 0);
            SlapTimer = TickTimer.CreateFromTicks(Runner, 0);
        }
        
        public void PlayIdle()
        {
            IdleTimer = TickTimer.CreateFromSeconds(Runner, 3);
            // IdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
        }

        public void PlayTeleport()
        {
            TeleportTimer = TickTimer.CreateFromSeconds(Runner, 2);
            // TODO : VFX의 시간도 Clip대로 맞춰야함
            // TeleportTimer = TickTimer.CreateFromSeconds(Runner, tptClip.length);
            // networkAnimator.SetTrigger(Teleport);
        }

        public void PlayHideInBox()
        {
            HideTimer = TickTimer.CreateFromSeconds(Runner, 2);
            // HideTimer = TickTimer.CreateFromSeconds(Runner, hideClip.length);
            // networkAnimator.SetTrigger(Hide);
        }
        
        public void PlayAppearInBox()
        {
            AppearTimer = TickTimer.CreateFromSeconds(Runner, 2);
            // AppearTimer = TickTimer.CreateFromSeconds(Runner, appearClip.length);
            // networkAnimator.SetTrigger(Appear);
        }
        
        public void PlaySmokeAttack()
        {
            SmokeTimer = TickTimer.CreateFromSeconds(Runner, 2);    // smoke 공격의 시간으로 설정해야함
            // Box의 뚜껑이 열리는 Animation이 있었으면 좋겠음
        }
        
        public void PlayMaskChange()
        {
            MaskChangeTimer = TickTimer.CreateFromSeconds(Runner, 1);
            // Box가 흔들리는 Animation이 있었으면 좋겠음
        }
        
        public void PlayPunchReadyAction()
        {
            PunchReadyTimer = TickTimer.CreateFromSeconds(Runner, 1);
            // PunchReadyTimer = TickTimer.CreateFromSeconds(Runner, punchReadyClip.length);
            // networkAnimator.SetTrigger(Punch);
        }
        
        public void PlayPunchAction()
        {
            PunchTimer = TickTimer.CreateFromSeconds(Runner, 3);
        }
        
        public void PlayShieldAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ShieldClip.length);
            // networkAnimator.SetTrigger(Shield);
        }
        
        public void PlayReverseShieldAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ReverseShieldClip.length);
            // networkAnimator.SetTrigger(ReverseShield);
        }
        
        public void PlayShieldOffAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);   // 상수화 해야함
        }
    }
}