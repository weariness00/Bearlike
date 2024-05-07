using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class InteractUI : Singleton<InteractUI> 
    {
        #region Static

        public static TMP_Text KeyCodeText => Instance.keyCodeText;
        public static Slider GageSlider => Instance.slider;    

        public static void SetKeyActive(bool value) => Instance.keyUIObject.SetActive(value);
        public static void SetGageActive(bool value) => Instance.gageUIObject.SetActive(value);

        #endregion

        [Header("Key")] 
        public GameObject keyUIObject;
        public Image image;
        public TMP_Text keyCodeText;

        [Header("Gage")] 
        public GameObject gageUIObject;
        public Slider slider;

        private void Start()
        {
            SetActiveAll(false);
        }
        
        public void SetActiveAll(bool value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.gameObject.SetActive(value);
            }
        }
    }
}

