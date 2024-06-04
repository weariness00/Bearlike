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
            skill.LevelUp(1, false);
            
            if(skill.icon) icon.texture = skill.icon.texture;
            titleText.text = skill.skillName;
            explainText.text = skill.explain;
            
            skill.LevelUp(-1, false);
            
            id = skill.id;
        }
    }
}

