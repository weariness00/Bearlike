using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class MegaShotGun_Animator : MonoBehaviour
    {
        public GameObject hideBulletObject;
        private static readonly int AniShoot = Animator.StringToHash("tShoot");
        private static readonly int AniReload = Animator.StringToHash("fReload"); //0 -> 1(n번) -> 2

        void BulletHide()
        {
            hideBulletObject.SetActive(false);
            //Relaod3 10frame~
        }
    }
}