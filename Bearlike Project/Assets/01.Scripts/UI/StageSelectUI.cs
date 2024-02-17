using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GamePlay.StageLevel;
using Manager;
using Photon;
using Script.Data;
using Script.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUI : NetworkBehaviour
    {
        private int clientNumber = -1;

        #region Network Variable

        private ChangeDetector _changeDetector;
        [Networked] private NetworkBool IsServerSetting { get; set; }
        [Networked] [Capacity(3)] private NetworkArray<NetworkBool> NetworkReadyArray { get; } // 투표를 마치고 준비가 되었는지
        [Networked] [Capacity(3)] public NetworkArray<int> StageVoteCount { get; }
        [Networked] [Capacity(3)] public NetworkArray<StageLevelType> NetworkStageLevelTypes { get; }
        [Networked] [Tooltip("스테이지 선택지 개수")] public int StageChoiceCount { get; set; } = 2;

        #endregion

        public List<StageLevelBase> nextStageList = new List<StageLevelBase>();

        [Header("스테이지 선택 버튼")] public Button selectButton;

        [Header("스테이지 정보 그룹")] public Transform stageToggleGroup;
        public GameObject stageSelectUIPrefab;
        private List<StageSelectUIHandler> stageSelectUIHandlerList = new List<StageSelectUIHandler>();

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            
            clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;

            StageLevelBase.stageClearAction += SettingServer;
            SettingStageInfo();
            SettingStageUI();
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
                    case nameof(IsServerSetting):
                        if (IsServerSetting)
                        {
                            SettingStageUI();
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
            for (int i = 0; i < StageChoiceCount; i++)
            {
                int stageVoteCount = StageVoteCount.Get(i);
                stageSelectUIHandlerList[i].voteText.text = stageVoteCount.ToString();
            }
        }

        // 투표를 마쳤다는 표시
        public void Ready()
        {
            ReadyRPC(clientNumber);
            
            DebugManager.ToDo("투표 관련 UI도 만들어주고 업데이트 해줘야한다.");
        }

        public void SettingStageInfo()
        {
            if (Runner.IsServer == false)
            {
                for (int i = 0; i < StageChoiceCount; i++)
                {
                    var index = i;
                    var stage = GameManager.Instance.GetRandomStage();
                    NetworkStageLevelTypes.Set(index, stage.stageLevelInfo.StageLevelType);
                }
            }
        }

        public void SettingServer()
        {
            if (Runner.IsServer == false)
            {
                IsServerSetting = false;
                SettingStageInfo();
                IsServerSetting = true;
            }
        }

        void SettingStageUI()
        {
            foreach (var toggleObject in stageSelectUIHandlerList)
            {
                Destroy(toggleObject);
            }

            stageSelectUIHandlerList.Clear();
            for (int i = 0; i < StageChoiceCount; i++)
            {
                var index = i;
                var stageType = NetworkStageLevelTypes.Get(i);
                var stage = GameManager.Instance.GetStageIndex((int)stageType);
                var stageSelectUIObject = Instantiate(stageSelectUIPrefab, stageToggleGroup);
                var stageSelectUIHandler = stageSelectUIObject.GetComponent<StageSelectUIHandler>();
                stageSelectUIHandler.toggle.onValueChanged.AddListener((value) =>
                {
                    FixeVoteCountRPC(index, value);
                });
                stageSelectUIHandler.gameObject.SetActive(true);
                stageSelectUIHandler.Setting(stage.stageLevelInfo);
                
                if (nextStageList.Count >= StageChoiceCount)
                {
                    nextStageList.Clear();
                }
                stageSelectUIHandlerList.Add(stageSelectUIHandler);
                nextStageList.Add(stage);
            }

            gameObject.SetActive(true);
        }

        public void SetStage()
        {
            if (Runner.IsServer)
            {
                int bicSelectIndex = 0;
                for (int i = 0; i < StageVoteCount.Length; i++)
                {
                    int vote = StageVoteCount.Get(i);
                    if (bicSelectIndex < vote)
                    {
                        bicSelectIndex = i;
                    }
                }

                GameManager.Instance.SetStage(nextStageList[bicSelectIndex]);
            }

            gameObject.SetActive(false);
        }

        #region Vraiable RPC Function

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
        public void ReadyRPC(int index)
        {
            var value = NetworkReadyArray.Get(index);
            NetworkReadyArray.Set(index, !value);
        }

        #endregion
    }
}