using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using Manager;
using Photon;
using Status;
using UI.Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class BoxJesterBoom : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        [Networked] public Vector3 dir { get; set; }

        [Header("Bomb")] 
        [SerializeField] private GameObject bombPrefab;
        [SerializeField] private VisualEffect fireBall;
        
        private Vector3 Dest { get; set; }
        
        private float speed = 25f;
        private float _time = 1.5f;
        private float _height = 5f;
        
        public override void Spawned()
        {
            base.Spawned();
            fireBall.SendEvent("OnPlay");
            Destroy(gameObject, 5f);
            
            Dest = transform.position + dir * (speed * _time);

            StartCoroutine(BoomMoveCoroutine());
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (false == other.gameObject.CompareTag("Monster") && false == other.gameObject.CompareTag("Volume"))
            {
                Runner.SpawnAsync(bombPrefab, transform.position, transform.rotation, null, (runner, o) =>
                {
                    var bomb = o.GetComponent<BoxJesterBoomObject>();
                
                    bomb.OwnerId = OwnerId;
                });
                
                Destroy(gameObject);
            }
        }

        IEnumerator BoomMoveCoroutine()
        {
            float time = 0.0f;

            var pos = transform.position;
            float yOffset = 0.0f;

            while (time < 1.0f)
            {
                BoomVFXPositionRPC(transform.position);
                DebugManager.Log($"position : {transform.position}, time : {time}");
                yOffset = _height * 4.0f * (time - time * time * 2);
                transform.position = Vector3.Lerp(pos, Dest, time) + Vector3.up * yOffset;

                time += Time.deltaTime / _time;
                yield return null;
            }
        }
        
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void BoomVFXPositionRPC(Vector3 position)
        {
            fireBall.SetVector3("Position", position);
        }
    }
}