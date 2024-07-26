using System.Collections.Generic;
using Data;
using Fusion;
using GamePlay;
using GamePlay.Stage;
using GamePlay.StageLevel;
using Manager;
using Photon;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUI : NetworkBehaviour
    {
        private int clientNumber = -1;
        
        #region Network Variable

        private ChangeDetector _changeDetector;
        [Networked] public NetworkBool IsSettingUI { get; set; }
        [Networked] [Capacity(3)] private NetworkArray<NetworkBool> NetworkReadyArray { get; } // 투표를 마치고 준비가 되었는지
        [Networked] [Capacity(2)] public NetworkArray<int> StageVoteCount { get; }
        [Networked] [Capacity(2)] public NetworkArray<StageType> NetworkStages { get; }

        #endregion

        public List<StageData> nextStageList = new List<StageData>();

        [Header("스테이지 선택 토글")] public Toggle selectToggle;
        [Header("스테이지 정보 그룹")] public Transform stageToggleGroup;
        public GameObject stageSelectUIPrefab;
        private List<StageSelectUIHandler> stageSelectUIHandlerList = new List<StageSelectUIHandler>();

        private void Start()
        {
            StageBase.StageClearAction += SettingStageInfo;
        }

        private void OnDestroy()
        {
            StageBase.StageClearAction -= SettingStageInfo;
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            
            clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;
            
            selectToggle.onValueChanged.AddListener(Ready);
            
            // 객체 동기화를 위한 함수
            if (IsSettingUI)
            {
                SettingStageUI();
            }
            
            // UI 선택을 위한 정보 초기화
            SettingStageInfo();
        }
        
        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(StageVoteCount):
                        UpdateVoteText();
                        break;
                    case nameof(IsSettingUI): // 랜덤 스테이지가 정해지면 UI 셋팅
                        if (IsSettingUI)
                        {
                            SettingStageUI();
                        }
                        else
                        {
                            selectToggle.gameObject.SetActive(false);
                            gameObject.SetActive(false);
                        }
                        break;
                    case nameof(NetworkReadyArray):
                        var readyCount = 0;
                        foreach (var readyValue in NetworkReadyArray)
                        {
                            if (readyValue == false)
                            {
                                continue;
                            }
                            ++readyCount;
                        }

                        if (NetworkManager.PlayerCount == readyCount)
                        {
                            SetStage();
                        }
                        break;
                }
            }
        }
        
        public void UpdateVoteText()
        {
            for (int i = 0; i < NetworkStages.Length; i++)
            {
                int stageVoteCount = StageVoteCount.Get(i);
                stageSelectUIHandlerList[i].voteText.text = stageVoteCount.ToString();
            }
        }

        // 투표를 마쳤다는 표시
        public void Ready(bool value)
        {
            ReadyRPC(clientNumber, value);
        }

        public void SettingStageInfo()
        {
            gameObject.SetActive(true);
            if (HasStateAuthority == false)
            {
                return;
            }

            NetworkStages.Clear();
            for (int i = 0; i < NetworkStages.Length; i++)
            {
                var stageData = GameManager.Instance.GetRandomStage();
                NetworkStages.Set(i, stageData.info.stageType);
            }
            
            // Vote 초기화
            StageVoteCount.Clear();
            
            // Ready 초기화
            NetworkReadyArray.Clear();
            
            SetSettingUIRPC(true);
        }

        void SettingStageUI()
        {
            foreach (var selectHandler in stageSelectUIHandlerList)
            {
                Destroy(selectHandler.gameObject);
            }

            selectToggle.isOn = false;
            stageSelectUIHandlerList.Clear();
            for (int i = 0; i < NetworkStages.Length; i++)
            {
                StageData stageData;
                // 스테이지 최대치이면 보스 스테이지로 가도록 하기
                if (GameManager.Instance.stageCount.isMax)
                {
                    stageData = GameManager.Instance.GetBossStage();
                }
                else if (Managers.Instance.isTest)
                {
                    stageData = GameManager.Instance.GetStageIndex(i);
                }
                else
                {
                    var stageType = NetworkStages.Get(i);
                    stageData = GameManager.Instance.GetStageIndex((int)stageType);
                }
                stageData.info.SetJsonData(StageBase.GetInfoData(stageData.info.id));
                var index = i;
                var stageSelectUIObject = Instantiate(stageSelectUIPrefab, stageToggleGroup);
                var stageSelectUIHandler = stageSelectUIObject.GetComponent<StageSelectUIHandler>();
                stageSelectUIHandler.toggle.onValueChanged.AddListener((value) =>
                {
                    FixeVoteCountRPC(index, value);
                });
                stageSelectUIHandler.gameObject.SetActive(true);
                stageSelectUIHandler.Setting(stageData.info);
                
                if (nextStageList.Count >= NetworkStages.Length)
                {
                    nextStageList.Clear();
                }
                stageSelectUIHandlerList.Add(stageSelectUIHandler);
                nextStageList.Add(stageData);
            }

            gameObject.SetActive(true);
            selectToggle.gameObject.SetActive(true);
            UIManager.AddActiveUI(gameObject);
            
            DebugManager.Log("스테이지 선택 UI 셋팅");
        }

        // 투표된 스테이지 중 가장 투표수가 많은 스테이지로 시작
        public void SetStage()
        {
            if (Runner.IsServer)
            {
                int bicSelectIndex = 0;
                int index = 0;
                for (int i = 0; i < StageVoteCount.Length; i++)
                {
                    int vote = StageVoteCount.Get(i);
                    if (bicSelectIndex < vote)
                    {
                        bicSelectIndex = vote;
                        index = i;
                    }
                }

                IsSettingUI = false;

                GameManager.Instance.SetStage(nextStageList[index]);
            }

            foreach (var stageSelectUIHandler in stageSelectUIHandlerList)
                stageSelectUIHandler.toggle.interactable = false;
        }

        #region Vraiable RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetSettingUIRPC(NetworkBool value) => IsSettingUI = value;
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void FixeVoteCountRPC(int index, bool value)
        {
            var voteCount = StageVoteCount.Get(index);
            if (value)
            {
                StageVoteCount.Set(index, voteCount + 1);
            }
            else
            {
                StageVoteCount.Set(index, voteCount - 1);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetVoteCountRPC(int index, int value) => StageVoteCount.Set(index, value);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void ReadyRPC(int index, NetworkBool value) =>NetworkReadyArray.Set(index, value);
        
        #endregion
    }
}