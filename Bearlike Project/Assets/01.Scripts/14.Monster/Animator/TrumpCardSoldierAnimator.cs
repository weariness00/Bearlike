using System;
using Fusion;
using Photon;
using UnityEngine;

namespace Monster.Container
{
    public class TrumpCardSoldierAnimator : NetworkBehaviourEx
    {
        public TrumpCardSoldier monster;

        private Animator _animator;

        [Header("Aniamtion Clip")] 
        [SerializeField] private AnimationClip jump;

        private TickTimer _jumpTimer;
        
        private static readonly int AniJump = Animator.StringToHash("t Jump");

        public bool JumpTimerExpired => _jumpTimer.Expired(Runner);

        #region Unity Event Function

        private void Awake()
        {
            _animator = monster.networkAnimator.Animator;
        }

        public override void Spawned()
        {
            _jumpTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        #endregion
        
        public void PlayJump()
        {
            _animator.SetTrigger(AniJump);
            _jumpTimer = TickTimer.CreateFromSeconds(Runner, jump.length);
        }
        
        public void AniAttackRayEvent()
        {
            monster.AniAttackRayEvent();
        }
    }
}