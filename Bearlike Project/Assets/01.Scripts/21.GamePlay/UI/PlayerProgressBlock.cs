using System.Collections.Generic;
using Data;
using Fusion;
using Manager;
using Photon;
using Player;
using Skill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class PlayerProgressBlock : NetworkBehaviourEx
    {
        [Networked] public NetworkId PlayerId { get; set; }
        [Networked] public NetworkId ParentId { get; set; }
        [Networked] private int KillCount { get; set; }
        [Networked] private int DamageCount { get; set; }

        private PlayerController _playerController;

        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text killCountText;
        [SerializeField] private TMP_Text damageCountText;
        [SerializeField] private Transform skillGridTransform;
        [SerializeField] private GameObject skillBlock;
        private readonly Dictionary<int, SkillBlock> skillBlockDictionary = new Dictionary<int, SkillBlock>();
        
        private ChangeDetector _changeDetector;

        #region Unity Event Function

        public override void Spawned()
        {
            base.Spawned();

            { // 플레이어가 정상적으로 존재하는지 확인
                var obj = Runner.FindObject(PlayerId);
                if (obj) obj.TryGetComponent(out _playerController);
            }

            if (_playerController == null)
            {
                DebugManager.LogError("Player가 없어 게임 진행 현황 UI를 제작할 수 없습니다.");
                Destroy(gameObject);
                return;
            }

            { // UI가 네트워크 객체로 생성됨 부모 설정을 해주어야한다.
                var obj = Runner.FindObject(ParentId);
                if (obj) transform.parent = obj.transform;
            }

            {
                EventBusManager.Subscribe<SkillBase>(EventBusType.AddSkill, AddSkillBlock);
                EventBusManager.Subscribe<SkillBase>(EventBusType.SkillLevelUp, SkillLevelUp);
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            nameText.text = UserData.Instance.UserDictionary.Get(_playerController.PlayerRef).Name.ToString();
            levelText.text = _playerController.status.level.Current.ToString();
            killCountText.text = KillCount.ToString();
            damageCountText.text = DamageCount.ToString();
            
            foreach (var skill in _playerController.skillSystem.skillList) AddSkillBlock(skill);
            _playerController.MonsterKillAction += (obj) => SetKillCountRPC(KillCount + 1);
            _playerController.AfterApplyDamageAction += (value) => SetDamageCountRPC(DamageCount + value);
            _playerController.status.LevelUpAction += () => levelText.text = _playerController.status.level.Current.ToString();
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(KillCount):
                        killCountText.text = KillCount.ToString();
                        break;
                    case nameof(DamageCount):
                        damageCountText.text = DamageCount.ToString();
                        break;
                }
            }
        }

        #endregion

        #region Member Function

        public void AddSkillBlock(SkillBase skill)
        {
            if(_playerController != skill.GetComponentInParent<PlayerController>()) return;
            
            var obj = Instantiate(skillBlock.gameObject, skillGridTransform);
            obj.SetActive(true);
            var block = new SkillBlock()
            {
                Icon = obj.GetComponentInChildren<Image>(),
                LevelText = obj.GetComponentInChildren<TMP_Text>()
            };
            block.Icon.sprite = skill.icon;
            block.LevelText.text = skill.level.Current.ToString();
            skillBlockDictionary.Add(skill.id, block);
        }

        public void SkillLevelUp(SkillBase skill)
        {
            if (skillBlockDictionary.TryGetValue(skill.id, out SkillBlock block))
            {
                block.LevelText.text = skill.level.Current.ToString();
            }
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetKillCountRPC(int value)
        {
            KillCount = value;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void SetDamageCountRPC(int value)
        {
            DamageCount = value;
        }

        #endregion

        #region Struct

        private struct SkillBlock
        {
            public Image Icon;
            public TMP_Text LevelText;
        }

        #endregion
    }
}

