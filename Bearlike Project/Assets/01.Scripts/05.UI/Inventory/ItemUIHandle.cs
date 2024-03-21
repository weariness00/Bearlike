using Inventory;
using Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class ItemUIHandle : MonoBehaviour, IInventoryUIUpdate
    {
        public RawImage icon;
        public TMP_Text count;
        
        public void UIUpdateFromItem<Item1>(Item1 item)
        {
            if (item is ItemBase itemBase)
            {
                icon.texture = itemBase.icon;
                count.text = itemBase.amount.Current.ToString();
                
                if (itemBase.amount.isMin)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}