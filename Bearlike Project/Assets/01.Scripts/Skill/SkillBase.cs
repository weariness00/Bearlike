using System.Collections.Generic;
using Fusion;
using Newtonsoft.Json;
using Script.Data;
using Status;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public abstract class SkillBase : IJsonData<SkillJsonData>
    {
        public string name;
        public string explain;
        public Texture2D image;

        public bool isInvoke; // 현재 스킬이 발동 중인지
        private StatusValue<float> _duration = new StatusValue<float>();
        private StatusValue<float> _coolTime = new StatusValue<float>();

        public StatusValue<int> damage = new StatusValue<int>(){Max = 99999};
        
        [Networked]
        public StatusValue<float> Duration
        {
            get => _duration;
            set => _duration = value;
        }

        [Networked]
        public StatusValue<float> CoolTime
        {
            get => _coolTime;
            set => _coolTime = value;
        }

        public abstract void MainLoop();
        public abstract void Run(GameObject runObject);

        public SkillJsonData GetJsonData()
        {
            return null;
        }

        public void SetJsonData(SkillJsonData json)
        {
            explain = json.explain;
            CoolTime.Current = json.coolTime;

            damage.Current = json.GetStatusInt("Damage");
        }
    }
}