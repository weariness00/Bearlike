using Fusion;
using Manager;
using Photon;
using Status;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAnimator : NetworkBehaviourEx
    {
        [Header("Object")]
        public BoxJester boxJester;

        private NetworkMecanimAnimator _networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        public AnimationClip tpClip;
        [SerializeField] private AnimationClip MaskChageClip;
        [SerializeField] private AnimationClip DarknessBreathStartClip;
        [SerializeField] private AnimationClip DarknessBreathEndClip;
        [SerializeField] private AnimationClip punchReadyClip;
        // [SerializeField] private AnimationClip cloneClip;
        [SerializeField] private AnimationClip ShieldClip;
        [SerializeField] private AnimationClip ReverseShieldClip;
        [SerializeField] private AnimationClip handLazerClip;
        [SerializeField] private AnimationClip throwBoomClip;
        
        private static readonly int tIdle = Animator.StringToHash("tIdle");
        private static readonly int tAttack = Animator.StringToHash("tAttack");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int tFaceHide = Animator.StringToHash("tFace Hide");
        private static readonly int FaceHide = Animator.StringToHash("Face Hide");
        private static readonly int tHat = Animator.StringToHash("tHat");
        private static readonly int tChangeMask = Animator.StringToHash("tChangeFace");
        private static readonly int tSmoke = Animator.StringToHash("tSmoke");
        private static readonly int tSmokeEnd = Animator.StringToHash("tSmokeEnd");
        private static readonly int tTeleport = Animator.StringToHash("tTeleport");
        private static readonly int tDeath = Animator.StringToHash("tDeath");
        
        private TickTimer IdleTimer { get; set; }
        private TickTimer TeleportTimer { get; set; }
        private TickTimer SmokeStartTimer { get; set; }
        private TickTimer SmokingTimer { get; set; }
        private TickTimer SmokeEndTimer { get; set; }
        private TickTimer MaskChangeTimer { get; set; }
        private TickTimer PunchReadyTimer { get; set; }
        private TickTimer PunchTimer { get; set; }
        private TickTimer CloneTimer { get; set; }
        private TickTimer ShieldTimer { get; set; }
        private TickTimer HatTimer { get; set; }
        private TickTimer HandLazerTimer { get; set; }
        private TickTimer ThrowBoomTimer { get; set; }
        private TickTimer SlapTimer { get; set; }
        
        public bool IdleTimerExpired => IdleTimer.Expired(Runner);
        public bool TeleportTimerExpired => TeleportTimer.Expired(Runner);
        public bool SmokeStartTimerExpired => SmokeStartTimer.Expired(Runner);
        public bool SmokingTimerExpired => SmokingTimer.Expired(Runner);
        public bool SmokeEndTimerExpired => SmokeEndTimer.Expired(Runner);
        public bool MaskChangeTimerExpired => MaskChangeTimer.Expired(Runner);
        public bool PunchReadyTimerExpired => PunchReadyTimer.Expired(Runner);
        public bool PunchTimerExpired => PunchTimer.Expired(Runner);
        public bool CloneTimerExpired => CloneTimer.Expired(Runner);
        public bool ShieldTimerExpired => ShieldTimer.Expired(Runner);
        public bool HatTimerExpired => HatTimer.Expired(Runner);
        public bool HandLazerTimerExpired => HandLazerTimer.Expired(Runner);
        public bool ThrowBoomTimerExpired => ThrowBoomTimer.Expired(Runner);
        public bool SlapTimerExpired => SlapTimer.Expired(Runner);
        
        private void Awake()
        {
            _networkAnimator = transform.root.GetComponent<NetworkMecanimAnimator>();
        }
        
        public void PlayIdle()
        {
            IdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length * 2);
            
            _networkAnimator.Animator.SetBool("bTestIdle", true);
            // var state = _networkAnimator.Animator.GetCurrentAnimatorStateInfo(0);
            //
            // if(!state.IsName("Clown_Idle_Hand"))
            //     _networkAnimator.SetTrigger(tIdle);
        }

        public void OffIdle()
        {
            _networkAnimator.Animator.SetBool("bTestIdle", false);
        }
        
        public void PlayTeleport()
        {
            OffIdle();
            TeleportTimer = TickTimer.CreateFromSeconds(Runner, tpClip.length);
            _networkAnimator.SetTrigger(tTeleport);
        }
        
        public void PlaySmokeStartAttack()
        {
            OffIdle();
            SmokeStartTimer = TickTimer.CreateFromSeconds(Runner, DarknessBreathStartClip.length);
            _networkAnimator.SetTrigger(tSmoke);
        }
        
        public void PlaySmokingAttack()
        {
            OffIdle();
            SmokingTimer = TickTimer.CreateFromSeconds(Runner, 2);    // 상수화 필요
        }
        
        public void PlaySmokeEndAttack()
        {
            OffIdle();
            SmokeEndTimer = TickTimer.CreateFromSeconds(Runner, DarknessBreathEndClip.length);
            _networkAnimator.SetTrigger(tSmokeEnd);
        }
        
        public void PlayMaskChange()
        {
            OffIdle();
            MaskChangeTimer = TickTimer.CreateFromSeconds(Runner, MaskChageClip.length);
            _networkAnimator.SetTrigger(tChangeMask);
        }
        
        public void PlayPunchReadyAction()
        {
            OffIdle();
            PunchReadyTimer = TickTimer.CreateFromSeconds(Runner, punchReadyClip.length);
            _networkAnimator.Animator.SetFloat(Attack, 0);
            _networkAnimator.SetTrigger(tAttack);
        }
        
        public void PlayPunchAction()
        {
            OffIdle();
            PunchTimer = TickTimer.CreateFromSeconds(Runner, 3);
            _networkAnimator.Animator.SetFloat(Attack, 1);
            _networkAnimator.SetTrigger(tAttack);
        }
        
        public void PlayShieldAction()
        {
            OffIdle();
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ShieldClip.length);
            _networkAnimator.Animator.SetFloat(FaceHide, 0);
            _networkAnimator.SetTrigger(tFaceHide);
        }
        
        public void PlayReverseShieldAction()
        {
            OffIdle();
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 7);   // 상수화 해야함
            // ShieldTimer = TickTimer.CreateFromSeconds(Runner, ReverseShieldClip.length);
            _networkAnimator.Animator.SetFloat(FaceHide, 1);
            _networkAnimator.SetTrigger(tFaceHide);
        }
        
        public void PlayShieldOffAction()
        {
            OffIdle();
            ShieldTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);   // 상수화 해야함
        }

        public void PlayHatAction()
        {
            OffIdle();
            HatTimer = TickTimer.CreateFromSeconds(Runner, 10.0f);  // 상수화 필요
            _networkAnimator.SetTrigger(tHat);
        }

        public void PlayHandLazerAction()
        {
            OffIdle();
            HandLazerTimer = TickTimer.CreateFromSeconds(Runner, handLazerClip.length);
            _networkAnimator.Animator.SetFloat(Attack, 2);
            _networkAnimator.SetTrigger(tAttack);
        }

        public void PlayThrowBoomAction()
        {
            OffIdle();
            ThrowBoomTimer = TickTimer.CreateFromSeconds(Runner, throwBoomClip.length);
            _networkAnimator.Animator.SetFloat(Attack, 3);
            _networkAnimator.SetTrigger(tAttack);
        }
        
        public void PlaySlapAction()
        {
            OffIdle();
            SlapTimer = TickTimer.CreateFromSeconds(Runner, 4.5f);
        }

        public void PlayCloneAction()
        {
            OffIdle();
            CloneTimer = TickTimer.CreateFromSeconds(Runner, tpClip.length);
            _networkAnimator.SetTrigger(tTeleport);
        }

        public void PlayDieAction()
        {
            OffIdle();
            _networkAnimator.SetTrigger(tDeath);
        }
    }
}