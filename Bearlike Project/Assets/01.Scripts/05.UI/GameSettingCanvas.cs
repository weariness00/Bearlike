using System;
using Manager;
using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameSettingCanvas : MonoBehaviour
    {
        public Canvas settingCanvas;
        [SerializeField] private GameObject parentObject;
        public Button goLobbyButton;
        public Button quitGameButton;
        [SerializeField] private Button settingButton;
        
        private void Start()
        {
            //
            goLobbyButton.onClick.AddListener(() => NetworkManager.Runner.Shutdown());
#if UNITY_EDITOR
            quitGameButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            quitGameButton.onClick.AddListener(Application.Quit);
#endif
            settingButton.onClick.AddListener(() =>
            {
                SettingCanvas.Active(SettingCanvasType.All);
                UIManager.AddActiveUI(SettingCanvas.Instance.gameObject);
            });
            
            parentObject.SetActive(false);
        }

        private void Update()
        {
            if (UIManager.HasActiveUI() == false && Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                
                parentObject.SetActive(true);
                UIManager.AddActiveUI(parentObject);
            }
        }
    }
}

