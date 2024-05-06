using System.Collections.Generic;
using Data;
using Fusion;
using GamePlay.Stage;
using Photon;
using Player;
using UnityEngine;

namespace GamePlay.GM
{
    public class GMMode : NetworkBehaviourEx
    {
        public bool isOnGMMode = false;
        public List<PlayerController> playerList = new List<PlayerController>();

        public GMMonsterSpawnerCanvas gmMonsterSpawnerCanvas;

        private void Start()    
        {
            isOnGMMode = false;
        }
        
        public override void Spawned()
        {
            Invoke(nameof(Init), 1);
            Object.AssignInputAuthority(Runner.LocalPlayer);
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasInputAuthority)
                return;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F12))
                isOnGMMode = !isOnGMMode;
            
            if(!isOnGMMode)
                return;

            GMAction();
        }

        void Init()
        {
            playerList = new List<PlayerController>();
            foreach (var (playerRef, data) in UserData.Instance.UserDictionary)
            {
                var obj = Runner.FindObject(data.NetworkId);
                var pc = obj.GetComponent<PlayerController>();
                
                playerList.Add(pc);
            }
        }
        // F1 : 스테이지 Clear
        // F2 : 스테이지 Over
        // F3 : 몬스터 생성 Canvas
        
        // CTRL + 1~3 : 1~3 번 플레이어 100 데미지
        // ALT + 1~3 : 1~3번 플레이 부상에서 회복
        void GMAction()
        {
            if (Input.GetKey(KeyCode.CapsLock))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    playerList[0].status.ApplyDamageRPC(100, playerList[0].Object.Id);
                    playerList[0].status.InjuryTimer = TickTimer.CreateFromSeconds(Runner, 0);
                }
                else if (playerList.Count < 2 && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    playerList[1].status.ApplyDamageRPC(100, playerList[1].Object.Id);
                    playerList[1].status.InjuryTimer = TickTimer.CreateFromSeconds(Runner, 0);
                }
                else if (playerList.Count < 3 && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    playerList[2].status.ApplyDamageRPC(100, playerList[2].Object.Id);
                    playerList[2].status.InjuryTimer = TickTimer.CreateFromSeconds(Runner, 0);
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if(playerList[0].status.isInjury)
                        playerList[0].status.RecoveryFromInjuryActionRPC();
                    else if(playerList[0].status.isRevive)
                        playerList[0].status.RecoveryFromReviveActionRPC();
                }
                else if (playerList.Count < 2 && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if(playerList[1].status.isInjury)
                        playerList[1].status.RecoveryFromInjuryActionRPC();
                    else if(playerList[1].status.isRevive)
                        playerList[1].status.RecoveryFromReviveActionRPC();
                }
                else if (playerList.Count < 3 && Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if(playerList[2].status.isInjury)
                        playerList[2].status.RecoveryFromInjuryActionRPC();
                    else if(playerList[2].status.isRevive)
                        playerList[2].status.RecoveryFromReviveActionRPC();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.F1))
                StageClearRPC();
            else if (Input.GetKeyDown(KeyCode.F2))
                StageOverRPC();
            else if( Input.GetKeyDown(KeyCode.F3))
                gmMonsterSpawnerCanvas.gameObject.SetActive(!gmMonsterSpawnerCanvas.gameObject.activeSelf);
        }

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void StageClearRPC()
        {
            var stages = FindObjectsOfType<StageBase>();
            foreach (var stage in stages)
            {
                if(stage.StageInfo.stageType == StageType.None)
                    continue;
                stage.StageClear();
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void StageOverRPC()
        {
            var stages = FindObjectsOfType<StageBase>();
                
            foreach (var stage in stages)
            {
                if(stage.StageInfo.stageType == StageType.None)
                    continue;
                stage.StageOver();
            }
        }
        
        #endregion
    }
}

