using System;
using System.Collections.Generic;
using Manager;
using Photon;
using Player;
using Script.GamePlay;
using UI;
using UnityEngine;
using UnityEngine.VFX;
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
        public List<VisualEffect> portalVFXList = new List<VisualEffect>();
        
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

            InteractInit();
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

                if (targetObject.CompareTag("Player"))
                {
                    var pc = targetObject.GetComponent<PlayerController>();
                    pc.SetPositionRPC(spot.position);
                    pc.SetLookRotationRPC(spot.forward);
                    
                }
                else if (targetObject.TryGetComponent(out NetworkBehaviourEx networkEx))
                {
                    networkEx.SetPositionRPC(spot.position);
                    networkEx.SetRotationRPC(spot.rotation);
                }
                DebugManager.Log($"{targetObject.name}객체가 {name}에서 {otherPortal.name}으로 이동");
            }
        }

        public void InteractInit()
        {
            InteractEnterAction += SetInteractUI;
            InteractKeyDownAction += Teleport;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractKeyDownAction { get; set; }
        public Action<GameObject> InteractKeyPressAction { get; set; }
        public Action<GameObject> InteractKeyUpAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }

        void SetInteractUI(GameObject targetObject)
        {
            InteractUI.SetKeyActive(true);
            InteractUI.KeyCodeText.text = "F";
        }
    }
}

