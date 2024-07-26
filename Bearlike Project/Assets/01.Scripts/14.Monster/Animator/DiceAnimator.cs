using System;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Monster.Container
{
    public class DiceAnimator : MonoBehaviour
    {
        public Dice dice;
        [HideInInspector] public NetworkMecanimAnimator networkAnimator;

        public AnimationClip attackClip;
        
        public float AttackTime => attackClip.length;

        private static readonly int AniAttack = Animator.StringToHash("tAttack");

        private void Awake()
        {
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
        }

        public void PlayAttack() => networkAnimator.SetTrigger(AniAttack);

        #region Event Function

        private void SpearAttack()
        {
            dice.SpearAttack();
        }

        #endregion
    }
}

