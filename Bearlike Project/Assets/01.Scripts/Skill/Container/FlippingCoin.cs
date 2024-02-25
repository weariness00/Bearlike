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

        public PlayerStatus playerStatus;
        
        private int _type;              // 동전 앞뒷면
        private bool _bOn;              // 현재 발동 중인지 판단하는 bool

        private float _difference;      // 차이 값 

        #endregion
        
        public FlippingCoin(PlayerStatus status)
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

            // playerStatus = GameObject.Find("Local Player").GetComponent<PlayerStatus>();
            playerStatus = status;
            
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
                RemoveBuffRPC(playerStatus, _type);
                
                Duration.Current = Duration.Min;
                _bOn = false;
                // _difference = 0; // 굳이 필요 없을듯
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
                ApplyBuffRPC(playerStatus, _type);
                
                Duration.Current = Duration.Max;
                CoolTime.Current = CoolTime.Max;

                _bOn = true;
            }
            else
            {
                Debug.Log($"남은 쿨타임 : {CoolTime.Current}");
            }
        }

        // 다른 클라이언트에는 적용x ==> status를 불러올때 이름을 어떻게 수정해야 할까 ==> class를 인자로 넣으면 안된다.
        // 서버 클라이언트는 다른 클라이언트 개수만큼 중복 실행됨 ==> 서버 클라이언트는 모든 doll의 stateauthority가 있기 때문 ==> 조작을 json data로 받아오면 완성
        
        // HACK : 굳이 RPC로 Status을 수정 해야하나? => 어차피 주인 client만 스텟 수정을 하면 상관 없지 않나? 고려 해보자
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void ApplyBuffRPC(PlayerStatus playerStatus, int type, RpcInfo info = default)
        {
            if (type == 0)
            {
                _difference = playerStatus.attackSpeed.Current * 0.5f;
                playerStatus.attackSpeed.Current += _difference;
            }
            else
            {
                _difference = playerStatus.attack.Current * 0.2f;
                playerStatus.attack.Current += (int)_difference;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RemoveBuffRPC(PlayerStatus playerStatus, int type, RpcInfo info = default)
        {
            if (type == 0)
            {
                playerStatus.attackSpeed.Current -= _difference;
            }
            else
            {
                playerStatus.attack.Current -= (int)_difference;
            }
        }
    }
}