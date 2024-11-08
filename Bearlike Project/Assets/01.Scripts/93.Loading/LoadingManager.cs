﻿using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Status;
using UnityEngine;
using Util;

namespace Loading
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        // 로딩 시작시 호출
        // _waitCount = 0에서 1로 바뀌면 호출됨
        public static Action StartAction 
        {
            get => Instance._startAction;
            set => Instance._startAction = value;
        }
        
        //로딩 끝날시 호출
        // _waitCount <= 0 으면 호출됨
        public static Action EndAction    
        {
            get => Instance._endAction;
            set => Instance._endAction = value;
        }

        // 로딩 도중 로딩에 사용되는 프로세스 이름을 반환한다.
        public static Action<string> LoadingProcessSuccess
        {
            get => Instance._loadingProcessSuccess;
            set => Instance._loadingProcessSuccess = value;
        }
        

        public static bool IsLoading => Instance.isLoading; 

        // 초기화
        public static void Initialize()
        {
            if(Instance.isDebug) DebugManager.Log("Loading 초기화");

            StartAction = null;
            EndAction = null;
            LoadingProcessSuccess = null;
            Instance.isLoading = false;

            Instance.refValue.Max = 0;
            Instance._waitCount = 0;
            Instance._downByte.Max = 0;
        }
        
        // 한번에 많은 로딩 카운트를 추가할 때 사용
        public static void AddWaitCount(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddWait();
            }
        }

        // 로딩 추가
        public static void AddWait(int downByte = 0) => Instance.AddWaitDownByte(downByte);
        // 로딩 끝
        public static void EndWait(string processName = null, int downByte = 0)
        {
            if(Instance.isDebug) DebugManager.Log("Loading 제거");
            
            --Instance._waitCount;
            Instance._downByte.Current += downByte;
            ++Instance.refValue.Current;

            if (Instance._waitCount <= 0)
            {
                Instance.EndWaitActionEvent();
            }
            
            if(processName != null) LoadingProcessSuccess?.Invoke(processName);
        }

        public static bool HasWaitName(string waitName) => Instance.endWaitStringSet.Contains(waitName);
        public static void EndWaitName(string waitName) => Instance.endWaitStringSet.Add(waitName);

        [HideInInspector] public StatusValue<int> refValue;
        [SerializeField] private int _waitCount; // 현재 로딩되어야 할 카운트
        [SerializeField] private StatusValue<int> _downByte = new StatusValue<int>(); // 로딩에 필요한게 용량 부분인지
        [SerializeField] private HashSet<string> endWaitStringSet; // loading이 완료된 것에 이름을 담아주는 Set
        [SerializeField] private bool isDebug;

        private Action _startAction;
        private Action _endAction;
        private Action<string> _loadingProcessSuccess;
        private bool isLoading; // 로딩중인지

        private void AddWaitDownByte(int downByte = 0)
        {
            if (isLoading == false)
            {
                if(isDebug) DebugManager.Log($"Loading 시작");
                
                _waitCount = 0;
                _downByte.Max = 0;
                
                StartAction?.Invoke();

                isLoading = true;
            }

            if(isDebug) DebugManager.Log("Loading 추가");

            ++refValue.Max;
            ++_waitCount;
            _downByte.Max += downByte;
        }

        private Coroutine _endWaitCoroutine;
        private void EndWaitActionEvent()
        {
            if(_endWaitCoroutine != null) StopCoroutine(_endWaitCoroutine); 
            _endWaitCoroutine = StartCoroutine(EndWaitCoroutine());
        }
        private IEnumerator EndWaitCoroutine()
        {
            yield return new WaitForSeconds(1f);

            if (_waitCount <= 0)
            {
                if(isDebug) DebugManager.Log($"Loading 완료");

                EndAction?.Invoke();
                isLoading = false;
            }
        }
    }
}