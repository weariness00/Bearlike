using System;
using UnityEngine;

namespace Monster.Container
{
    public class DiceAnimator : MonoBehaviour
    {
        public Dice dice;
        [HideInInspector] public Animator animator;

        public AnimationClip attackClip;
        
        public float AttackTime => attackClip.length;

        private static readonly int AniAttack = Animator.StringToHash("tAttack");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayAttack() => animator.SetTrigger(AniAttack);

        #region Event Function

        private void SpearAttack()
        {
            dice.SpearAttack();
        }

        #endregion
    }
}

