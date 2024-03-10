using Mono.Cecil;
using Script.Weapon.Gun;
using State.StateClass.Base;
using UnityEditor.SceneManagement;
using UnityEngine;
using Weapon.Bullet;

namespace Weapon.Gun
{
    public class Maganum : GunBase
    {
        public override void Awake()
        {
            base.Awake();
        } 
        
        public override void Start()
        {
            base.Start();

            attack.Max = attack.Current = 10;
            property = (int)CrowdControl.Normality;
        } 
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            magazine.Max = magazine.Current = 6;
        }
        
        #endregion
    }
}