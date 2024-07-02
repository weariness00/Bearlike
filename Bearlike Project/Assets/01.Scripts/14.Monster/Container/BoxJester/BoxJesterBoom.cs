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
        [Networked] private Vector3 NetworkedDest { get; set; }
        [Networked] private Vector3 NetworkedStartPos { get; set; }
        [Networked] private float NetworkedTime { get; set; }

        [Header("Bomb")] [SerializeField] private GameObject bombPrefab;
        
        public Vector3 dir;
        private float speed;
        private float time;
        
        public override void Spawned()
        {
            Destroy(gameObject, 5f);
            
            speed = 25f;
            time = 1.5f;
            
            NetworkedStartPos = transform.position;
            NetworkedDest = transform.position + dir * (speed * time);
            NetworkedTime = time;

            BoomMoveRPC();
        }
        
        private void OnCollisionEnter(Collision other)
        {
            StatusBase otherStatus = null;

            if (false == other.gameObject.CompareTag("Monster"))
            {
                Runner.SpawnAsync(bombPrefab, transform.position, transform.rotation, null, (runner, o) =>
                {
                    var bomb = o.GetComponent<BoxJesterBoomObject>();
                
                    bomb.OwnerId = OwnerId;
                });
                
                Destroy(gameObject, 0f); // 터지는 이벤트 발생해야함
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void BoomMoveRPC()
        {
            transform.DOMoveX(NetworkedDest.x, NetworkedTime).SetEase(Ease.InQuad);
            transform.DOMoveZ(NetworkedDest.z, NetworkedTime).SetEase(Ease.InQuad);

            transform.DOMoveY(transform.position.y + 4, NetworkedTime * 0.3f).SetEase(Ease.InSine);
        }
    }
}