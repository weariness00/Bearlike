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

        private int _type;

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
        }
        
        public override void MainLoop()
        {
            _currentPlayTime = _gm.PlayTimer;
            
            _deltaPlayTime = _currentPlayTime - _previousPlayTime;
            
            CoolTime.Current -= _deltaPlayTime;
            Duration.Current -= _deltaPlayTime;

            // 한 번만 remove하게 해줘야 한다.
            if (Mathf.Round((Duration.Current - Duration.Min) * 10) * 0.1f <= 0f)
            {
                var playerState = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
                RemoveBuffRPC(playerState);
            }
            
            _previousPlayTime = _currentPlayTime;
        }
        
        public override void Run()
        {
            // 둘중 하나 채택
            // if(Math.Abs(CoolTime.Current - CoolTime.Min) < 1E-6)
            if (Mathf.Round((CoolTime.Current - CoolTime.Min) * 10) * 0.1f <= 0f)
            {
                var playerState = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
                _type = Random.Range(0, 2);
                ApplyBuffRPC(playerState);
            }
        }

        // HACK : 굳이 RPC로 Status을 수정 해야하나? => 어차피 주인 client만 스텟 수정을 하면 상관 없지 않나? 고려 해보자
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ApplyBuffRPC(PlayerStatus playerStatus)
        {
            if (_type == 0)
            {
                playerStatus.attackSpeed.Current *= 1.5f;
            }
            else
            {
                playerStatus.attack.Current = (int)(playerStatus.attack.Current * 1.2f);
            }

            Duration.Current = Duration.Max;
            CoolTime.Current = CoolTime.Max;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RemoveBuffRPC(PlayerStatus playerStatus)
        {
            if (_type == 0)
            {
                playerStatus.attackSpeed.Current /= 1.5f;
            }
            else
            {
                playerStatus.attack.Current = (int)(playerStatus.attack.Current / 1.2f);
            }

            Duration.Current = Duration.Min;
        }
    }
}