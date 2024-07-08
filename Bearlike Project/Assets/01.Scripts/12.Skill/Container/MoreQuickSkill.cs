using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Skill.Container
{
    /// <summary>
    /// 더 빠른 스킬
    /// 스킬의 쿨타임을 줄여준다.
    /// </summary>
    public class MoreQuickSkill : SkillBase
    {
        [SerializeField] private float reductionRate;

        private List<SkillBase> _activeSkillList = new List<SkillBase>();
        private HashSet<SkillBase> _coolReductionSkillList = new HashSet<SkillBase>(); // 이미 스킬 쿨타임을 적용 시킨 스킬인지 확인

        #region Unity Event Function

        public override void Awake()
        {
            base.Awake();

            var statusData = GetStatusData(id);
            reductionRate = statusData.GetFloat("Reduction Rate");
        }

        #endregion

        #region Member Function
        
        public override void MainLoop(){}
        public override void Run(){}

        public override void LevelUp(int upAmount = 1, bool isAddInventory = true)
        {
            base.LevelUp(upAmount, isAddInventory);
            if(ownerPlayer) ownerPlayer.skillSystem.SetCoolTimeReductionRate(reductionRate * level);
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Reduction Rate)")) explain = explain.Replace("(Reduction Rate)", $"{reductionRate * 100f}");
            if (explain.Contains("(Level)")) explain = explain.Replace("(Level)", $"{level.Current}");
            
            explain = StringExtension.CalculateNumber(explain);
        }

        #endregion
    }
}