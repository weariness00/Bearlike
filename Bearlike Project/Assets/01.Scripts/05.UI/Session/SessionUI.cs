using System;
using System.Collections.Generic;
using Fusion;
using Photon;
using UnityEngine;
using Util;

namespace UI
{
    public class SessionUI : MonoBehaviour
    {
        public Transform spawnParent;
        public GameObject sessionBlockPrefab;

        private List<SessionBlockHandle> _sessionBlockHandles = new List<SessionBlockHandle>();

        private void Start()
        {
            NetworkManager.Instance.SessionListUpdateAction += SessionUpdate;
        }

        public void SessionUpdate(SessionInfo[] sessionInfos)
        {
            foreach (var sessionBlockHandle in _sessionBlockHandles)
            {
                Destroy(sessionBlockHandle.gameObject);
            }
            _sessionBlockHandles.Clear();
            
            sessionBlockPrefab.SetActive(true);
            foreach (var sessionInfo in sessionInfos)
            {
                var blockHandle = Instantiate(sessionBlockPrefab, spawnParent).GetComponent<SessionBlockHandle>();
                blockHandle.SetSessionInfo(sessionInfo);
                _sessionBlockHandles.Add(blockHandle);
            }
            sessionBlockPrefab.SetActive(false);
        }
    }
}