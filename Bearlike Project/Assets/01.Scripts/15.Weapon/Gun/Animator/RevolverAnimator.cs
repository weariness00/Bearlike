using System;
using UnityEngine;

namespace Weapon.Gun.Continer
{
    public class RevolverAnimator : MonoBehaviour
    {
        public GameObject hideBulletObject;

        [HideInInspector] public Animator animator;

        [SerializeField] private AnimationClip fireClip;
        [SerializeField] private AnimationClip reloadClip;

        public float FireTime => fireClip.length;
        public float ReloadTime => reloadClip.length;
        
        private static readonly int AniFire = Animator.StringToHash("tFire");
        private static readonly int AniFireSpeed = Animator.StringToHash("fFireSpeed");
        private static readonly int AniReload = Animator.StringToHash("tReload");
        private static readonly int AniReloadSpeed = Animator.StringToHash("fReloadSpeed");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        #region Animator Parameter Function

        public void PlayFire() => animator.SetTrigger(AniFire);

        public void SetFireSpeed(float speed)
        {
            var realSpeed = speed / FireTime;
            animator.SetFloat(AniFireSpeed, realSpeed);
        }
        
        public void PlayReload() => animator.SetTrigger(AniReload);

        public void SetReloadSpeed(float speed)
        {
            var realSpeed = speed / ReloadTime;
            animator.SetFloat(AniReloadSpeed, realSpeed);
        }

        #endregion
        
        #region Clip Events Function

        void BulletHide()
        {
            hideBulletObject.SetActive(false);
            //34~46 frame
        }

        void BulletVisible()
        {
            hideBulletObject.SetActive(true);
        }

        #endregion
    }
}

