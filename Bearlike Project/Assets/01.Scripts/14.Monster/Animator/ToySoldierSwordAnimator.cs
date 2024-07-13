using Fusion;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class ToySoldierSwordAnimator : NetworkBehaviour
    {
        // 애니메이터 프로퍼티
        private static readonly int AniMoveSpeed = Animator.StringToHash("f Move Speed");
        private static readonly int AniDefaultAttack = Animator.StringToHash("tAttack");
        private static readonly int AniStabbingAttack = Animator.StringToHash("tAttack2");
        private static readonly int AniAttackSpeed = Animator.StringToHash("f Attack Speed");

        private ToySoldierSword toySoldierSword;
        private NetworkMecanimAnimator networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip moveClip;
        [SerializeField] private AnimationClip defaultAttackClip;
        [SerializeField] private AnimationClip stabbingAttackClip;

        [SerializeField] private VisualEffect defaultAttackVFX;
        
        private TickTimer AniIdleTimer { get; set; }
        private TickTimer AniMoveTimer { get; set; }
        private TickTimer AniDefaultAttackTimer { get; set; }
        private TickTimer AniStabbingAttackTimer { get; set; }
        
        public bool IdleTimerExpired => AniIdleTimer.Expired(Runner);
        public bool MoveTimerExpired => AniMoveTimer.Expired(Runner);
        public bool DefaultAttackTimerExpired => AniDefaultAttackTimer.Expired(Runner);
        public bool StabbingAttackTimerExpired => AniStabbingAttackTimer.Expired(Runner);

        public float MoveSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniMoveSpeed);
            set => networkAnimator.Animator.SetFloat(AniMoveSpeed, value);
        }

        public float AttackSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniAttackSpeed);
            set => networkAnimator.Animator.SetFloat(AniAttackSpeed, value);
        }

        private void Awake()
        {
            toySoldierSword = GetComponentInParent<ToySoldierSword>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
        }

        public void PlayIdle()
        {
            AniIdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
            MoveSpeed = 0f;
        }

        public void PlayMove()
        {
            AniMoveTimer = TickTimer.CreateFromSeconds(Runner, moveClip.length);
            MoveSpeed = 1f;
        }

        public void PlayDefaultAttack()
        {
            AniDefaultAttackTimer = TickTimer.CreateFromSeconds(Runner, defaultAttackClip.length / AttackSpeed);
            networkAnimator.SetTrigger(AniDefaultAttack);
        }

        public void PlayStabbingAttack()
        {
            AniStabbingAttackTimer = TickTimer.CreateFromSeconds(Runner, stabbingAttackClip.length / AttackSpeed);
            networkAnimator.SetTrigger(AniStabbingAttack);
        }

        #region Animation Clip Event Function

        private void DefaultAttackEvent()
        {
            toySoldierSword.DefaultAttackEvent();
        }

        private void StabbingAttackEvent()
        {
            toySoldierSword.StabbingAttackEvent();
        }

        #endregion
    }
}