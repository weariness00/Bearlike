using System;
using System.Collections;
using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class TrumpCardSoldierAnimator : NetworkBehaviourEx
    {
        public TrumpCardSoldier trumpCardSoldier;

        private NetworkMecanimAnimator networkAnimator;

        [Header("Aniamtion Clip")] 
        [SerializeField] private AnimationClip jump;

        [Header("VFX")] 
        [SerializeField] private VisualEffect stabbingVFX;

        [Header("ETC Component")] 
        [SerializeField] private Transform stabbingVFXTransform;
        
        
        private TickTimer _jumpTimer;
        
        private static readonly int AniJump = Animator.StringToHash("t Jump");

        public bool JumpTimerExpired => _jumpTimer.Expired(Runner);

        #region Unity Event Function

        private void Awake()
        {
            networkAnimator = trumpCardSoldier.networkAnimator; 
        }

        public override void Spawned()
        {
            _jumpTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        #endregion
        
        public void PlayJump()
        {
            networkAnimator.SetTrigger(AniJump);
            _jumpTimer = TickTimer.CreateFromSeconds(Runner, jump.length);
        }

        #region Animation Clip Event Function

        private void AttackStartEvent()
        {
            StartCoroutine(AttackVFXCoroutine());
        }

        private IEnumerator AttackVFXCoroutine()
        {
            var frame = 0.1f / trumpCardSoldier.status.attackSpeed.Current;
            
            stabbingVFX.SetFloat("Speed", frame);
            stabbingVFX.Play();
            while (frame > 0)
            {
                frame -= Time.deltaTime;
                
                stabbingVFX.transform.position = stabbingVFXTransform.position;
                stabbingVFX.transform.rotation = stabbingVFXTransform.rotation;
                    
                yield return null;
            }
        }
        
        private void AttackEndEvent()
        {
            StopCoroutine(AttackVFXCoroutine());

            trumpCardSoldier.AniAttackRayEvent();
        }
        
        #endregion
    }
}