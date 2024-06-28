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
        
        #region Unity Event Function

        private void Start()
        {
            LogOut();
            accountCreateButton.onClick.AddListener(AccountCreate);
            loginButton.onClick.AddListener(Login);

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
                FireBaseDataBaseManager.RootReference.GetChild("UserData").SetChild(FireBaseAuthManager.UserId,true);
                
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
    }
}