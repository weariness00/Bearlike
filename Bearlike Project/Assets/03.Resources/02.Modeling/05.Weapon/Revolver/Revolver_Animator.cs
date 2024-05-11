using UnityEditor.Animations;
using UnityEngine;

namespace Test
{
    public class Revolver_Animator : MonoBehaviour
    {
        public GameObject hideBulletObject;
        private static readonly int AniFire = Animator.StringToHash("isFire");
        private static readonly int AniReload = Animator.StringToHash("isReload");
        
        void BulletHide()
        {
            hideBulletObject.SetActive(false);
            //34~46 frame
        }

        void BulletVisible()
        {
            hideBulletObject.SetActive(true);
        }
    }
}