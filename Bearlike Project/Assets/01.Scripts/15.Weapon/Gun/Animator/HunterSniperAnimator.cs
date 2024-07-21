using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Weapon.Gun.Container
{
    public class HunterSniperAnimator : MonoBehaviour
    {
        public GameObject hideMagazineObject;

        [HideInInspector] public Animator animator;
        [SerializeField] private AnimationClip fireClip;
        [SerializeField] private AnimationClip reloadClip;
        [SerializeField] private AnimationClip reloadMagicClip;

        public float FireTime => fireClip.length;
        public float ReloadTime => _reloadType == 0 ? reloadClip.length : reloadMagicClip.length;
        
        private float _reloadType = 0f;
        
        private static readonly int AniFire = Animator.StringToHash("tFire");
        private static readonly int AniFireSpeed = Animator.StringToHash("fFireSpeed");
        private static readonly int AniReload = Animator.StringToHash("tReload"); //장전이 2개인데 fbx 넣으면 좀 더 괜찮을까 싶어서 해봄 1개 지워도 됨
        private static readonly int AniReloadSpeed = Animator.StringToHash("fReloadSpeed"); //장전이 2개인데 fbx 넣으면 좀 더 괜찮을까 싶어서 해봄 1개 지워도 됨
        private static readonly int AniReloadType = Animator.StringToHash("fReloadType"); //장전이 2개인데 fbx 넣으면 좀 더 괜찮을까 싶어서 해봄 1개 지워도 됨
        //다른 총도 변경 가능, 애니메이션 좀 더 강조되게 수정 가능

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        
        #region Animator Parameter Function

        public void PlayReload()
        {
            _reloadType = Random.Range(0, 1);
            animator.SetFloat(AniReloadType, _reloadType);
            animator.SetTrigger(AniReload);
        }

        public void SetReloadSpeed(float speed)
        {
            var realSpeed = speed / ReloadTime;
            animator.SetFloat(AniReloadSpeed, realSpeed);
        }

        public void PlayFire()
        {
            animator.SetTrigger(AniFire);
        }
        
        public void SetFireSpeed(float speed)
        {
            var realSpeed = speed / FireTime;
            animator.SetFloat(AniFireSpeed, realSpeed);
        }

        #endregion
        
        #region Events Function

        void MagazineHide()
        {
            hideMagazineObject.SetActive(false);
            //reload 10~15 frame
        }

        void MagazineVisible()
        {
            hideMagazineObject.SetActive(true);
        }
        
        #endregion
    }
}