using System;
using Skill;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Skill
{
    public class SkillSelectBlockHandle : MonoBehaviour
    {
        public Button button;
        public RawImage icon;
        public TMP_Text titleText;
        public TMP_Text explainText;

        public int id;

        public void SettingBlock(SkillBase skill)
        {
            skill.level.Current += 1;
            skill.ExplainUpdate();
            skill.level.Current -= 1;
            
            if(icon && skill.icon) icon.texture = skill.icon.texture;
            if(titleText) titleText.text = skill.skillName;
            if(explainText) explainText.text = skill.explain;
            
            id = skill.id;
        }
    }
}

