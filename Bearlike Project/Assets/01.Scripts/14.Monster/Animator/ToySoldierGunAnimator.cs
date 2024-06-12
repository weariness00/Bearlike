using System;
using Fusion;
using Photon;
using UnityEngine;

namespace Monster.Container
{
    public class ToySoldierGunAnimator : NetworkBehaviourEx
    {
        // 애니메이터 프로퍼티
        private static readonly int AniMoveSpeed = Animator.StringToHash("f Move Speed");
        private static readonly int AniAttack = Animator.StringToHash("tAttack");
        
        [Header("Animator")]
        [SerializeField] private NetworkMecanimAnimator networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip moveClip;
        [SerializeField] private AnimationClip longAttackClip;
        
        private TickTimer AniIdleTimer { get; set; }
        private TickTimer AniMoveTimer { get; set; }
        private TickTimer AniLongAttackTimer { get; set; }
        
        public bool IdleTimerExpired => AniIdleTimer.Expired(Runner);
        public bool MoveTimerExpired => AniMoveTimer.Expired(Runner);
        public bool LongAttackTimerExpired => AniLongAttackTimer.Expired(Runner);

        public float MoveSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniMoveSpeed);
            set => networkAnimator.Animator.SetFloat(AniMoveSpeed, value);
        }

        private void Awake()
        {
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

        public void PlayLongAttack()
        {
            AniLongAttackTimer = TickTimer.CreateFromSeconds(Runner, longAttackClip.length);
            networkAnimator.SetTrigger(AniAttack);
        }
    }
}