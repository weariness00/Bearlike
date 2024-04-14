using Fusion;
using GamePlay;
using Player;
using Status;
using UnityEngine;

namespace Skill.Container
{
    /// <summary>
    /// 회피 시스템 작동 : 스킬을 사용하면 10초간 회피률이 1.3배 상승한다.
    ///                  지속 시간 : 10초 / 재사용 대기 시간 : 30초
    /// </summary>
    public sealed class AvoidingSystemOperation : SkillBase
    {
        #region time

        // private GameManager _gm;
        private float _currentPlayTime;
        private float _previousPlayTime;

        private float _deltaPlayTime;

        #endregion

        #region property

        public PlayerStatus playerStatus;
        
        private int _type;
        private bool _bOn;              // 현재 발동 중인지 판단하는 bool

        private float _difference;      // 차이 값 

        #endregion
        
        #region Value

        public float duration = 10.0f;
        private const float AvoidValue = 0.3f;
        private const float CoolTime = 30.0f;
        [Networked] private TickTimer DurationTimer { get; set; }
        private const float DurationTime = 10.0f;

        #endregion
        
        public override void Awake()
        {
            base.Awake();   
            var tempCoolTime = new StatusValue<float>();
            tempCoolTime.Max = CoolTime;
            tempCoolTime.Min = tempCoolTime.Current = 0.0f;

            coolTime = tempCoolTime;

            var tempDuration = new StatusValue<float>();
            tempDuration.Max = DurationTime;
            tempDuration.Min = tempDuration.Current = 0.0f;

            duration = tempDuration;
            
            // _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            
            _bOn = false;
            _difference = 0;
        }

        public override void Spawned()
        {
            base.Spawned();
            DurationTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        public override void Earn(GameObject earnTargetObject)
        {
        }

        public override void MainLoop()
        {
            // _currentPlayTime = _gm.PlayTimer;
            
            _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            
            // coolTime.Current -= _deltaPlayTime;
            // duration.Current -= _deltaPlayTime;
            //
            // if (_bOn && Mathf.Round((duration.Current - duration.Min) * 10) * 0.1f <= 0f)
            // {
            //     playerStatus.avoid.Current -= _difference;
            //     
            //     duration.Current = duration.Min;
            //     _bOn = false;
            // }
            
            _previousPlayTime = _currentPlayTime;
        }

        public override void Run(GameObject runObject)
        {
            // if (_bOn == false && Mathf.Round((coolTime.Current - coolTime.Min) * 10) * 0.1f <= 0f)
            // {
            //     playerStatus = runObject.GetComponent<PlayerStatus>();
            //
            //     _difference = playerStatus.avoid * AvoidValue;
            //     playerStatus.avoid.Current += _difference;
            //
            //     duration.Current = duration.Max;
            //     coolTime.Current = coolTime.Max;
            //
            //     _bOn = true;
            // }
        }

        public override void LevelUp()
        {
            
        }
    }
}