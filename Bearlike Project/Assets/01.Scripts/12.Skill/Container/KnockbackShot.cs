using GamePlay;
using Player;
using Status;
using UnityEngine;
using Weapon.Gun;

namespace Skill.Container
{
    public class KnockbackShot : SkillBase
    {
        #region time

        private GameManager _gm;
        private float _currentPlayTime;
        private float _previousPlayTime;

        private float _deltaPlayTime;

        #endregion

        #region property

        public PlayerStatus playerStatus;
        public GunBase gun;
        
        private int _type;              // 동전 앞뒷면
        private bool _bOn;              // 현재 발동 중인지 판단하는 bool

        private float _difference;      // 차이 값 

        #endregion

        #region Value

        public StatusValue<float> duration = new StatusValue<float>();
        private const float AttackValue = 0.5f;
        private const float AttackSpeedValue = 0.2f;
        private const float CoolTime = 30.0f;
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
            
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            // playerStatus = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            // playerStatus = status;
            
            _bOn = false;
            _difference = 0;
        }

        public override void Earn(GameObject earnTargetObject)
        {
            
        }

        public override void MainLoop()
        {
            _currentPlayTime = _gm.PlayTimer;
            
            _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            
            // coolTime.Current -= _deltaPlayTime;
            duration.Current -= _deltaPlayTime;

            if (_bOn && Mathf.Round((duration.Current - duration.Min) * 10) * 0.1f <= 0f)
            {
                duration.Current = duration.Min;
                _bOn = false;
            }
            
            _previousPlayTime = _currentPlayTime;
        }
        
        public override void Run(GameObject runObject)
        {
            // 둘중 하나 채택
            // if(Math.Abs(CoolTime.Current - CoolTime.Min) < 1E-6)
            // if (_bOn == false && Mathf.Round((coolTime.Current - coolTime.Min) * 10) * 0.1f <= 0f)
            // {
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
        }

        public override void LevelUp()
        {
            
        }
    }
}
