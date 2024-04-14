using Fusion;
using GamePlay;
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
        [Networked] public TickTimer T { get; set; }
        #region time

        private GameManager _gm;
        private float _currentPlayTime;
        private float _previousPlayTime;

        private float _deltaPlayTime;

        #endregion

        #region property

        private GunBase _sniper;
        
        private bool _bOn;
        private float _difference;

        #endregion
        
        #region Value

        public StatusValue<float> duration = new StatusValue<float>();
        private const float CoolTime = 50.0f;
        private const float DurationTime = 7.0f;

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

            _gm = GameManager.Instance;
                
            _difference = 0;
        }

        private void Start()
        {
            _sniper = transform.root.GetComponentInChildren<GunBase>();    // or 인스펙터에서 추가하는 방식
            // gameObject.SetActive(false);
        }

        public override void Earn(GameObject earnTargetObject)
        {
            
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
        }

        public override void LevelUp()
        {
            
        }
    }
}
