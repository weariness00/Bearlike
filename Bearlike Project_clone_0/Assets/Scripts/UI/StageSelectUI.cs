using System;
using System.Collections.Generic;
using GamePlay.StageLevel;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUI : MonoBehaviour
    {
        private List<StageLevelBase> _stageList = new List<StageLevelBase>();
        public int selectStageIndex = 0;
        [Tooltip("스테이지 선택지 개수")]public int stageChoiceCount = 2;

        [Header("스테이지 토글 그룹")] 
        public Transform stageToggleGroup;
        public GameObject togglePrefab;
        private List<GameObject> _toggleObjectList = new List<GameObject>();

        private void Start()
        {
            SettingStageUI();
        }

        public void SettingStageUI()
        {
            foreach (var toggleObject in _toggleObjectList)
            {
                Destroy(toggleObject);
            }
            
            _toggleObjectList.Clear();
            _stageList.Clear();
            for (int i = 0; i < stageChoiceCount; i++)
            {
                var index = i;
                var toggleObject = Instantiate(togglePrefab, stageToggleGroup);
                var toggle = toggleObject.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((value) =>
                {
                    if (value)
                    {
                        selectStageIndex = index;
                    }
                });
                toggleObject.SetActive(true);
                _toggleObjectList.Add(toggleObject);
                _stageList.Add(GameManager.Instance.GetRandomStage());
            }
        }

        public void SetStage()
        {
            GameManager.Instance.SetStage(_stageList[selectStageIndex]);
            
            gameObject.SetActive(false);
        }
    }
}

