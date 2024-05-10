using System;
using UnityEngine;

namespace Weapon.Gun.Continer
{
    public class MegaShotGunAnimator : MonoBehaviour
    {
        [HideInInspector] public Animator animator;
        public GameObject hideBulletObject;

        [SerializeField] private AnimationClip reloadStartClip;
        [SerializeField] private AnimationClip reloadRunningClip;
        [SerializeField] private AnimationClip reloadEndClip;

        public float ReloadStartTime => reloadStartClip.length;
        public float ReloadRunningTime => reloadRunningClip.length;
        public float ReloadEndTime => reloadEndClip.length;
        
        private static readonly int AniFire = Animator.StringToHash("tFire");
        private static readonly int AniFireSpeed = Animator.StringToHash("fFireSpeed");
        private static readonly int AniReloadStart = Animator.StringToHash("tReloadStart");
        private static readonly int AniReloadEnd = Animator.StringToHash("tReloadEnd");
        private static readonly int AniReloadSpeed = Animator.StringToHash("fReloadSpeed");
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void BulletHide()
        {
            hideBulletObject.SetActive(false);
            //Relaod3 10frame~
        }

        void BulletVisible()
        {
            hideBulletObject.SetActive(true);
        }

        #region Animator Parametar Function
        
        
        public void PlayFireBullet() => animator.SetTrigger(AniFire);
        public void SetFireSpeed(float speed) => animator.SetFloat(AniFireSpeed, speed);
        // 0 Start, 1 Running, 2 End
        public void PlayReloadStart() => animator.SetTrigger(AniReloadStart);
        public void PlayReloadEnd() => animator.SetTrigger(AniReloadEnd);
        public void SetReloadSpeed(float speed)
        {
            float realSpeed = speed / ReloadRunningTime;
            animator.SetFloat(AniReloadSpeed, realSpeed);
        }

        #endregion
    }
}

