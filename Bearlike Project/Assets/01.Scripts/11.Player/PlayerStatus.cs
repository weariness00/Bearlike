using System;
using System.Collections.Generic;
using Fusion;
using Manager;
using Status;
using UI.Status;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    /// <summary>
    /// Player의 State을 나타내는 Class
    /// </summary>
    public sealed class PlayerStatus : StatusBase
    {
        #region Member Perperty
        [HideInInspector] public PlayerController playerController;
        
        public StatusValue<int> level = new StatusValue<int>();               // 레벨
        public StatusValue<int> experience = new StatusValue<int>();                 // 경험치
        public List<int> experienceAmountList = new List<int>();  // 레벨별 경험치량
        public float immortalDurationAfterSpawn = 2f;           // 무적 시간

        public StatusValue<float> jumpPower = new StatusValue<float>();

        [Networked] public TickTimer InjuryTimer { get; set; }
        public int injuryTime = 30; // 부상 상태로 있을 수 있는 시간
        public bool isInjury; // 부상 상태인지
        public Action InjuryAction;
        public Action RecoveryFromInjuryAction;
        public StatusValue<float> recoveryFromInjuryTime = new StatusValue<float>(){Max = 12}; // 다른 플레이어를 부상에서 회복시키는데 걸리는 시간

        public Action ReviveAction; // 소생 상태에 빠졌을때 발동
        public Action RecoveryFromReviveAction;
        public bool isRevive; // 소생 상태인지
        public bool isHelpOtherPlayer; // 다른 플레이어와 상호작용중인지
        
        // 무적 시간
        [Networked] private TickTimer ImmortalTimer { get; set; }
        public bool IsImmortal => ImmortalTimer.ExpiredOrNotRunning(Runner) == false;

        public AudioSource applyDamageSound;
            
        #endregion

        #region Unity Event Function
        
        // Member Function
        // ObjectState abstract class Function
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            
            condition = (int)CrowdControl.Normality;
            property = (int)CrowdControl.Normality;
            
            for(int i = 0; i < 10; ++i)
                experienceAmountList.Add(10 * (int)math.pow(i,2));    // 임시 수치 적용
            
            level.Max = 200;
            level.Min = 1;
            level.Current = 1;

            experience.Max = 100;
            experience.Min = 0;
            experience.Current = 0;
            
            // mPlayerID 초기화 필요 ==> 입장 할때 순서대로 번호 부여 혹은 고유 아이디 존재하게 구현
            // mPlayerJob 초기화 필요 ==> 직업 선택한후에 초기화 해주게 구현
        }

        private void Start()
        {
        }
        
        public override void Spawned()
        {
            base.Spawned();
            ImmortalTimer = TickTimer.CreateFromSeconds(Runner, immortalDurationAfterSpawn);

            RecoveryFromInjuryAction += () =>
            {
                hp.Current = hp.Max / 3;
                recoveryFromInjuryTime.Current = 0;
                isInjury = false;
            };

            RecoveryFromReviveAction += () =>
            {
                hp.Current = hp.Max;
                isRevive = false;
            };
        }

        public override void Render()
        {
            // immortalityIndicator.SetActive(IsImmortal);
        }
        
        #endregion

        // Loop
        public override void MainLoop()
        {
            if (ConditionPoisonedIsOn())
            {
                BePoisoned(Define.PoisonDamage);
                ShowInfo();
            }
        }
        // Loop
        
        // HP
        // 스킬, 무기, 캐릭터 스텟을 모두 고려한 함수 구현 필요
        public void BePoisoned(int value)
        {
            hp.Current -= value;
            
        }
        
        public override void ApplyDamage(int applyDamage, NetworkId ownerId, CrowdControl cc) // MonsterRef instigator,
        {
            base.ApplyDamage(applyDamage, ownerId, cc);

            // _playerCameraController.ScreenHitImpact(1,1);
            // URPRendererFeaturesManager.Instance.StartEffect("HitEffect");
            
            HpControlRPC();
        }
        
        public override void PlayerApplyDamage(int damage, NetworkId id, CrowdControl enemyProperty = CrowdControl.Normality, RpcInfo info = default)
        {
            base.PlayerApplyDamage(damage, id, enemyProperty, info);
            
        }

        /// <summary>
        /// 체력이 0이면 부상
        /// 부상에서 일정 시간이 지나면 죽음으로 바뀌게 하는 로직
        /// </summary>
        void HpControl()
        {
            // 부상에서 죽어 부활만 가능한 상태일떄 로직
            if (isRevive)
            {
                
            }
            // 부상 상태 로직
            else if (isInjury)
            {
                // 부활 상태 전환
                if (InjuryTimer.Expired(Runner))
                {
                    isInjury = false;
                    isRevive = true;
                    ReviveAction?.Invoke();
                }
            }
            // 부상 상태로 전환
            else if (IsDie)
            {
                isInjury = true;
                InjuryTimer = TickTimer.CreateFromTicks(Runner, injuryTime);// 이건 부상 상태를 유지하는 시간
                
                InjuryAction?.Invoke(); // 부상상태에 되면 발동한는 함수
            }
        }
        
        // LV
        public void IncreaseExp(int value)
        {
            experience.Current += value;
            
            while (experience.Max <= experience.Current)
            {
                if (level.isMax)
                {
                    DebugManager.Log("최대 레벨 도달");
                    return;
                }
                
                experience.Current -= experience.Max;
                experience.Max = (int)(1.5f * experience.Max);
                LevelUpRPC();
            }
        }

        public void LevelUp()
        {
            level.Current++;
            if(HasInputAuthority)
                playerController.skillSelectUI.SpawnSkillBlocks(3);
        }

        public override void HealingText(int realHealAmount)
        {
            base.HealingText(realHealAmount);

            var randomDir = Random.insideUnitSphere;
            var dir = transform.forward;
            dir.x += Mathf.Abs(randomDir.x) * (dir.x >= 0 ? 1 : -1);
            dir.y += Mathf.Abs(randomDir.y) * (dir.y >= 0 ? 1 : -1);
            dir.z += Mathf.Abs(randomDir.z) * (dir.z >= 0 ? 1 : -1);
            dir = dir.normalized * 2f;
            DamageTextCanvas.SpawnDamageText(transform.position + dir, realHealAmount, DamageTextType.Heal);
        }

        // DeBug Function
        public override void ShowInfo()
        {
        }

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void LevelUpRPC() => LevelUp();
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetRecoveryInjuryTimeRPC(float time) => recoveryFromInjuryTime.Current = time;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetIsInjuryRPC(NetworkBool value) => isInjury = value;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RecoveryFromInjuryActionRPC() => RecoveryFromInjuryAction?.Invoke();
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RecoveryFromReviveActionRPC() => RecoveryFromReviveAction?.Invoke();

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetHelpOtherPlayerRPC(NetworkBool value) => isHelpOtherPlayer = value;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void HpControlRPC() => HpControl();

        // GM Mode 용 함수
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void GoReviveRPC() => InjuryTimer = TickTimer.CreateFromTicks(Runner, 0);
        
        #endregion
    }
}