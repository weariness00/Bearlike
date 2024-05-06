﻿using Data;
using Player;
using UnityEngine;

namespace GamePlay.GM
{
    public class GMMode : MonoBehaviour
    {
        public bool isOnGMMode = false;
        public PlayerController[] players;

        public GMMonsterSpawnerCanvas gmMonsterSpawnerCanvas;
        public GMItemSpawnerCanvas gmItemSpawnerCanvas;

        private void Start()    
        {
            isOnGMMode = false;
            Invoke(nameof(Init), 1);
        }
        
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F12))
                isOnGMMode = !isOnGMMode;
            
            if(!isOnGMMode)
                return;

            GMAction();
        }

        void Init()
        {
            players = FindObjectsOfType<PlayerController>();
            for (var i = 0; i < players.Length; i++)
            {
                var player = players[i];
                foreach (var (key, data) in UserData.Instance.UserDictionary)
                {
                    if (player.Object.Id == data.NetworkId)
                    {
                        (players[i], players[data.ClientNumber]) = (players[data.ClientNumber], players[i]);
                    }
                }
            }
        }
        // F1 : 스테이지 Clear
        // F2 : 스테이지 Over
        // F3 : 몬스터 생성 Canvas
        // F4 : 아이템 생성 Canvas
        
        // CTRL + 1~3 : 1~3 번 플레이어 100 데미지
        // ALT + 1~3 : 1~3번 플레이 부상에서 회복
        void GMAction()
        {
            if (Input.GetKey(KeyCode.CapsLock))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    players[0].status.ApplyDamageRPC(100, players[0].Object.Id);
                }
                else if (players.Length > 2 && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    players[1].status.ApplyDamageRPC(100, players[1].Object.Id);
                }
                else if (players.Length > 3 && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    players[2].status.ApplyDamageRPC(100, players[2].Object.Id);
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if(players[0].status.isInjury)
                        players[0].status.RecoveryFromInjuryActionRPC();
                    else if(players[0].status.isRevive)
                        players[0].status.RecoveryFromReviveActionRPC();
                }
                else if (players.Length < 2 && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if(players[1].status.isInjury)
                        players[1].status.RecoveryFromInjuryActionRPC();
                    else if(players[1].status.isRevive)
                        players[1].status.RecoveryFromReviveActionRPC();
                }
                else if (players.Length < 3 && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if(players[2].status.isInjury)
                        players[2].status.RecoveryFromInjuryActionRPC();
                    else if(players[2].status.isRevive)
                        players[2].status.RecoveryFromReviveActionRPC();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.F1))
                GameManager.Instance.currentStage.StageClearRPC();
            else if (Input.GetKeyDown(KeyCode.F2))
                GameManager.Instance.currentStage.StageOverRPC();
            else if( Input.GetKeyDown(KeyCode.F3))
                gmMonsterSpawnerCanvas.gameObject.SetActive(!gmMonsterSpawnerCanvas.gameObject.activeSelf);
            else if(Input.GetKeyDown(KeyCode.F4))
                gmItemSpawnerCanvas.gameObject.SetActive(!gmItemSpawnerCanvas.gameObject.activeSelf);
        }
    }
}

