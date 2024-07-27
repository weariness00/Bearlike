using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Manager;
using UnityEngine;

namespace Manager.FireBase
{
    public class FireBaseAuthManager
    {
        public static FireBaseAuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FireBaseAuthManager();
                    _instance.Init();
                }

                return _instance;
            }
        }
        private static FireBaseAuthManager _instance;

        public static void CreateAccount(string email, string password) => Instance.FireBaseCreateAccount(email, password);
        public static void Login(string email, string password, Action<bool> successAction = null) => Instance.FireBaseLogin(email, password, successAction);
        public static void LogOut() => Instance.FireBaseLogOut();

        public static Action<string> AccountCreateAction { get; set; }
        public static Action<AuthError, string> AuthErrorAction { get; set; }
        public static Action<bool> LoginState { get; set; }

        public static string UserId => Instance._user.UserId;
        
        private FirebaseAuth _auth;
        private FirebaseUser _user;

        private bool tryLogin = false;
        
        private void Init()
        {
            _auth = FirebaseAuth.DefaultInstance;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                if (task.Result == DependencyStatus.Available)
                {
                    DebugManager.Log("Firebase 적용 중");
                }
                else
                {
                    DebugManager.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                }
            });

            _auth.StateChanged += OnChanged;
        }

        private void OnChanged(object ender, EventArgs e)
        {
            if (_auth.CurrentUser != _user)
            {
                bool signed = (_auth.CurrentUser != _user && _auth.CurrentUser != null);
                if (!signed && _user != null)
                {
                    LoginState?.Invoke(false);
                }

                _user = _auth.CurrentUser;
                if (signed)
                {
                    LoginState?.Invoke(true);
                }
            }
        }

        private void FireBaseCreateAccount(string email, string password)
        {
            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    DebugManager.LogWarning("회원가입 취소");
                    AccountCreateAction?.Invoke("회원가입 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    DebugManager.LogWarning("회원가입 실패: " + task.Exception);
                    if (task.Exception.GetBaseException() is FirebaseException firebaseEx)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        HandleAuthError(errorCode);
                    }
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                DebugManager.Log($"계정 생성 [{newUser.Email}]");
                AccountCreateAction?.Invoke("계정 생성 성공");
            });
        }

        private void FireBaseLogin(string email, string password, Action<bool> successAction)
        {
            if(tryLogin) return;
            tryLogin = true;

            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogWarning("로그인 취소");
                    successAction?.Invoke(false);
                    tryLogin = false;
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogWarning("로그인 실패: " + task.Exception);
                    if (task.Exception.GetBaseException() is FirebaseException firebaseEx)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        HandleAuthError(errorCode);
                    }
                    successAction?.Invoke(false);
                    tryLogin = false;
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                successAction?.Invoke(true);
                Debug.Log($"로그인 [{newUser.Email}]");
                tryLogin = false;
            });
        }

        private void FireBaseLogOut()
        {
            if (_auth != null)
            {
                _auth.SignOut();
                Debug.Log("로그아웃");
            }
        }
        
        private void HandleAuthError(AuthError errorCode)
        {
            string message = "";
            switch (errorCode)
            {
                case AuthError.AccountExistsWithDifferentCredentials:
                    message = "이미 다른 자격 증명을 사용하여 가입된 계정입니다.";
                    break;
                case AuthError.MissingPassword:
                    message = "비밀번호를 입력하세요.";
                    break;
                case AuthError.WeakPassword:
                    message = "비밀번호가 너무 약합니다.";
                    break;
                case AuthError.WrongPassword:
                    message = "비밀번호가 올바르지 않습니다.";
                    break;
                case AuthError.InvalidEmail:
                    message = "유효하지 않은 이메일 주소입니다.";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "이미 사용 중인 이메일 주소입니다.";
                    break;
                case AuthError.UserNotFound:
                    message = "해당 사용자 정보를 찾을 수 없습니다.";
                    break;
                case AuthError.NetworkRequestFailed:
                    message = "네트워크 요청에 실패했습니다. 인터넷 연결을 확인하세요.";
                    break;
                default:
                    message = "알 수 없는 오류가 발생했습니다.";
                    break;
            }
            
            AuthErrorAction?.Invoke(errorCode, message);
            Debug.LogError("오류: " + message);
        }
    }
}