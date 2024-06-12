using Fusion;
using Skill.Support;
using Status;
using UnityEngine;

namespace Skill.Container
{
    /// <summary>
    /// 레벨업을 할 시 특정 버프를 주는 장판을 소환
    /// </summary>
    public class LevelUpBoost : SkillBase
    {
        [SerializeField] private NetworkPrefabRef boostFlagRef;
        [SerializeField] private float boostRange; // 장판 범위
        [SerializeField] private float boostDuration; // 지속시간
        [SerializeField] private int healAmount; // 체력 회복량
        [SerializeField] private float healTime; // 체력회복에 걸리는 시간

        public override void Awake()
        {
            base.Awake();

            var statusData = GetStatusData(id);
            healAmount = statusData.GetInt("Heal Amount");
            
            boostRange = statusData.GetFloat("Boost Range");
            boostDuration = statusData.GetFloat("Boost Duration");
            healTime = statusData.GetFloat("Heal Time");
        }

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            ownerPlayer.status.LevelUpAction += Run;
        }

        public override void MainLoop()
        {
        }

        public override void Run()
        {
            if(!HasStateAuthority) return;

            Runner.SpawnAsync(boostFlagRef, ownerPlayer.transform.position, null, null, (runner, o) =>
            {
                if (!o.TryGetComponent(out BoostFlag flag)) return;

                flag.OwnerSkillId = Object.Id;
                flag.BoostRange = boostRange;
                flag.BoostDuration = boostDuration;
                flag.HealingAmount = healAmount;
                flag.HealingTime = healTime;
            });
        }
    }
}