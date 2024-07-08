using System;
using Data;
using Fusion;
using Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MatchRoomUserUI : NetworkBehaviourEx
    {
        public TMP_Text[] users_Text;
        public Button startButton;
        public Button exitButton;

        [SerializeField] private TMP_Dropdown difficultDropdown;

        #region Unity Event Function
    
        protected void Awake()
        {
            exitButton.onClick.AddListener(OnExit);
            difficultDropdown.onValueChanged.AddListener((value) =>
            {
                PlayerPrefs.SetInt("Difficult", value);
                if (HasStateAuthority)
                    SetDifficultRPC(value);
            });
        }

        private void Start()
        {
            UserData.Instance.UserJoinAction += UserActionToDataUpdate;
            UserData.Instance.UserLeftAction += UserActionToDataUpdate;
            UserData.Instance.NameUpdateAfterAction += DataUpdateRPC;
        }

        private void Update()
        {   
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExit();
            }
        }

        private void OnDestroy()
        {
            UserData.Instance.UserJoinAction -= UserActionToDataUpdate;
            UserData.Instance.UserLeftAction -= UserActionToDataUpdate;
        }

        public override void Spawned()
        {
            base.Spawned();
            DataUpdate();
        
            if (HasStateAuthority)
            {
                difficultDropdown.interactable = true;
                difficultDropdown.value  = PlayerPrefs.GetInt("Difficult");
            }
            else
            {
                startButton.gameObject.SetActive(false);
                difficultDropdown.interactable = false;
                RequestSetDifficultRPC();
            }
        }
    
        #endregion

        // UserData의 Action들에 넣고 뺼 용으로 사용하는 함수
        private void UserActionToDataUpdate(PlayerRef playerRef) => DataUpdateRPC();
        [Rpc(RpcSources.All,RpcTargets.All)]
        public void DataUpdateRPC() => DataUpdate();
        public void DataUpdate()
        {
            try
            {
                var items = NetworkUtil.DictionaryItems(UserData.Instance.UserDictionary);
                UpdateData(items);
            }
            catch (Exception e)
            {
                UserData.Instance.AfterSpawnedAction += DataUpdateRPC;
            }
        }
    
        public void UpdateData(UserDataStruct[] dataList)
        {
            for (int i = 0; i < 3; i++)
            {
                users_Text[i].text = "Unknown";
            }

            for (int i = 0; i < dataList.Length; i++)
            {
                users_Text[i].text = dataList[i].Name.ToString();
            }
        }

        public void OnExit()
        {
            NetworkManager.Runner.Shutdown();
        }

        public string GetDifficult()
        {
            var option = difficultDropdown.options[difficultDropdown.value];
            return option.text;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void SetDifficultRPC(int value) => difficultDropdown.value = value;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RequestSetDifficultRPC() => SetDifficultRPC(difficultDropdown.value);
    }
}