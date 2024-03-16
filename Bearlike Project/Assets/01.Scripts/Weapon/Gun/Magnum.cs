using Script.Weapon.Gun;
using State.StateClass.Base;

namespace Weapon.Gun
{
    public class Magnum : GunBase
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
            base.BulletInit();
            magazine.Max = magazine.Current = 6;
        }
        
        #endregion
    }
}