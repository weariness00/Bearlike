using System;
using System.Collections.Generic;
using Fusion;
using Manager;
using Script.Manager;
using Script.Monster;
using Script.Photon;
using Scripts.State.GameStatus;
using UnityEngine;

namespace GamePlay.StageLevel
{
    public class StageLevelBase : NetworkBehaviour
    {
        #region Stage Info Variable

        [Header("스테이지 정보")]
        public StageLevelInfo stageLevelInfo;

        [Header("맵 정보")] 
        public Vector3 mapSize;

        [Header("몬스터 정보")]
        public List<NetworkSpawner> monsterSpawnerList = new List<NetworkSpawner>();
        public Transform monsterParentTransform = null;

        public bool isStageClear = false;
        public int nextStageSceneIndex = (int)SceneType.StageDestroy;

        #endregion
        
        private Action _updateStageAction = null;
        
        #region Destroy Variable

        public StatusValue<float> destroyTimeLimit = new StatusValue<float>();
        public StatusValue<int> monsterKillCount = new StatusValue<int>();

        #endregion
        
        public string mapInfo;
        public string goalInfo; 
        public string awardInfo;
        public string difficultInfo;

        #region Unity Event Function

        private void Awake()
        {
            var camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                DestroyImmediate(camera.gameObject);
            }
        }

        private void Start()
        {
            stageLevelInfo.AliveMonsterCount.isOverMax = true;
            if (monsterParentTransform == null) { monsterParentTransform = gameObject.transform; }

            StageMemoryOptimize();
            switch (stageLevelInfo.StageLevelType)
            {
                case StageLevelType.Destroy:
                    _updateStageAction = DestroyUpdate;
                    break;
            }
            
            GameManager.Instance.sceneList.Add(this);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            _updateStageAction?.Invoke();
        }

        #endregion

        #region Defualt Function

        // 스테이지 타입에 맞지 않는 멥버 변수 메모리 해제
        void StageMemoryOptimize()
        {
            if (stageLevelInfo.StageLevelType != StageLevelType.Destroy)
            {
                destroyTimeLimit = null;
                monsterKillCount = null;
            }
        }
        
        // 스테이지에 들어서면 정보에 맞춰 해당 스테이지를 셋팅 해준다.
        public void StageSetting()
        {
            StartMonsterSpawn();
        }

        public void StartMonsterSpawn()
        {
            foreach (var monsterSpawner in monsterSpawnerList)
            {
                if (monsterSpawner.spawnObjectList.Count == 0)
                {
                    DebugManager.LogWarning("스폰할 몬스터가 리스트에 없습니다.");
                    continue;
                }
                if (stageLevelInfo.AliveMonsterCount.isMax)
                {
                    break;
                }
                stageLevelInfo.AliveMonsterCount.Current += monsterSpawner.spawnCount.Max;
                monsterSpawner.SpawnSuccessAction += (obj) =>
                {
                    obj.transform.parent = monsterParentTransform;
                    
                    var monster = obj.GetComponent<MonsterBase>();
                    monster.DieAction += () => { --stageLevelInfo.AliveMonsterCount.Current; };
                };
                monsterSpawner.SpawnStart();
            }
        }

        void StageClear()
        {
            NetworkManager.LoadScene(SceneType.StageDestroy);
            
        }
        
        #endregion

        #region Destroy Function

        void DestroyInit()
        {
            monsterKillCount.isOverMax = true;
        }

        void DestroyUpdate()
        {
            destroyTimeLimit.Current += Runner.DeltaTime;
            if (monsterKillCount.isMax)
            {
                DebugManager.Log("스테이지 클리어\n" +
                                 $"스테이지 모드 :{stageLevelInfo.StageLevelType}");
                isStageClear = true;
            }
            else if (destroyTimeLimit.isMax)
            {
                DestroyOver();
                _updateStageAction = null;
                return;
            }
        }

        void DestroyOver()
        {
            
        }

        #endregion
    }
}