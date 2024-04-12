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
            // json화
            ammo.Max = ammo.Current = 36;
        }
    }
}
