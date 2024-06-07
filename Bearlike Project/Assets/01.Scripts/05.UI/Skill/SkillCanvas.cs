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
    public class SkillCanvas : MonoBehaviour
    {
        public SkillBlock firstSkill = new SkillBlock();
        public SkillBlock secondSkill = new SkillBlock();
        public SkillBlock ultimateSkill = new SkillBlock();

        private Coroutine _firstCoolTimeCoroutine;
        private Coroutine _secondCoolTimeCoroutine;
        private Coroutine _ultimateCoolTimeCoroutine;

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
                block.coolTimeImage.fillAmount = 0;
                block.useImage.gameObject.SetActive(false);
                block.timerText.gameObject.SetActive(false);
            }
        }

        public void StartCoolTime(SkillBase skill)
        {
            if (skill.IsUse)
            {
                if (firstSkill.skill == skill)
                {
                    if(_firstCoolTimeCoroutine != null) StopCoroutine(_firstCoolTimeCoroutine);
                    _firstCoolTimeCoroutine = StartCoroutine(StartCoolTimeCoroutine(firstSkill));
                }
                else if(secondSkill.skill == skill)
                {
                    if(_secondCoolTimeCoroutine != null) StopCoroutine(_secondCoolTimeCoroutine);
                    _secondCoolTimeCoroutine = StartCoroutine(StartCoolTimeCoroutine(secondSkill));
                }
                else if(ultimateSkill.skill == skill)
                {
                    if(_ultimateCoolTimeCoroutine != null) StopCoroutine(_ultimateCoolTimeCoroutine);
                    _ultimateCoolTimeCoroutine = StartCoroutine(StartCoolTimeCoroutine(ultimateSkill));
                }
            }
        }

        private IEnumerator StartCoolTimeCoroutine(SkillBlock block)
        {
            var skill = block.skill;
            
            block.useImage.gameObject.SetActive(true);
            while (true)
            {
                yield return null;
                if (skill.isInvoke == false)
                    break;
            }   
            block.useImage.gameObject.SetActive(false);
            
            float timer = skill.GetCoolTime();
            float realCoolTime = timer;
            block.coolTimeImage.fillAmount = 1;
            block.timerText.gameObject.SetActive(true);
            while (true)
            {
                timer -= Time.deltaTime;
                block.coolTimeImage.fillAmount = timer / realCoolTime;
                block.timerText.text = ((int)timer).ToString(CultureInfo.InvariantCulture);
                yield return null;
                
                if (skill.IsUse)
                    break;
            }
            
            block.coolTimeImage.fillAmount = 0;
            block.timerText.gameObject.SetActive(false);

            if (firstSkill.skill == skill)
                _firstCoolTimeCoroutine = null;
            else if (secondSkill.skill == skill)
                _secondCoolTimeCoroutine = null;
            else if (ultimateSkill.skill == skill)
                _ultimateCoolTimeCoroutine = null;
        }
        
        [System.Serializable]
        public class SkillBlock
        {
            public SkillBase skill;
            public Image icon;
            public Image coolTimeImage; // 쿨타임을 시각적으로 보여주는 이미지
            public Image useImage; // 스킬을 눌렀으나 아직 사용하지 않거나 스킬의 지속시간이 있을경우 활성화
            public TMP_Text timerText;
        }
    }
}

