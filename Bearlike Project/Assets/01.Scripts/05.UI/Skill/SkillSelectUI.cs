using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Manager;
using Photon;
using Player;
using Skill;
using Status;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util;
using Util.UnityEventComponent;

namespace UI.Skill
{
    public class SkillSelectUI : NetworkBehaviourEx
    {
        public Canvas canvas;
        [Header("Player")]
        public PlayerController playerController;
        
        [Header("Random Select View")]
        [SerializeField] private ScrollRect randomSelectScroll;
        [SerializeField] private Toggle randomViewToggle;
        public ToggleGroup toggleGroup;
        public GameObject selectUIBlockObject;

        private readonly List<SkillSelectBlockHandle> _randomSelectSkillBlockHandleList = new List<SkillSelectBlockHandle>();
        private int _selectCount = 0; // 횟수가 남아있다면 스킬을 선택후 다시 스킬 선택창 리롤

        [Header("All Select View")] 
        [SerializeField] private ScrollRect allSelectScroll;
        [SerializeField] private Toggle allViewToggle;
        [SerializeField] private ToggleGroup allSelectToggleGroup;
        [SerializeField] private GameObject allSelectUIBlockObject;
        [SerializeField] private RectTransform allSelectSkillExplainPanelRectTransform; // 스킬 설명 패널
        [SerializeField] private TMP_Text allSelectSkillExplainText;

        public int AllSelectCount { get; set; }
        
        private void Awake()
        {
            selectUIBlockObject.SetActive(false);
            allSelectUIBlockObject.SetActive(false);
            allSelectSkillExplainPanelRectTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                if (randomSelectScroll.gameObject.activeSelf)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) && _randomSelectSkillBlockHandleList.Count > 0)
                    {
                        _randomSelectSkillBlockHandleList[0].button.onClick?.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2) && _randomSelectSkillBlockHandleList.Count > 1)
                    {
                        _randomSelectSkillBlockHandleList[1].button.onClick?.Invoke();
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3) && _randomSelectSkillBlockHandleList.Count > 2)
                    {
                        _randomSelectSkillBlockHandleList[2].button.onClick?.Invoke();
                    }
                }
            }
        }

        #region Member Funtion

        public int GetSelectCount() => _selectCount;
        public void AddSelectCount() => ++_selectCount;
        public void RemoveSelectCount() => --_selectCount;
        
        private void SetAllScrollActive(bool value)
        {
            randomSelectScroll.gameObject.SetActive(value);
            allSelectScroll.gameObject.SetActive(value);
        }
        
        // Select UI를 초반 셋팅 해주는 함수
        public void SpawnRandomSkillBlocks(int count)
        {
            gameObject.SetActive(true);
            SetAllScrollActive(false);
            randomSelectScroll.gameObject.SetActive(true);
            randomViewToggle.isOn = true;
            UIManager.AddActiveUI(gameObject);

            // 이미 있는 스킬들 삭제
            foreach (var handle in _randomSelectSkillBlockHandleList)
                Destroy(handle.gameObject);
            _randomSelectSkillBlockHandleList.Clear();

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
                handle.button.onClick.AddListener(() =>
                {
                    if (hasSkill == null) SpawnSkillRPC(skill.id);
                    else skill.LevelUpRPC(); 

                    RemoveSelectCount();
                    UIManager.Dequeue();
                    if (_selectCount > 0) SpawnRandomSkillBlocks(count);
                    else
                    {
                        foreach (var h in _randomSelectSkillBlockHandleList)
                            Destroy(h.gameObject); 
                        _randomSelectSkillBlockHandleList.Clear();
                        gameObject.SetActive(false);
                    }
                });
                
                _randomSelectSkillBlockHandleList.Add(handle);
            }
        }

        // 가지고 있는 스킬 중 랜덤하게 1개를 강화
        // 가지고 있는 스킬 중에 SpawnCount만큼 선택지를 제공
        public void SpawnHasRandomSkillBlock(int spawnCount)
        {
            DebugManager.ToDo("아직 미완 이 함수를 실행한 뒤에 이미 오른 레벨 등으로 인해 선택지가 제공된 것들을 원상복귀 해주어야함");
            
            gameObject.SetActive(true);
            
            // 이미 있는 스킬들 삭제
            var handles = toggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
            foreach (var handle in handles)
                Destroy(handle.gameObject);
            
            // 가지고 있는 스킬 중 만렙이 아닌 것들만 선택
            List<SkillBase> spawnSkillIdList = new List<SkillBase>();
            foreach (var skill in playerController.skillSystem.skillList)
            {
                if (skill.level.isMax == false)
                {
                    spawnSkillIdList.Add(skill);
                }
            }
            UniqueRandom uniqueRandom = new UniqueRandom(0, spawnSkillIdList.Count);

            // 스킬 UI 생성
            for (int i = 0; i < spawnCount; i++)
            {
                if (uniqueRandom.Length == 0) break;
                
                var randomInt = uniqueRandom.RandomInt();
                SkillBase skill = spawnSkillIdList[randomInt];
                
                var obj = Instantiate(selectUIBlockObject, toggleGroup.transform);
                var handle = obj.GetComponent<SkillSelectBlockHandle>();
                obj.SetActive(true);
                handle.SettingBlock(skill);
                // handle.button.onClick.AddListener(() => SelectSkill(skill.id));
            }
        }

        #region All Select Function

        // 모든 스킬들 중 1개를 선택하여 레벨업 할 수 있게 한다.
        public void SpawnAllSkillBlock()
        {
            DebugManager.ToDo("아직 미완 이 함수를 실행한 뒤에 이미 오른 레벨 등으로 인해 선택지가 제공된 것들을 원상복귀 해주어야함");
            
            gameObject.SetActive(true);
            SetAllScrollActive(false);
            allSelectSkillExplainPanelRectTransform.gameObject.SetActive(false);
            allSelectScroll.gameObject.SetActive(true);
            allViewToggle.isOn = true;
            UIManager.AddActiveUI(gameObject);
            
            // 이미 있는 스킬들 삭제
            {
                var handles = allSelectToggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
                foreach (var handle in handles)
                    Destroy(handle.gameObject);
            }
            
            // UI 생성
            foreach (var skillBase in SkillObjectList.SkillList)
            {
                SkillBase skill = skillBase;
                if (playerController.skillSystem.TryGetSkillFromID(skill.id, out var hasSkill))
                {
                    // 스킬레벨이 최대치이면 제외
                    if(hasSkill.level.isMax) continue;
                    skill = hasSkill;
                }
                else
                {   
                    skill.SetJsonData(SkillBase.GetInfoData(skill.id));
                    skill.status = skill.GetComponent<StatusBase>();
                    skill.Awake();
                }
                
                var obj = Instantiate(allSelectUIBlockObject, allSelectToggleGroup.transform);
                var handle = obj.GetComponent<SkillSelectBlockHandle>();
                obj.SetActive(true);
                obj.AddOnPointerEnter(data =>
                {
                    ++skill.level.Current;
                    skill.ExplainUpdate();
                    --skill.level.Current;

                    allSelectSkillExplainText.text = skill.explain.WrapText(30);
                    allSelectSkillExplainPanelRectTransform.gameObject.SetActive(true);
                    
                    // TMP_Text의 GetPreferredValues를 사용하여 텍스트의 크기를 계산
                    allSelectSkillExplainPanelRectTransform.sizeDelta = new Vector2(allSelectSkillExplainText.preferredWidth + 150, allSelectSkillExplainText.preferredHeight + 150);
                    
                    Vector2 anchor = new Vector2(0.5f, 0.5f); // 기본 앵커와 피봇을 중앙으로 설정
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    var halfWidth = Screen.width / 2;
                    var halfHeight = Screen.height / 2;
                    // 왼쪽 위 사분면
                    if (data.position.x < halfWidth && data.position.y > halfHeight)
                    {
                        anchor = new Vector2(0, 1);
                        pivot = new Vector2(0, 1);
                    }
                    // 오른쪽 위 사분면
                    else if (data.position.x > halfWidth && data.position.y > halfHeight)
                    {
                        anchor = new Vector2(1, 1);
                        pivot = new Vector2(1, 1);
                    }
                    // 왼쪽 아래 사분면
                    else if (data.position.x < halfWidth && data.position.y < halfHeight)
                    {
                        anchor = new Vector2(0, 0);
                        pivot = new Vector2(0, 0);
                    }
                    // 오른쪽 아래 사분면
                    else if (data.position.x > halfWidth && data.position.y < halfHeight)
                    {
                        anchor = new Vector2(1, 0);
                        pivot = new Vector2(1, 0);
                    }

                    // UI 요소의 앵커와 피봇 설정
                    allSelectSkillExplainPanelRectTransform.anchorMin = anchor;
                    allSelectSkillExplainPanelRectTransform.anchorMax = anchor;
                    allSelectSkillExplainPanelRectTransform.pivot = pivot;
                });
                obj.AddOnPointerMove(data =>
                {
                    allSelectSkillExplainPanelRectTransform.transform.position = data.position;
                });
                obj.AddOnPointerExit(data =>
                {
                    allSelectSkillExplainPanelRectTransform.gameObject.SetActive(false);
                });
                handle.SettingBlock(skill);
                handle.button.onClick.AddListener(() =>
                {
                    if (hasSkill == null) SpawnSkillRPC(skill.id);
                    else skill.LevelUpRPC(); 

                    UIManager.Dequeue();
                    allSelectSkillExplainPanelRectTransform.gameObject.SetActive(false);
                    if (--AllSelectCount > 0) SpawnAllSkillBlock();
                    else
                    {
                        var handles = allSelectToggleGroup.GetComponentsInChildren<SkillSelectBlockHandle>().ToList();
                        foreach (var h in handles)
                            Destroy(h.gameObject); 
                        gameObject.SetActive(false);
                    }
                });
            }
        }

        #endregion
        
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

