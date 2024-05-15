using System.Collections;
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

        public PlayerStatus playerStatus;
        public GameObject effectObject;
        public GameObject effectTableObject;

        public FlippingCoinEffect effect;
        
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
            
            effectObject.SetActive(false);
            effectTableObject.SetActive(false);
            effect = effectObject.GetComponent<FlippingCoinEffect>();
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
                DebugManager.ToDo("FlippingCoin은 player status에 더할 필요가 없다.");
                pc.status.AddAdditionalStatus(status);
                playerStatus = pc.status;
            }
        }

        public override void MainLoop()
        {
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                isInvoke = false;
                // SetSkillCoolTimerRPC(coolTime);
                SetSkillCoolTimerRPC(1);

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
            if (IsUse && false == isInvoke)
            {
                StartCoroutine(StartEffect());
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)
                
                _type = Random.Range(0, 2);
                
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
                
                // DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, 1);
            }
        }

        IEnumerator StartEffect()
        {
            effectObject.SetActive(true);
            effectTableObject.SetActive(true);
            effect.FlickCoin();
            yield return new WaitForSeconds(2.0f);
            effectObject.SetActive(false);
            effectTableObject.SetActive(false);
        }
    }
}