using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class HunterSniper : MonoBehaviour
    {
        public GameObject hideMagazineObject;
        private static readonly int AniFire = Animator.StringToHash("tFire");
        private static readonly int AniReload = Animator.StringToHash("fReload"); //장전이 2개인데 fbx 넣으면 좀 더 괜찮을까 싶어서 해봄 1개 지워도 됨
        //다른 총도 변경 가능, 애니메이션 좀 더 강조되게 수정 가능

        void BulletHide()
        {
            hideMagazineObject.SetActive(false);
            //reload 10~15 frame
        }

        void BulletVisible()
        {
            hideMagazineObject.SetActive(true);
        }
    }
}
