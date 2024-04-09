namespace Weapon.Gun
{
    public class Sniper : GunBase
    {
        public override void Awake()
        {                           
            base.Awake();
            
            BulletInit();        
        }                           
                                    
        public override void Start()
        {                           
            base.Start();

            ammo.Max = ammo.Current = 36;
        }
        
        #region Bullet Funtion

        public override void BulletInit()
        {
            bulletFirePerMinute = 40;
            
            magazine.Max = magazine.Current = 5;
            
            fireLateSecond.Max = 60 / bulletFirePerMinute;
            fireLateSecond.Current = float.MaxValue;
            
            reloadLateSecond.Max = reloadLateSecond.Current = 2.0f;
            
            attackRange = 150.0f;
            
            bullet.maxMoveDistance = attackRange;
            // bullet.player = gameObject;
        }
        
        #endregion
    }
}
