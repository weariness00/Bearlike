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

            // shootSound = transform.GetChild(0).GetComponent<AudioSource>();
            // reloadSound = transform.GetChild(1).GetComponent<AudioSource>();
            // emptyAmmoSound = transform.GetChild(2).GetComponent<AudioSource>();
            
            attack.Max = attack.Current = 10;
            property = (int)CrowdControl.Normality;
        } 
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            ammo.Max = ammo.Current = 48;
            
            magazine.Max = magazine.Current = 6;

            bulletFirePerMinute = 6000;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
        }
        
        #endregion
    }
}