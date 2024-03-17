using System;
using Manager;
using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameSettingUI : MonoBehaviour
    {
        public Button goLobbyButton;
        public Button quitGameButton;

        private bool childrenActiveSelf;

        private void Start()
        {
            goLobbyButton.onClick.AddListener(() => NetworkManager.Runner.Shutdown());
#if UNITY_EDITOR
            quitGameButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            quitGameButton.onClick.AddListener(Application.Quit);
#endif
            ActiveChildren(false);
        }

        public void Update()
        {
            if (KeyManager.InputActionDown(KeyToAction.Esc))
            {
                var value = !childrenActiveSelf;
                ActiveChildren(value);
            }
        }

        void ActiveChildren(bool value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(value);
            }

            childrenActiveSelf = value;
        }
    }
}