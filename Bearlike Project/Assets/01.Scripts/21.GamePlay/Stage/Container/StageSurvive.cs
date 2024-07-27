using Fusion;
using GamePlay.Stage;
using Status;
using UnityEngine;

namespace GamePlay.StageLevel.Container
{
    public class StageSurvive : StageBase
    {
        [Header("생존 스테이지 정보")] 
        public int surviveTime = 1;
        [Networked] private TickTimer LimitTimer { get; set; }

        [HideInInspector] 
        public float currentTime = 0f;
        private float _startTime;

        public override void StageStart()
        {
            base.StageStart();
            LimitTimer = TickTimer.CreateFromSeconds(Runner, surviveTime);
            currentTime = 0f;
            _startTime = GameManager.Instance.PlayTimer;
        }

        public override void StageUpdate()
        {
            base.StageUpdate();
            if (LimitTimer.Expired(Runner))
            {
                StageClear();
            }
            else
            {
                currentTime = GameManager.Instance.PlayTimer - _startTime;
            }
        }
        
        public override void StageOver()
        {
            if (isStageOver)
            {
                return;
            }
            base.StageOver();
        }
    }
}

