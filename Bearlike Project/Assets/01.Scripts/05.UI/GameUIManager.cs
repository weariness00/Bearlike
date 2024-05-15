﻿using Manager;
using Photon;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region Static

        private UniqueQueue<GameObject> _ActiveUIQueue;
        public static void AddActiveUI(GameObject uiObject) => Instance._ActiveUIQueue.Enqueue(uiObject);

        #endregion

        public Canvas settingCanvas;
        public Button goLobbyButton;
        public Button quitGameButton;

        private bool childrenActiveSelf;

        private void Awake()
        {
            base.Awake();
            _ActiveUIQueue = new UniqueQueue<GameObject>();
        }

        private void Start()
        {
            //
            goLobbyButton.onClick.AddListener(() => NetworkManager.Runner.Shutdown());
#if UNITY_EDITOR
            quitGameButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            quitGameButton.onClick.AddListener(Application.Quit);
#endif
            settingCanvas.gameObject.SetActive(false);
        }

        public void Update()
        {
            if (KeyManager.InputActionDown(KeyToAction.Esc))
            {
                if (_ActiveUIQueue.IsEmpty())
                {
                    settingCanvas.gameObject.SetActive(true);
                    _ActiveUIQueue.Enqueue(settingCanvas.gameObject);
                }
                else
                {
                    var queue = _ActiveUIQueue.Dequeue();
                    queue.SetActive(false);
                }
            }
        }
        
        public static void ActiveUIAllDisable()
        {
            var list = Instance._ActiveUIQueue.AllDequeue();
            foreach (var uiObj in list)
                uiObj.SetActive(false);
        }
    }
}