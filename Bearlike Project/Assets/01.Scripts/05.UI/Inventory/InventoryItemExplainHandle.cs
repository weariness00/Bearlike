using System;
using TMPro;
using UnityEngine;

namespace UI.Inventory
{
    public class InventoryItemExplainHandle : MonoBehaviour
    {
        [HideInInspector] public RectTransform rectTransform;
        
        public TMP_Text titleText;
        public TMP_Text explainText;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }
}