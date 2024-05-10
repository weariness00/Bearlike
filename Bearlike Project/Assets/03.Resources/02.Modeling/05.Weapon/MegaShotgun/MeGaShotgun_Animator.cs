using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class MegaShotGun : MonoBehaviour
    {
        public GameObject hideBulletObject;
        private static readonly int AniFire = Animator.StringToHash("tFire");
        private static readonly int AniReload = Animator.StringToHash("tReloadStart"); //0 -> 1(n번) -> 2

        void BulletHide()
        {
            hideBulletObject.SetActive(false);
            //Relaod3 10frame~
        }

        void BulletVisible()
        {
            hideBulletObject.SetActive(true);
        }
    }
}