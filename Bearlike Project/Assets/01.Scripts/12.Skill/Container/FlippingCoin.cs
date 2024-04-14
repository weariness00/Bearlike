using GamePlay;
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
        #region time

        // private GameManager _gm;
        private float _currentPlayTime;
        private float _previousPlayTime;
        private float _deltaPlayTime;

        #endregion

        #region property

        public PlayerStatus playerStatus;
        
        private int _type;              // 동전 앞뒷면
        private bool _bOn;              // 현재 발동 중인지 판단하는 bool
        private float _difference;      // 차이 값 

        #endregion

        #region Value

        public float duration;

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
            
            // _gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            // playerStatus = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            // playerStatus = status;
            
            _bOn = false;
            _difference = 0;
        }

        public override void Start()
        {
            base.Start();
            var statusData = GetStatusData(id);
            duration = statusData.GetFloat("Duration");
        }
        
        public override void Earn(GameObject earnTargetObject)
        {
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
            _previousPlayTime = _currentPlayTime;
        }
        
        public override void Run(GameObject runObject)
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
        }

        public override void LevelUp()
        {
            
        }
    }
}