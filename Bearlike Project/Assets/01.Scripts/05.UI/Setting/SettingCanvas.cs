using System;
using System.Collections.Generic;
using Manager;
using Script.Data;
using UnityEngine;
using Util;

namespace UI
{
    public enum SettingCanvasType
    {
        Setting,
        Sound,
        Performance,
        All,
    }

    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneEnd)]
    public class SettingCanvas : Singleton<SettingCanvas>
    {
        public SoundManagerCanvas soundCanvas;
        
        private List<GameObject> _settingCanvasObjectList = new List<GameObject>();
        
        protected override void Awake()
        {
            base.Awake();
            _settingCanvasObjectList.Add(soundCanvas.gameObject);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public static void Active(SettingCanvasType type) => Instance.SetActiveCanvas(type);
        public void SetActiveCanvas(SettingCanvasType type)
        {
            gameObject.SetActive(true);
            UIManager.AddActiveUI(gameObject);
            
            foreach (var o in _settingCanvasObjectList)
                o.SetActive(false);

            switch (type)
            {
                case SettingCanvasType.Setting:
                    break;
                case SettingCanvasType.Sound:
                    soundCanvas.gameObject.SetActive(true);
                    break;
                case SettingCanvasType.All:
                    foreach (var o in _settingCanvasObjectList)
                        o.SetActive(true);
                    break;
            }
        }
    }
}