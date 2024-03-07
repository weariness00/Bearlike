using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Script.GamePlay;
using Script.Manager;
using UnityEngine;

namespace GamePlay
{
    // A에서 B로 넘어갈 수 있는 포털
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : MonoBehaviour
    {
        // #region Networked Variable
        //
        // [Networked] public NetworkBool IsConnect { get; set; } // 포탈과 연결 되었는지
        //
        // #endregion
        [HideInInspector]public BoxCollider boxCollider;
        public SpawnPlace spawnPlace = new SpawnPlace();
        public Portal otherPortal; // 다른 포탈

        public LayerMask targetLayerMask; // 넘어갈 수 있는 대상
        public bool isConnect; // 포털과 연결된 상태인지

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.includeLayers = targetLayerMask;
            spawnPlace.Initialize();
        }

        private void OnCollisionEnter(Collision other)
        {
            Teleport(other.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            Teleport(other.gameObject);
        }

        public void SetPortal(Portal _otherP)
        {
            otherPortal = _otherP;
            _otherP.otherPortal = this;
        }

        private void Teleport(GameObject targetObject)
        {
            if (isConnect)
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
                }
                else
                {
                    targetObject.transform.position = spot.position;
                }
                
                DebugManager.Log($"{targetObject.name}객체가 {name}에서 {otherPortal.name}으로 이동");
            }
        }
    }
}

