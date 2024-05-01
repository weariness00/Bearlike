using Inventory;
using Skill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillUIHandle : MonoBehaviour, IInventoryUIUpdate
    {
        public RawImage icon;
        public TMP_Text levelText;
        
        public void UIUpdateFromItem<UIItem>(UIItem item)
        {
            if (item is SkillBase skill)
            {
                icon.texture = skill.icon;
                levelText.text = skill.level.Current.ToString();
                
                if (skill.level.isMin)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

