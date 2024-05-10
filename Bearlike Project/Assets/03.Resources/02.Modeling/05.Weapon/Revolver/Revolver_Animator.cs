using UnityEngine;

namespace Test
{
    public class Revolver_Animator : MonoBehaviour
    {
        public GameObject HideBulletObject;
        private static readonly int AniShoot = Animator.StringToHash("isShoot");
        private static readonly int AniReload = Animator.StringToHash("isReload");
        
        void BulletHide()
        {
            HideBulletObject.SetActive(false);
            //34~46 frame
        }
    }
}