using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Manager;
using Photon;
using Script.GamePlay;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Photon
{
    public class NetworkSpawner : NetworkBehaviourEx
    {
        public bool isStartSpawn = false; // 이 컴포넌트가 생성되자마자 스폰하게 할 것인지
        public bool isLoop = false; // spawnCount가 max가 아니면 계속 소환하게 할 것인지
        
        // 해당 random은 아래의 리스트의 원소상에서의 랜덤임
        public bool isRandomObject = false; // 랜덤한 객체를 소환할 것인지
        public bool isRandomPlace = false; // 랜덤한 위치에 소환할 것인지
        public bool isRandomInterval = false; // 랜덤한 간격에 소환할 것인지
        public Transform parentTransform; // 소환된 객체가 갈 부모 객체 ( 하이어라키에서 관리 편의성을 위해 사용 )
        public StatusValue<int> spawnCount = new StatusValue<int>(); // 현재 스폰된 갯수
        
        public List<NetworkPrefabRef> spawnObjectList = new List<NetworkPrefabRef>();
        public int[] spawnObjectOrders; // 스폰할 객체, 1개일 경우 해당 객체만 스폰 여러개일 경우 순차적으로 스폰
        private int _SpawnObjectOrderCount; // 현재 스폰할 객체
        private NetworkPrefabRef _currentSpawnObjectOrder;
        
        [SerializeField] SpawnPlace spawnPlace; // 스폰 위치
        [Tooltip("다음 스폰 위치 순서 인덱스 SpawnPlace를 기반으로 한다.")] public int[] spawnPlaceOrders; // 스폰 위치 설정, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        private int _spawnPlaceCount;
        private Transform _currentSpawnPlace;

        public float[] spawnIntervals; // 스폰 간격, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        private int _spawnIntervalCount;
        [Networked] private TickTimer CurrentSpawnInterval { get;set; } // 현재 스폰 간격
        [Networked] public NetworkBool IsSpawn { get; set; } // 현재 스폰중인지
        
        public Action<GameObject> SpawnSuccessAction; // 스폰 되면 실행하는 이벤트
        private Coroutine _currentSpawnCoroutine = null; // 스폰 코루틴

        void OnDestroy()
        {
            CurrentSpawnInterval = TickTimer.None;
        }
        
        public override void Spawned()
        {
            if (isStartSpawn)
            {
                SpawnStartRPC();
            }
            
            if (spawnPlace.Length == 0)
            {
                _currentSpawnPlace = gameObject.transform;
            }
        }

        public void SpawnStart()
        {
            if (spawnObjectList.Count == 0)
            {
                DebugManager.LogWarning($"{name}에 스폰할 네트워크 객체가 없습니다.");
                return;
            }
            if (IsSpawn)
            {
                return;
            }

            SetIsSpawnRPC(true);
            
            spawnPlace.Initialize();

            _spawnPlaceCount = -1;
            _spawnIntervalCount = -1;
            _SpawnObjectOrderCount = -1;

            NextObject();
            NextPlace();
            NextInterval();

            if(_currentSpawnCoroutine != null) StopCoroutine(_currentSpawnCoroutine);
            _currentSpawnCoroutine = StartCoroutine(SpawnCoroutine());
        }

        // 스폰 정지
        public void SpawnStop()
        {
            SetIsSpawnRPC(false);
            if(_currentSpawnCoroutine != null) StopCoroutine(_currentSpawnCoroutine);
        }

        // 정지된 스폰 이어하기
        public void SpawnResume()
        {
            _currentSpawnCoroutine = StartCoroutine(SpawnCoroutine());
        }
        
        void NextObject()
        {
            if (isRandomObject)
                _SpawnObjectOrderCount = Random.Range(0, spawnObjectOrders.Length);
            else
                _SpawnObjectOrderCount++;

            if (spawnObjectOrders.Length - 1 < _SpawnObjectOrderCount) _SpawnObjectOrderCount = 0;
            _currentSpawnObjectOrder = spawnObjectList[_SpawnObjectOrderCount];
        }

        void NextPlace()
        {
            if (spawnPlace.Length == 0) { return; }
            
            _spawnPlaceCount++;
            int length = 0;
            
            // 길이 할당
            if (spawnPlaceOrders.Length != 0)
            {
                length = spawnPlaceOrders.Length;
            }
            else
            {
                length = spawnPlace.Length;
            }
            
            // 인덱스 설정
            if (isRandomPlace)
            {
                _spawnPlaceCount = Random.Range(0, length);
            }

            if (_spawnPlaceCount >= length) _spawnPlaceCount = 0;
            
            // 위치 할당
            _currentSpawnPlace = spawnPlace.GetSpot(_spawnPlaceCount);
        }
        
        void NextInterval()
        {
            if (spawnIntervals.Length == 0)
            {
                CurrentSpawnInterval = TickTimer.CreateFromSeconds(Runner, 1f);
                return;
            }

            float interval = 0;
            
            if (isRandomInterval)
                _spawnIntervalCount =  Random.Range(0, spawnIntervals.Length);
            else
                _spawnIntervalCount++;

            if (_spawnIntervalCount >= spawnIntervals.Length) _spawnIntervalCount = 0;
            
            if (spawnIntervals.Length == 0) interval = 0;
            else interval = spawnIntervals[_spawnIntervalCount];
            
            CurrentSpawnInterval = TickTimer.CreateFromSeconds(Runner, interval);
        }
        
        /// <summary>
        /// Photon Fusion2 전용
        /// </summary>
        async Task SpawnTask()
        {
            try
            {
                var obj = await Runner.SpawnAsync(_currentSpawnObjectOrder, _currentSpawnPlace.position, _currentSpawnPlace.rotation);
                SetParentRPC(obj.Id);
                SpawnSuccessAction?.Invoke(obj.gameObject);
                NextObject();
                NextPlace();
                NextInterval();
            
                DebugManager.Log("네트워크 객체 소환\n" +
                                 $"이름 : {obj.name}");
            }
            catch (Exception e)
            {
                DebugManager.LogError(e);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetParentRPC(NetworkId networkID)
        {
            if (parentTransform == null)
            {
                return;
            }
            
            var newParentNetworkObject = Runner.FindObject(networkID);
            
            if (newParentNetworkObject != null)
            {
                newParentNetworkObject.transform.SetParent(parentTransform);
            }
        }

        private IEnumerator SpawnCoroutine()
        {
            if (spawnObjectList.Count == 0)
            {
                Debug.LogWarning("Null Reference Is Spawn Object List");
                yield break;
            }
            
            spawnCount.Current = 0;
            while (true)
            {
                yield return null;
                if (CurrentSpawnInterval.Expired(Runner) == false) continue; // 스폰 간격만큼의 시간이 지났는지 확인
                if (spawnCount.isMax)
                {
                    if (isLoop) continue;
                    SpawnStop();
                    yield break;
                }
                yield return SpawnTask();
                SetSpawnCountRPC(StatusValueType.Current,++spawnCount.Current);
            }
        }

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsSpawnRPC(NetworkBool value) => IsSpawn = value;

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void SetSpawnCountRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Min:
                    spawnCount.Min = value;
                    break;
                case StatusValueType.Current:
                    spawnCount.Current = value;
                    break;
                case StatusValueType.Max :
                    spawnCount.Max = value;
                    break;
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SpawnStartRPC() => SpawnStart();

        #endregion
    }
}

