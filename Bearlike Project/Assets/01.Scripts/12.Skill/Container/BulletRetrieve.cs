using UnityEngine;
using Util;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 총을 쏘면 일정 확률로 N개의 총알을 되도려주는 스킬
    /// </summary>
    public class BulletRetrieve : SkillBase
    {
        [SerializeField] private float retrieveProbability; // 총알 회수 확률
        [SerializeField] private int retrieveAmount; // 총알 회수 수량 1발 쏘았는데 2발을 돌려줄 수도 있다.

        private GunBase gun;
        
        public override void Awake()
        {
            base.Awake();

            var statusData = GetStatusData(id);

            retrieveAmount = statusData.GetInt("Retrieve Amount");
            
            retrieveProbability = statusData.GetFloat("Retrieve Probability");
        }

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (ownerPlayer.weaponSystem.TryGetEquipGun(out gun))
            {
                gun.AfterFireAction -= Run;
                gun.AfterFireAction += Run;
            }
        }

        public override void MainLoop()
        {
            
        }

        public override void Run()
        {
            if (gun &&
                (level.Current * retrieveProbability).IsProbability(1.0f))
            {
                gun.magazine.Current += retrieveAmount;
            }
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Retrieve Probability)"))
                explain = explain.Replace("(Retrieve Probability)", $"{retrieveProbability * level}");
            if (explain.Contains("(Retrieve Amount)"))
                explain = explain.Replace("(Retrieve Amount)", $"{(retrieveAmount)}");
            
            explain = explain.CalculateNumber().Floor(true);
        }
    }
}