using System;
using System.Collections.Generic;
using Script.Data;
using UnityEngine;
using Util;

namespace UI
{
    public enum SettingCanvasType
    {
        Setting,
        Sound,
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

        public static void SetActive(SettingCanvasType type) => Instance.SetActiveCanvas(type);
        public void SetActiveCanvas(SettingCanvasType type)
        {
            if (type == SettingCanvasType.Setting)
            {
                gameObject.SetActive(!gameObject.activeSelf);
                return;
            }
            
            foreach (var o in _settingCanvasObjectList)
                o.SetActive(false);

            switch (type)
            {
                case SettingCanvasType.Sound:
                    soundCanvas.gameObject.SetActive(true);
                    break;
            }
        }
    }
}