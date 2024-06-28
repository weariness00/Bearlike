using System;
using Loading;
using Photon;
using ProjectUpdate;
using SceneExtension;
using Script.Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class LobbyManager : MonoBehaviour
    {
        public AudioClip BGM;
        public TMP_InputField userID;

        public Button magicCottonSceneButton;

        private SceneReference _lobbyLoading;
        private SceneReference _magicCotton;

        private void Start()
        {
            _lobbyLoading = SceneList.GetScene("Lobby Loading");
            _magicCotton = SceneList.GetScene("Magic Cotton");
            SoundManager.Play(BGM, SoundManager.SoundType.BGM);
            
            Cursor.lockState = CursorLockMode.None;
            
            NetworkManager.Instance.LobbyConnect();
            LoadingManager.StartAction += () => SceneManager.LoadScene(_lobbyLoading, LoadSceneMode.Additive);
            LoadingManager.EndAction += () =>
            {
                SceneManager.UnloadSceneAsync(_lobbyLoading);
                SceneManager.LoadScene(_magicCotton, LoadSceneMode.Additive);
            };
            ProjectUpdateManager.Instance.Init();
            
        }

        private void Update()
        {
            if (KeyManager.InputActionDown(KeyToAction.Esc))
            {
                SettingCanvas.SetActive(SettingCanvasType.Setting);
            }
        }
    }
}