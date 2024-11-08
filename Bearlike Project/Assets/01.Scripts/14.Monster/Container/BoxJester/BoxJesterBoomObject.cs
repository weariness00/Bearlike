﻿using System;
using System.Collections.Generic;
using Fusion;
using Photon;
using Status;
using UI.Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class BoxJesterBoomObject : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        public ParticleSystem BoomEffect;
        
        // public MonsterStatus status;
        
        private HashSet<GameObject> damagePlayerSet = new HashSet<GameObject>(); // 이미 대미지를 입은 플레이어인지

        private void Awake()
        {
            // status = gameObject.GetOrAddComponent<MonsterStatus>();
            
            // status.damage.Max = 10;
            // status.damage.Current = 10;
        }
        
        public override void Spawned()
        {
            BoomEffect.Play();
            Destroy(gameObject, 1.5f);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (true == other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    if (damagePlayerSet.Contains(otherStatus.gameObject) == false)
                    {
                        // status.AddAdditionalStatus(otherStatus);
                        // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                        //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                        // status.RemoveAdditionalStatus(otherStatus);

                        otherStatus.ApplyDamageRPC(10, DamageTextType.Normal, OwnerId);
                        
                        damagePlayerSet.Add(otherStatus.gameObject);
                    }
                }
            }
        }
    }
}