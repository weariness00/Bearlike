using Fusion;
using GamePlay;
using Manager;
using Player;
using Status;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Skill.Container
{
    /// <summary>
    /// 동전 던지기(첫째곰) : 앞 면이 나올시에는 공속이 1.5배 상승, 뒷 면이 나올시에는 데미지 1.2배 상승
    ///                    지속 시간은 10초 정도로 설정 AND 쿨타임은 30초로 설정
    /// </summary>
    public sealed class FlippingCoin : SkillBase
    {
        #region property

        // TODO : 스킬 자체의 status로 작동되도록 변경해야함
        public PlayerStatus playerStatus;
        
        private float _durationTime;
        
        private TickTimer DurationTimeTimer { get; set; }
        
        private int _type;              // 동전 앞뒷면
        private float _difference;      // 차이 값 
        
        #endregion

        public override void Start()
        {
            base.Start();
            var statusData = GetStatusData(id);
            _durationTime = statusData.GetFloat("Duration Time");
            Debug.Log($"FlippingCoin : {_durationTime}");
        }

        public override void Spawned()
        {
            base.Spawned();
            
            DurationTimeTimer = TickTimer.CreateFromTicks(Runner, 0);
        }
        
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                pc.status.AddAdditionalStatus(status);
            }
        }

        public override void MainLoop()
        {
            // _currentPlayTime = _gm.PlayTimer;
            //
            // _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            //
            // coolTime -= _deltaPlayTime;
            // duration -= _deltaPlayTime;
            //
            // if (_bOn && Mathf.Round((duration.Current - duration.Min) * 10) * 0.1f <= 0f)
            // {
            //     if (_type == 0)
            //     {
            //         playerStatus.SetAttackSpeedRPC(StatusValueType.Current, playerStatus.attackSpeed.Current - _difference);
            //     }
            //     else
            //     {
            //         playerStatus.damage.Current -= (int)_difference;
            //     }
            //     
            //     Debug.Log($"현재 Attack : {playerStatus.damage.Current}, AttackSpeed : {playerStatus.attackSpeed.Current}");
            //     
            //     duration.Current = duration.Min;
            //     _bOn = false;
            // }
            //
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                isInvoke = false;
                SetSkillCoolTimerRPC(coolTime);
                if (_type == 0)
                {
                    playerStatus.attackSpeed.Current -= (int)_difference;
                }
                else
                {
                    playerStatus.damage.Current -= (int)_difference;
                }
            }
        }
        
        public override void Run()
        {
            // 둘중 하나 채택
            // if(Math.Abs(CoolTime.Current - CoolTime.Min) < 1E-6)
            // if (_bOn == false && Mathf.Round((coolTime.Current - coolTime.Min) * 10) * 0.1f <= 0f)
            // {
            //     _type = Random.Range(0, 2);
            //
            //     playerStatus = runObject.GetComponent<PlayerStatus>();
            //     
            //     if (_type == 0)
            //     {
            //         _difference = playerStatus.attackSpeed.Current * AttackValue;
            //         playerStatus.attackSpeed.Current += _difference;
            //     }
            //     else
            //     {
            //         _difference = playerStatus.damage.Current * AttackSpeedValue;
            //         playerStatus.damage.Current += (int)_difference;
            //     }
            //     
            //     duration.Current = duration.Max;
            //     coolTime.Current = coolTime.Max;
            //
            //     _bOn = true;
            //     
            //     Debug.Log($"현재 Attack : {playerStatus.damage.Current}, AttackSpeed : {playerStatus.attackSpeed.Current}");
            // }
            // else
            // {
            //     Debug.Log($"남은 쿨타임 : {coolTime.Current}");
            // }

            if (IsUse && false == isInvoke)
            {
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)
                
                _type = Random.Range(0, 2);

                playerStatus = ownerPlayer.status;
                
                if (_type == 0)
                {
                    _difference = playerStatus.attackSpeed.Current * 1.5f;
                    playerStatus.attackSpeed.Current += (int)_difference;
                }
                else
                {
                    _difference = playerStatus.damage.Current * 1.2f;
                    playerStatus.damage.Current += (int)_difference;
                }
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }

    }
}