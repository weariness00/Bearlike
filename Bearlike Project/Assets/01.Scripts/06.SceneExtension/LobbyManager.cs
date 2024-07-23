using Loading;
using Manager;
using Manager.FireBase;
using Photon;
using ProjectUpdate;
using Script.Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SceneExtension
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class LobbyManager : MonoBehaviour
    {
        public AudioClip BGM;
        public TMP_InputField userID;

        public Button magicCottonSceneButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private TMP_InputField nickNameInput;

        private SceneReference _lobbyLoading;
        private SceneReference _magicCotton;

        private void Awake()
        {
            settingButton.onClick.AddListener(() =>
            {
                SettingCanvas.Active(SettingCanvasType.All);
            });
            
            logoutButton.onClick.AddListener(() =>
            {
                FireBaseAuthManager.LogOut();
                NetworkManager.Runner.Shutdown();
            });
        }

        private void Start()
        {
            _lobbyLoading = SceneList.GetScene("Lobby Loading");
            _magicCotton = SceneList.GetScene("Magic Cotton");
            SoundManager.Play(BGM, SoundManager.SoundType.BGM);
            
            Cursor.lockState = CursorLockMode.None;
            
            NetworkManager.Instance.LobbyConnect();
            LoadingManager.Initialize();
            LoadingManager.StartAction += () => SceneManager.LoadScene(_lobbyLoading, LoadSceneMode.Additive);
            LoadingManager.EndAction += () =>
            {
                SceneManager.UnloadSceneAsync(_lobbyLoading);
                SceneManager.LoadScene(_magicCotton, LoadSceneMode.Additive);
            };
            ProjectUpdateManager.Instance.Init();
            
            // 닉네임
            nickNameInput.onEndEdit.AddListener((value) =>
            {
                FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}/Name").SetValueAsync(value);
            });
            LoadingManager.AddWait();
            FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}/Name").SnapShot(snapshot =>
            {
                nickNameInput.text = snapshot.ValueString();
                    
                LoadingManager.EndWait("닉네임 불러오기 성공");
            });
        }
    }
}