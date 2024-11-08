﻿using Item;
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
                icon.texture = itemBase.Icon;
                count.text = itemBase.Amount.Current.ToString();
                
                if (itemBase.Amount.isMin)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}