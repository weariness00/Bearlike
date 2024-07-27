using Manager;
using Status;
using UnityEngine;
using Util;

namespace Skill.Container
{
    public class AttachTape : SkillBase
    {
        [Header("테이프 변수")]
        [SerializeField] private Sprite buffIcon;
        [SerializeField] private StatusValue<int> damageNullified; // 최대 피해 무효 횟수

        public override void Awake()
        {
            base.Awake();

            damageNullified.Max = level.Max;
        }

        public override void Spawned()
        {
            base.Spawned();
            isInvoke = true;
            StartCoolTimer(0);
        }

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            ownerPlayer.status.AddBeforeApplyDamageEvent(DamageNullified);
        }

        public override void MainLoop()
        {
            if (IsUse)
            {
                StartCoolTimer(GetCoolTime());
                RunRPC();
            }
        }

        public override void Run()
        {
            ++damageNullified.Current;
            ownerPlayer.status.AddBeforeApplyDamageEvent(DamageNullified, false);

            if (HasInputAuthority)
            {
                if (!ownerPlayer.uiController.buffCanvas.HasUI(skillName))
                {
                    DebugManager.ToDo("풀 스크린 이펙트 넣기, 테이프 붙이는 효과음, 이펙트 넣기");
                    URPRendererFeaturesManager.Instance.StartEffect("ShieldEffect");

                    ownerPlayer.uiController.buffCanvas.SpawnUI(skillName);
                    ownerPlayer.uiController.buffCanvas.SetIcon(skillName, buffIcon);
                    ownerPlayer.uiController.buffCanvas.SetStackText(skillName, damageNullified);
                }
            }
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Cool Time)")) explain = explain.Replace("(Cool Time)", $"{GetCoolTime()}");
            if (explain.Contains("(Level)")) explain = explain.Replace("(Level)", $"{level.Current}");

            explain = explain.CalculateNumber();
        }

        /// <summary>
        /// 대미지를 막아주는 로직
        /// StatusBase의 BeforeApplyDamageEvent에 추가해준다.
        /// </summary>
        /// <param name="applyDamage">현재 적용되는 Damage</param>
        /// <returns> 최종적으로 적용된 applyDamage를 반환</returns>
        private int DamageNullified(int applyDamage)
        {
            if (!damageNullified.isMin)
            {
                --damageNullified.Current;
                applyDamage = 0;
            }

            if (damageNullified.isMin)
            {
                ownerPlayer.status.RemoveBeforeApplyDamageEvent(DamageNullified);
                ownerPlayer.uiController.buffCanvas.RemoveUI(skillName);
            }
            
            if (HasInputAuthority)
            {
                DebugManager.ToDo("풀 스크린 이펙트 빼기, 쉴드가 깨지거나 테이프가 때지는 효과랑 효과음 넣기");
                if (ownerPlayer.uiController.buffCanvas.HasUI(skillName))
                    ownerPlayer.uiController.buffCanvas.SetStackText(skillName, damageNullified);
            }
            
            return applyDamage;
        }
    }
}