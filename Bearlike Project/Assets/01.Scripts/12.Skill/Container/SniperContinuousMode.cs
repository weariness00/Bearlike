using Fusion;
using GamePlay;
using Manager;
using Player;
using Status;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    /// <summary>
    /// 저격 소총 반자동 모드 : 저격 소총의 연사속도가 비약적으로 상승하는 스킬
    ///                      지속시간 : 7초, 재사용 대기시간 : 50초
    /// </summary>
    public class SniperContinuousMode : SkillBase
    {
        #region property

        // TODO : 스킬 자체의 gun status로 작동되도록 변경해야함
        public PlayerStatus playerStatus;
        
        private float _durationTime;
        
        [Networked] private TickTimer DurationTimeTimer { get; set; }
        
        private int _type;              // 동전 앞뒷면
        private float _difference;      // 차이 값 
        
        #endregion

        private void Start()
        {
            base.Start();
            var statusData = GetStatusData(id);
            _durationTime = statusData.GetFloat("Duration Time");
            Debug.Log($"SniperContinuousMode : {_durationTime}");
        }

        public override void Earn(GameObject earnTargetObject)
        {
            // 얻는 대상을 총으로 해야하나
        }

        public override void MainLoop()
        {
            // if (coolTime.isMin == false)
            // {
            //     _currentPlayTime = _gm.PlayTimer;
            //
            //     _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            //
            //     coolTime.Current -= _deltaPlayTime;
            //     duration.Current -= _deltaPlayTime;
            // }
            //
            // if (_bOn && Mathf.Round((duration.Current - duration.Min) * 10) * 0.1f <= 0f)
            // {
            //     _sniper.bulletFirePerMinute += _difference;
            //     _sniper.fireLateSecond = 60 / _sniper.bulletFirePerMinute;
            //     
            //     duration.Current = duration.Min;
            //     _bOn = false;
            //     // gameObject.SetActive(false);
            // }
            //
            // if (coolTime.isMin == false)
            // {
            //     _previousPlayTime = _currentPlayTime;
            // }
            
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                isInvoke = false;
                CoolTimeTimer = TickTimer.CreateFromSeconds(Runner, coolTime);
                
                playerStatus.attackSpeed.Current -= (int)_difference;
            }
        }
    
        public override void Run(GameObject runObject)
        {
            // if (_bOn == false && Mathf.Round((coolTime.Current - coolTime.Min) * 10) * 0.1f <= 0f)
            // {
            //     // gameObject.SetActive(true);
            //     _difference = 2 * (_sniper.bulletFirePerMinute / 3);
            //     _sniper.bulletFirePerMinute += _difference;
            //     _sniper.fireLateSecond = 60 / _sniper.bulletFirePerMinute;
            //     
            //     duration.Current = duration.Max;
            //     coolTime.Current = coolTime.Max;
            //
            //     _bOn = true;
            // }
            
            if (CoolTimeTimer.Expired(Runner) && false == isInvoke)
            {
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)
                
                playerStatus = runObject.GetComponent<PlayerStatus>();
                
                _difference = playerStatus.attackSpeed.Current * 3.0f;
                playerStatus.attackSpeed.Current += (int)_difference;
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }

    }
}
