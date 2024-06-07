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

        // public PlayerStatus playerStatus;
        public GameObject effectObject;
        public GameObject effectTableObject;

        public FlippingCoinEffect effectRought;
        
        private float _durationTime;
        
        private TickTimer DurationTimeTimer { get; set; }
        
        private int _type;              // 동전 앞뒷면
        private float _difference;      // 차이 값 
        
        #endregion

        public override void Awake()
        {
            base.Awake();
            var statusData = GetStatusData(id);
            _durationTime = statusData.GetFloat("Duration Time");
            
            effectObject.SetActive(false);
            effectTableObject.SetActive(false);
            effectRought = effectObject.GetComponent<FlippingCoinEffect>();
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
            if (DurationTimeTimer.Expired(Runner) && true == isInvoke)
            {
                isInvoke = false;
                SetSkillCoolTimerRPC(GetCoolTime());

                if (_type == 0)
                {
                    status.attackSpeedMultiple -= 0.5f;
                }
                else
                {
                    status.damageMultiple -= 0.2f;
                }
            }
        }
        
        public override void Run()
        {
            if (IsUse && false == isInvoke)
            {
                // TODO : run이 rpc여서 모든 클라에서 실행된다.
                // StartCoroutine(StartEffect());
                StartVFXRPC();
                isInvoke = true;
                // TODO : VFX도 넣어보자(너무 티가 안남)
                
                _type = Random.Range(0, 2);
                
                if (_type == 0)
                {
                    status.attackSpeedMultiple += 0.5f;
                }
                else
                {
                    status.damageMultiple += 0.2f;
                }
                
                DurationTimeTimer = TickTimer.CreateFromSeconds(Runner, _durationTime);
            }
        }

        IEnumerator StartEffect()
        {
            effectObject.SetActive(true);
            effectTableObject.SetActive(true);
            effectRought.FlickCoin();
            yield return new WaitForSeconds(2.0f);
            effectObject.SetActive(false);
            effectTableObject.SetActive(false);
        }
    }
}