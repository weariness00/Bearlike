using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Script.GamePlay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Photon
{
    public class NetworkSpawner : NetworkBehaviour
    {
        // 해당 random은 아래의 리스트의 원소상에서의 랜덤임
        public bool isRandomObject = false; // 랜덤한 객체를 소환할 것인지
        public bool isRandomPlace = false; // 랜덤한 위치에 소환할 것인지
        public bool isRandomInterval = false; // 랜덤한 간격에 소환할 것인지
        public int co;

        public List<NetworkPrefabRef> spawnObjectList = new List<NetworkPrefabRef>();
        public int[] spawnObjectOrders; // 스폰할 객체, 1개일 경우 해당 객체만 스폰 여러개일 경우 순차적으로 스폰
        private int _SpawnObjectOrderCount; // 현재 스폰할 객체
        private NetworkPrefabRef _currentSpawnObjectOrder;
        
        [SerializeField] SpawnPlace spawnPlace; // 스폰 위치
        public int[] spawnPlaces; // 스폰 위치 설정, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        private int _spawnPlaceCount;
        private Transform _currentSpawnPlace;

        public float[] spawnIntervals; // 스폰 간격, 1개일 경우 반복 여러개일 경우 순차적으로 실행
        private int _spawnIntervalCount;
        [Networked] private TickTimer currentSpawnInterval { get; set; } // 현재 스폰 간격
        
        [HideInInspector] public List<NetworkObject> networkObjects;
        private Coroutine _currentSpawnCoroutine = null;

        public override void Spawned()
        {
            SpawnStart();
        }

        public void SpawnStart()
        {
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
            if(_currentSpawnCoroutine != null) StopCoroutine(_currentSpawnCoroutine);
        }

        // 정지된 스폰 이어하기
        public void SpawnResume()
        {
            _currentSpawnCoroutine = StartCoroutine(SpawnCoroutine());
        }

        void NextPlace()
        {
            if (isRandomPlace)
                _spawnPlaceCount = Random.Range(0, spawnPlaces.Length);
            else
                _spawnPlaceCount++;

            if (spawnPlaces.Length - 1 < _spawnPlaceCount) _spawnPlaceCount = 0;
            _currentSpawnPlace = spawnPlace.GetSpot(_spawnPlaceCount);
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
        
        void NextInterval()
        {
            if (isRandomInterval)
                _spawnIntervalCount =  Random.Range(0, spawnIntervals.Length);
            else
                _spawnIntervalCount++;
            
            if (spawnIntervals.Length - 1 < _spawnIntervalCount) _spawnIntervalCount = 0;
            currentSpawnInterval = TickTimer.CreateFromSeconds(Runner, spawnIntervals[_spawnIntervalCount]);
        }
        
        /// <summary>
        /// Photon Fusion2 전용
        /// </summary>
        async Task SpawnTask()
        {
            await Runner.SpawnAsync(_currentSpawnObjectOrder, _currentSpawnPlace.position);
            NextObject();
            NextPlace();
            NextInterval();
        }

        private IEnumerator SpawnCoroutine()
        {
            var c = 0;
            while (true)
            {
                yield return null;
                if (currentSpawnInterval.Expired(Runner) == false) continue; // 스폰 간격만큼의 시간이 지났는지 확인
                yield return SpawnTask();
                if (++c > co - 1) SpawnStop();
            }
        }
    }
}

