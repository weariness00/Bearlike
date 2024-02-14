using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GamePlay.StageLevel;
using Manager;
using Script.Data;
using Script.Manager;
using Script.Photon;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUI : NetworkBehaviour
    {
        private int clientNumber = -1;

        #region Network Variable

        private NetworkButtons buttons;
        [Networked] private NetworkBool IsServerSetting { get; set; }
        [Networked] [Capacity(3)] public NetworkArray<NetworkButtons> NetworkButtonsArray { get; }
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
            clientNumber = UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).ClientNumber;

            selectButton.onClick.AddListener(() => { buttons.Set(0, true); });

            StageLevelBase.stageClearAction += SettingServer;
            StageLevelBase.stageClearAction += SettingStageUI;
            SettingServer();
            SettingStageUI();
        }

        public override void FixedUpdateNetwork()
        {
            NetworkButtonsArray.Set(clientNumber, buttons);
        }

        public void SettingStageInfo()
        {
            for (int i = 0; i < StageChoiceCount; i++)
            {
                var index = i;
                var stage = GameManager.Instance.GetRandomStage();
                NetworkStageLevelTypes.Set(index, stage.stageLevelInfo.StageLevelType);
            }
        }

        public void SettingServer()
        {
            if (Runner.IsServer)
            {
                IsServerSetting = false;
                SettingStageInfo();
                IsServerSetting = true;
            }
        }

        public void SettingStageUI() => StartCoroutine(SettingStageUICoroutine());
        IEnumerator SettingStageUICoroutine()
        {
            while (IsServerSetting == false)
            {
                yield return null;
            }
            
            foreach (var toggleObject in stageSelectUIHandlerList)
            {
                Destroy(toggleObject);
            }

            stageSelectUIHandlerList.Clear();
            for (int i = 0; i < StageChoiceCount; i++)
            {
                var index = i;
                var stageType = NetworkStageLevelTypes.Get(index);
                var stage = GameManager.Instance.GetStageIndex((int)stageType);
                var stageSelectUIObject = Instantiate(stageSelectUIPrefab, stageToggleGroup);
                var stageSelectUIHandler = stageSelectUIObject.GetComponent<StageSelectUIHandler>();
                stageSelectUIHandler.toggle.onValueChanged.AddListener((value) =>
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

                    UpdateVoteTextRPC();
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
            int bicSelectIndex = 0;
            for (int i = 0; i < StageVoteCount.Length; i++)
            {
                int vote = StageVoteCount.Get(i);
                if (bicSelectIndex < vote)
                {
                    bicSelectIndex = i;
                }
            }

            GameManager.Instance.SetStage(GameManager.Instance.stageList[bicSelectIndex]);

            gameObject.SetActive(false);
        }

        #region Vraiable RPC Function
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void UpdateVoteTextRPC()
        {
            for (int i = 0; i < StageChoiceCount; i++)
            {
                int stageVoteCount = StageVoteCount.Get(i);
                stageSelectUIHandlerList[i].voteText.text = stageVoteCount.ToString();
            }
        }

        #endregion
    }
}