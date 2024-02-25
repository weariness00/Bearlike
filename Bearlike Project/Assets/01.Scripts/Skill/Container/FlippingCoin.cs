using System;
using Fusion;
using Manager;
using Scripts.State.GameStatus;
using State.StateClass;
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

        private GameManager _gm;
        private float _currentPlayTime;
        private float _previousPlayTime;

        private float _deltaPlayTime;

        #endregion

        #region property

        private PlayerStatus _playerStatus;
        
        private int _type;              // 동전 앞뒷면
        private bool _bOn;              // 현재 발동 중인지 판단하는 bool

        private float _difference;      // 차이 값 

        #endregion
        
        public FlippingCoin()
        {
            var tempCoolTime = new StatusValue<float>();
            tempCoolTime.Max = 30.0f;
            tempCoolTime.Min = tempCoolTime.Current = 0.0f;

            CoolTime = tempCoolTime;

            var tempDuration = new StatusValue<float>();
            tempDuration.Max = 10.0f;
            tempDuration.Min = tempDuration.Current = 0.0f;

            Duration = tempDuration;
            
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            _playerStatus = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            
            _bOn = false;
            _difference = 0;
        }
        
        public override void MainLoop()
        {
            _currentPlayTime = _gm.PlayTimer;
            
            _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            
            CoolTime.Current -= _deltaPlayTime;
            Duration.Current -= _deltaPlayTime;

            // 한 번만 remove하게 해줘야 한다.
            if (_bOn && Mathf.Round((Duration.Current - Duration.Min) * 10) * 0.1f <= 0f)
            {
                RemoveBuffRPC(_playerStatus);
            }
            
            _previousPlayTime = _currentPlayTime;
        }
        
        public override void Run()
        {
            // 둘중 하나 채택
            // if(Math.Abs(CoolTime.Current - CoolTime.Min) < 1E-6)
            if (_bOn == false && Mathf.Round((CoolTime.Current - CoolTime.Min) * 10) * 0.1f <= 0f)
            {
                _type = Random.Range(0, 2);
                ApplyBuffRPC(_playerStatus);
            }
        }

        // HACK : 굳이 RPC로 Status을 수정 해야하나? => 어차피 주인 client만 스텟 수정을 하면 상관 없지 않나? 고려 해보자
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ApplyBuffRPC(PlayerStatus playerStatus)
        {
            if (_type == 0)
            {
                _difference = playerStatus.attackSpeed.Current * 0.5f;
                playerStatus.attackSpeed.Current += _difference;
            }
            else
            {
                _difference = playerStatus.attack.Current * 0.2f;
                playerStatus.attack.Current += (int)_difference;
            }

            Duration.Current = Duration.Max;
            CoolTime.Current = CoolTime.Max;

            _bOn = true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RemoveBuffRPC(PlayerStatus playerStatus)
        {
            if (_type == 0)
            {
                playerStatus.attackSpeed.Current -= _difference;
            }
            else
            {
                playerStatus.attack.Current -= (int)_difference;
            }

            Duration.Current = Duration.Min;
            _bOn = false;
            // _difference = 0; // 굳이 필요 없을듯
        }
    }
}