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
        [SerializeField] private AnimationClip tpClip;
        [SerializeField] private AnimationClip MaskChageClip;
        [SerializeField] private AnimationClip DarknessBreathClip;
        [SerializeField] private AnimationClip punchReadyClip;
        // [SerializeField] private AnimationClip cloneClip;
        [SerializeField] private AnimationClip ShieldClip;
        [SerializeField] private AnimationClip ReverseShieldClip;
        // [SerializeField] private AnimationClip hatClip;
        [SerializeField] private AnimationClip handLazerClip;
        [SerializeField] private AnimationClip throwBoomClip;
        [SerializeField] private AnimationClip slapClip;
        
        private static readonly int tIdle = Animator.StringToHash("tIdle");
        private static readonly int tAttack = Animator.StringToHash("tAttack");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int tFaceHide = Animator.StringToHash("tFace Hide");
        private static readonly int FaceHide = Animator.StringToHash("Face Hide");
        private static readonly int tChangeMask = Animator.StringToHash("tChange Mask");
        private static readonly int tSmoke = Animator.StringToHash("tSmoke");
        private static readonly int tDeath = Animator.StringToHash("tDeath");
        
        // private static readonly int Teleport = Animator.StringToHash("tTeleport");
        // private static readonly int Hide = Animator.StringToHash("tHide");
        // private static readonly int Appear = Animator.StringToHash("tAppear");
        // private static readonly int Punch = Animator.StringToHash("tPunch");
        // private static readonly int Shield = Animator.StringToHash("tShield");
        // private static readonly int ReverseShield = Animator.StringToHash("tReverseShield");
        
        private TickTimer IdleTimer { get; set; }
        private TickTimer TeleportTimer { get; set; }
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
            networkAnimator = transform.root.GetComponent<NetworkMecanimAnimator>();
            
            IdleTimer = TickTimer.CreateFromTicks(Runner, 0);
            TeleportTimer = TickTimer.CreateFromTicks(Runner, 0);
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
            // IdleTimer = TickTimer.CreateFromSeconds(Runner, 3);
            IdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length * 2);
            var state = networkAnimator.Animator.GetCurrentAnimatorStateInfo(0);
            if(!state.IsName("Clown_Idle"))
                networkAnimator.SetTrigger(tIdle);
        }

        public void PlayTeleport()
        {
            TeleportTimer = TickTimer.CreateFromSeconds(Runner, 2);
            // TODO : VFX의 시간도 Clip대로 맞춰야함
            // TeleportTimer = TickTimer.CreateFromSeconds(Runner, tpClip.length);
            // networkAnimator.SetTrigger(Teleport);
        }
        
        public void PlaySmokeAttack()
        {
            SmokeTimer = TickTimer.CreateFromSeconds(Runner, 2);    // smoke 공격의 시간으로 설정해야함
            networkAnimator.SetTrigger(tSmoke);
        }
        
        public void PlayMaskChange()
        {
            MaskChangeTimer = TickTimer.CreateFromSeconds(Runner, 1);
            networkAnimator.SetTrigger(tChangeMask);
        }
        
        public void PlayPunchReadyAction()
        {
            PunchReadyTimer = TickTimer.CreateFromSeconds(Runner, punchReadyClip.length);
            networkAnimator.SetTrigger(tAttack);
            networkAnimator.Animator.SetFloat(Attack, 0);
        }
        
        public void PlayPunchAction()
        {
            PunchTimer = TickTimer.CreateFromSeconds(Runner, 3);
        }
        
        public void PlayShieldAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ShieldClip.length);
            networkAnimator.SetTrigger(tFaceHide);
            networkAnimator.Animator.SetFloat(FaceHide, 0);
        }
        
        public void PlayReverseShieldAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ReverseShieldClip.length);
            networkAnimator.SetTrigger(tFaceHide);
            networkAnimator.Animator.SetFloat(FaceHide, 1);
        }
        
        public void PlayShieldOffAction()
        {
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);   // 상수화 해야함
        }
    }
}