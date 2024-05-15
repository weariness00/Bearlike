using System.Linq;
using Photon;
using Player;
using Skill;
using Status;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Skill
{
    public class SkillSelectUI : MonoBehaviour
    {
        public Canvas canvas;
        public ToggleGroup toggleGroup;
        
        public GameObject selectUIBlockObject;

        [Header("Player")]
        public PlayerController playerController;
        
        #region Unity Event Function

        private void Awake()
        {
            selectUIBlockObject.GetComponent<SkillSelectBlockHandle>().DoubleClickEvent += SelectSkill;
        }

        private void Update()
        {
            // 해당 스킬을 업그레이드
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SelectSkill();
            }
        }

        #endregion

        #region Member Funtion

        // Select UI를 초반 셋팅 해주는 함수
        public void SpawnSkillBlocks(int count)
        {
            gameObject.SetActive(true);
            
            // 이미 있는 스킬들 삭제
            var handles = toggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
            foreach (var handle in handles)
                Destroy(handle.gameObject);

            // 스킬 UI 생성
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
                        gameObject.SetActive(false);
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
                
                if (playerController.skillSystem.TryGetSkillFromID(skill.id, out var hasSkill)) skill = hasSkill;
                else skill.SetJsonData(SkillBase.GetInfoData(skill.id));
                
                var obj = Instantiate(selectUIBlockObject, toggleGroup.transform);
                var handle = obj.GetComponent<SkillSelectBlockHandle>();
                obj.SetActive(true);
                handle.SettingBlock(skill);
            }
        }

        private async void SelectSkill()
        {
            var activeToggle = toggleGroup.GetFirstActiveToggle();
            var handle = activeToggle.GetComponent<SkillSelectBlockHandle>();
            if (!playerController.skillSystem.TryGetSkillFromID(handle.id, out var skill))
            {
                await NetworkManager.Runner.SpawnAsync(
                    SkillObjectList.GetFromID(handle.id).gameObject, Vector3.zero, Quaternion.identity, playerController.Object.InputAuthority,
                    (runner, obj) =>
                    {
                        obj.transform.SetParent(playerController.skillSystem.transform);
                        skill = obj.GetComponent<SkillBase>();
                        skill.ownerPlayer = playerController;
                        skill.LevelUp();
                        skill.Earn(playerController.gameObject);
                    });
                playerController.skillSystem.AddSkill(skill);
            }
            else
            {
                skill.LevelUpRPC(); 
            }
            
            gameObject.SetActive(false);
        }
        
        #endregion
    }
}

