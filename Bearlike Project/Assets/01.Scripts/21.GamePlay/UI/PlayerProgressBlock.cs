using Data;
using Fusion;
using Manager;
using Photon;
using Player;
using TMPro;
using UnityEngine;

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
        
        private ChangeDetector _changeDetector;

        #region Unity Event Function

        public override void Spawned()
        {
            base.Spawned();

            {
                var obj = Runner.FindObject(PlayerId);
                if (obj) obj.TryGetComponent(out _playerController);
            }

            if (_playerController == null)
            {
                DebugManager.LogError("Player가 없어 게임 진행 현황 UI를 제작할 수 없습니다.");
                Destroy(gameObject);
                return;
            }

            {
                var obj = Runner.FindObject(ParentId);
                if (obj) transform.parent = obj.transform;
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            nameText.text = UserData.Instance.UserDictionary.Get(_playerController.PlayerRef).Name.ToString();
            levelText.text = _playerController.status.level.Current.ToString();
            killCountText.text = KillCount.ToString();
            damageCountText.text = DamageCount.ToString();

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
    }
}

