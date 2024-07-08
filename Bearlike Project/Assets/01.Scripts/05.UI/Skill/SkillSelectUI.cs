﻿using System.Linq;
using Fusion;
using Manager;
using Photon;
using Player;
using Skill;
using Status;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Skill
{
    public class SkillSelectUI : NetworkBehaviourEx
    {
        public Canvas canvas;
        public ToggleGroup toggleGroup;
        
        public GameObject selectUIBlockObject;

        [Header("Player")]
        public PlayerController playerController;

        private int _selectCount = 0; // 횟수가 남아있다면 스킬을 선택후 다시 스킬 선택창 리롤

        #region Member Funtion

        public int GetSelectCount() => _selectCount;
        public void AddSelectCount() => ++_selectCount;
        public void RemoveSelectCount() => --_selectCount;
        
        // Select UI를 초반 셋팅 해주는 함수
        public void SpawnSkillBlocks(int count)
        {
            gameObject.SetActive(true);
            UIManager.AddActiveUI(gameObject);
            
            // 이미 있는 스킬들 삭제
            var handles = toggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
            foreach (var handle in handles)
                Destroy(handle.gameObject);

            // 스킬 UI 생성
            bool isSuccessSelectSkillSpawn = false;
            UniqueRandom random = new UniqueRandom(0, SkillObjectList.SkillCount);
            for (int i = 0; i < count; i++)
            {
                // 이미 만렙인 스킬들은 제외
                SkillBase skill = null;
                while (true)
                {
                    var randomInt = random.RandomInt();
                    
                    // 모든 스킬들이 만렙이면 더이상 Block을 만들지 않는다.
                    if (randomInt == -1)
                    {
                        if(!isSuccessSelectSkillSpawn) gameObject.SetActive(false);
                        return;
                    }
                    
                    skill = SkillObjectList.GetFromIndex(randomInt);
                    foreach (var pcSkill in playerController.skillSystem.skillList)
                    {
                        // 같은 스킬이 존재하며 해당 스킬이 만렙이면 다시 찾는다.
                        if (pcSkill.id == skill.id && pcSkill.level.isMax)
                        {
                            skill = null;
                            break;
                        }
                    }

                    if (skill)
                        break;
                }

                isSuccessSelectSkillSpawn = true;
                
                if (playerController.skillSystem.TryGetSkillFromID(skill.id, out var hasSkill)) skill = hasSkill;
                else
                {   
                    skill.SetJsonData(SkillBase.GetInfoData(skill.id));
                    skill.status = skill.GetComponent<StatusBase>();
                    skill.Awake();
                }
                
                var obj = Instantiate(selectUIBlockObject, toggleGroup.transform);
                var handle = obj.GetComponent<SkillSelectBlockHandle>();
                obj.SetActive(true);
                handle.SettingBlock(skill);
                handle.button.onClick.AddListener(() => SelectSkill(skill.id));
            }
        }

        private async void SelectSkill(int id)
        {
            if (!playerController.skillSystem.TryGetSkillFromID(id, out var skill))
            {
                SpawnSkillRPC(id);
            }
            else
            {
                skill.LevelUpRPC(); 
            }

            RemoveSelectCount();
            if (_selectCount > 0)
                SpawnSkillBlocks(3);
            else
            {
                var handles = toggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
                foreach (var h in handles)
                    Destroy(h.gameObject); 
                gameObject.SetActive(false);
            }
        }
        
        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void SpawnSkillRPC(int skillId)
        {
            var skillObj = await NetworkManager.Runner.SpawnAsync(SkillObjectList.GetFromID(skillId).gameObject, Vector3.zero, Quaternion.identity, playerController.Object.InputAuthority);
            InitSpawnSkillRPC(skillObj.Id);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void InitSpawnSkillRPC(NetworkId skillID)
        {
            var skill = Runner.FindObject(skillID).GetComponent<SkillBase>();
            skill.gameObject.transform.SetParent(playerController.skillSystem.transform);
            skill.Earn(playerController.gameObject);
            skill.LevelUp();
            playerController.skillSystem.AddSkill(skill);
        }
        

        #endregion
    }
}

