using Script.Weapon.Gun;
using State.StateClass.Base;

namespace Weapon.Gun
{
    public class Sniper : GunBase
    {
        public override void Awake()
        {                           
            base.Awake();           
        }                           
                                    
        public override void Start()
        {                           
            base.Start();

            ammo.Max = ammo.Current = 36;
            bulletFirePerMinute = 40;
            
            attack.Max = attack.Current = 50;
            property = (int)CrowdControl.Normality;
            
            BulletInit();
        }
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            base.BulletInit();
            magazine.Max = magazine.Current = 5;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
            
            reloadLateSecond.Max = reloadLateSecond.Current = 2.0f;
            
            attackRange = 150.0f;
            
            bullet.maxMoveDistance = attackRange;
            bullet.player = gameObject;
        }
        
        #endregion
    }
}
