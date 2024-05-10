using System;
using System.Collections;
using System.Globalization;
using Photon;
using Skill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Skill
{
    /// <summary>
    /// 스킬의 쿨타임 표시
    /// </summary>
    public class SkillCanvas : NetworkBehaviourEx
    {
        public SkillBlock firstSkill = new SkillBlock();
        public SkillBlock secondSkill = new SkillBlock();
        public SkillBlock ultimateSkill = new SkillBlock();

        public void SetFirstSkill(SkillBase skill) => firstSkill.skill = skill;
        public void SetSecondSkill(SkillBase skill) => secondSkill.skill = skill;
        public void SetUltimateSkill(SkillBase skill) => ultimateSkill.skill = skill;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Initialize()
        {
            InitSkillBlock(firstSkill);
            InitSkillBlock(secondSkill);
            InitSkillBlock(ultimateSkill);
        }

        private void InitSkillBlock(SkillBlock block)
        {
            if (block.skill)
            {
                block.icon.sprite = block.skill.icon;
                block.timerText.gameObject.SetActive(false);
            }
        }

        public void StartCoolTime(SkillBase skill)
        {
            if (skill.IsUse)
            {
                if(firstSkill.skill == skill)
                    StartCoroutine(StartCoolTimeCoroutine(firstSkill));
                else if(secondSkill.skill == skill)
                    StartCoroutine(StartCoolTimeCoroutine(secondSkill));
                else if(ultimateSkill.skill == skill)
                    StartCoroutine(StartCoolTimeCoroutine(ultimateSkill));
            }
        }

        private IEnumerator StartCoolTimeCoroutine(SkillBlock block)
        {
            var skill = block.skill;
            float timer = skill.coolTime;
            block.icon.fillAmount = 0;
            block.timerText.gameObject.SetActive(true);
            while (true)
            {
                if (skill.IsUse)
                    break;

                timer -= Time.deltaTime;
                block.icon.fillAmount = (skill.coolTime - timer) / skill.coolTime;
                block.timerText.text = ((int)timer).ToString(CultureInfo.InvariantCulture);
                yield return null;
            }
            
            block.icon.fillAmount = 1;
            block.timerText.gameObject.SetActive(false);
        }
        
        [System.Serializable]
        public class SkillBlock
        {
            public SkillBase skill;
            public Image icon;
            public TMP_Text timerText;
        }
    }
}

