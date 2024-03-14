using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Manager;
using Script.GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace GamePlay
{
    // A에서 B로 넘어갈 수 있는 포털
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : MonoBehaviour, IInteract
    {
        // #region Networked Variable
        //
        // [Networked] public NetworkBool IsConnect { get; set; } // 포탈과 연결 되었는지
        //
        // #endregion

        [HideInInspector]public BoxCollider boxCollider;
        public SpawnPlace spawnPlace = new SpawnPlace();
        public Portal otherPortal; // 다른 포탈

        private bool _isConnect; // 포털과 연결된 상태인지
        public bool IsConnect
        {
            get => _isConnect;
            set => _isConnect = IsInteract = value;
        }

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            
            gameObject.layer = LayerMask.NameToLayer("Portal");
            spawnPlace.Initialize();

            InteractAction += Teleport;
        }

        public void SetPortal(Portal _otherP)
        {
            otherPortal = _otherP;
            _otherP.otherPortal = this;
        }

        public void Teleport(GameObject targetObject)
        {
            if (IsConnect)
            {
                if (otherPortal == null)
                {
                    return;
                }

                var spot = otherPortal.spawnPlace.GetRandomSpot(); // 이동할 위치

                if (targetObject.layer == LayerMask.NameToLayer("Player"))
                {
                    var simpleKCC = targetObject.transform.root.GetComponent<SimpleKCC>();
                    simpleKCC.SetPosition(spot.position);
                    simpleKCC.SetLookRotation(spot.forward);
                }
                else
                {
                    targetObject.transform.position = spot.position;
                    targetObject.transform.rotation = spot.rotation;
                }
                
                DebugManager.Log($"{targetObject.name}객체가 {name}에서 {otherPortal.name}으로 이동");
            }
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractAction { get; set; }
    }
}

