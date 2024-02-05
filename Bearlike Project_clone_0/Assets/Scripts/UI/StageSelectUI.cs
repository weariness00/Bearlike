using System.Collections.Generic;
using Fusion;
using GamePlay.StageLevel;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUI : NetworkBehaviour
    {
        [Networked][Capacity(3)]public NetworkArray<int> SelectStageIndex { get; }
        public List<StageLevelBase> nextStageList = new List<StageLevelBase>();
        [Networked] [Tooltip("스테이지 선택지 개수")] public int StageChoiceCount { get; set; } = 2;

        [Header("스테이지 토글 그룹")] 
        public Transform stageToggleGroup;
        public GameObject togglePrefab;
        private List<GameObject> _toggleObjectList = new List<GameObject>();

        #region Vraiable RPC Function

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void AddNextStageListRPC(StageLevelType stageLevelType)
        {
            if (nextStageList.Count >= StageChoiceCount)
            {
                nextStageList.Clear();
            }

            var stage = GameManager.Instance.GetStageIndex((int)stageLevelType);
            nextStageList.Add(stage);
        }

        #endregion
        
        private void Start()
        {
            GameManager.Instance.stageClearAction += SettingStageUI;
        }

        public override void Spawned()
        {
            base.Spawned();
            // SettingStageUI();
        }

        public void SettingStageUI()
        {
            if (Runner.IsServer == false)
            {
                return;
            }
            
            foreach (var toggleObject in _toggleObjectList)
            {
                Destroy(toggleObject);
            }
            
            _toggleObjectList.Clear();
            for (int i = 0; i < StageChoiceCount; i++)
            {
                var index = i;
                var toggleObject = Instantiate(togglePrefab, stageToggleGroup);
                var toggle = toggleObject.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((value) =>
                {
                    if (value)
                    {
                        SelectStageIndex.Set(i, SelectStageIndex.Get(index) + 1);
                    }
                    else
                    {
                        SelectStageIndex.Set(i, SelectStageIndex.Get(index) - 1);
                    }
                });
                
                AddNextStageListRPC(GameManager.Instance.GetRandomStage().stageLevelInfo.StageLevelType);

                toggleObject.SetActive(true);
                _toggleObjectList.Add(toggleObject);
            }
        }

        public void SetStage()
        {
            int bicSelectIndex = 0;
            foreach (var value in SelectStageIndex)
            {
                if (bicSelectIndex < value)
                {
                    bicSelectIndex = value;
                }
            }
            
            GameManager.Instance.SetStage(GameManager.Instance.stageList[bicSelectIndex]);
            
            gameObject.SetActive(false);
        }
    }
}

