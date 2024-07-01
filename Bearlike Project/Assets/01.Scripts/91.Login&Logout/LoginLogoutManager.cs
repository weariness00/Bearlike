using Manager.FireBase;
using SceneExtension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Login_Logout
{
    public class LoginLogoutManager : MonoBehaviour
    {
        public TMP_InputField idInputFiled;
        public TMP_InputField passwordInputFiled;
        public Button loginButton;
        public Button accountCreateButton;
        public Toggle idMemoryToggle;
        [SerializeField] private Button exitButton; // 게임 종료 버튼
        
        #region Unity Event Function

        private void Start()
        {
            passwordInputFiled.onValueChanged.AddListener(ValidateInput);
            
            LogOut();
            accountCreateButton.onClick.AddListener(AccountCreate);
            loginButton.onClick.AddListener(Login);
            
#if UNITY_EDITOR
            exitButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            exitButton.onClick.AddListener(Application.Quit);
#endif

            FireBaseAuthManager.LoginState -= ChangeLoginState;
            FireBaseAuthManager.LoginState += ChangeLoginState;

            if (PlayerPrefs.HasKey("IDMemory")) idMemoryToggle.isOn = PlayerPrefs.GetInt("IDMemory") == 1;
            if (idMemoryToggle.isOn && PlayerPrefs.HasKey("ID")) idInputFiled.text = PlayerPrefs.GetString("ID");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Login();
            }

            if (idInputFiled.isFocused && Input.GetKeyDown(KeyCode.Tab))
            {
                passwordInputFiled.Select();
            }
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetInt("IDMemory", idMemoryToggle.isOn ? 1 : 0);
        }

        #endregion

        #region Member Function

        private void ChangeLoginState(bool value)
        {
            if (value)
            {
                if (idMemoryToggle.isOn) PlayerPrefs.SetString("ID", idInputFiled.text);
                FireBaseDataBaseManager.RootReference.GetChild("UserData").SnapShot(snapshot =>
                {
                    if (snapshot.HasChild(FireBaseAuthManager.UserId) == false)
                    {
                        snapshot.Reference.SetChild(FireBaseAuthManager.UserId, true);
                    }
                });
                
                SceneManager.LoadScene(SceneList.GetScene("Lobby"));
            }
        }

        public void AccountCreate()
        {
            FireBaseAuthManager.CreateAccount(idInputFiled.text, passwordInputFiled.text);
        }

        public void Login()
        {
            FireBaseAuthManager.Login(idInputFiled.text, passwordInputFiled.text);
        }

        public void LogOut()
        {
            FireBaseAuthManager.LogOut();
        }
        
        #endregion

        #region Password Funciton

        void ValidateInput(string input)
        {
            string filtered = "";
            foreach (char c in input)
            {
                if (IsAllowedCharacter(c))
                {
                    filtered += c;
                }
            }
            passwordInputFiled.text = filtered;
        }
        
        bool IsAllowedCharacter(char c)
        {
            // 숫자, 영어, 특수문자만 허용
            return char.IsLetterOrDigit(c) || IsSpecialCharacter(c);
        }

        bool IsSpecialCharacter(char c)
        {
            // 특수문자 범위 지정 (예: !, @, #, $, %, ^, &, *)
            string specialCharacters = "!@#$%^&*()_-+=<>?{}[]|~`";
            return specialCharacters.Contains(c);
        }

        #endregion
    }
}