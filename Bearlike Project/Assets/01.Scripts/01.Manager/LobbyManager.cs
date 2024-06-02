using System;
using Loading;
using Photon;
using Script.Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class LobbyManager : MonoBehaviour
    {
        public AudioClip BGM;
        public TMP_InputField userID;
    
        public SceneReference lobbyLoading;

        private void Start()
        {
            SoundManager.Play(BGM, SoundManager.SoundType.BGM);
            
            Cursor.lockState = CursorLockMode.None;
            
            NetworkManager.Instance.LobbyConnect();
            LoadingManager.StartAction += () => SceneManager.LoadScene(lobbyLoading, LoadSceneMode.Additive);
            LoadingManager.EndAction += () => SceneManager.UnloadSceneAsync(lobbyLoading);
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