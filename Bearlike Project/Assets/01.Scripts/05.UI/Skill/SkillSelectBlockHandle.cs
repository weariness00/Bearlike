using System;
using Skill;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Skill
{
    public class SkillSelectBlockHandle : MonoBehaviour, IPointerClickHandler
    {
        public Toggle toggle;
        public RawImage icon;
        public TMP_Text titleText;
        public TMP_Text explainText;

        public int id;

        public Action DoubleClickEvent;

        public void SettingBlock(SkillBase skill)
        {
            if(skill.icon) icon.texture = skill.icon.texture;
            titleText.text = skill.skillName;
            explainText.text = skill.explain;

            id = skill.id;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (toggle.isOn)
            {
                DoubleClickEvent?.Invoke();
            }
        }
    }
}

