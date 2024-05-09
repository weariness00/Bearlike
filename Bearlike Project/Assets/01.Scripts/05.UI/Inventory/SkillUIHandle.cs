using System;
using Skill;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class SkillUIHandle : MonoBehaviour, IInventoryUIUpdate
    {
        private static InventoryItemExplainHandle _inventoryItemExplainHandle;
        
        public Toggle toggle;
        public RawImage icon;
        public TMP_Text levelText;

        private SkillBase _skill;

        private void Awake()
        {
            if(!_inventoryItemExplainHandle)
                _inventoryItemExplainHandle = FindObjectOfType<SkillInventory>().explainHandel;
            
            toggle.onValueChanged.AddListener(OnClick);
        }

        public void UIUpdateFromItem<UIItem>(UIItem item)
        {
            if (!_skill && item is SkillBase skill)
            {
                _skill = skill;
                icon.texture = _skill.icon.texture;
            }

            levelText.text = _skill.level.Current.ToString();
            if (_skill.level.isMin)
                Destroy(gameObject);
        }

        private void OnClick(bool value)
        {
            if (value)
            {
                _skill.ExplainUpdate();
                _inventoryItemExplainHandle.titleText.text = _skill.skillName; 
                _inventoryItemExplainHandle.explainText.text = " " + _skill.explain;
            }
        }
    }
}

