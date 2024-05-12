using Player;
using Status;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 총기 오작동
    /// 1발을 쏘면 1발이 더 나간다 추가적인 탄환 소모는 없다.
    /// </summary>
    public class GunMalfunction : SkillBase
    {
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                if (pc.weaponSystem.equipment is GunBase gun)
                {
                    gun.AfterFireAction += () =>
                    {
                        gun.SetMagazineRPC(StatusValueType.Current, ++gun.magazine.Current);
                        if(HasStateAuthority) gun.FireBulletRPC();
                    };
                }
            }
        }   

        public override void MainLoop()
        {
        }

        public override void Run()
        {
        }
    }
}

