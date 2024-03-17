using Script.Data;
using Status;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public enum SKillType
    {
        Active,
        Passive
    }
    
    [System.Serializable]
    public abstract class SkillBase : MonoBehaviour, IJsonData<SkillJsonData>
    {
        public string skillName;
        public string explain;
        public Texture2D icon;

        public SKillType type;
        public bool isInvoke; // 현재 스킬이 발동 중인지
        public StatusValue<float> coolTime = new StatusValue<float>();

        public StatusValue<int> damage = new StatusValue<int>(){Max = 99999};
        public StatusValue<float> duration = new StatusValue<float>();

        public abstract void MainLoop();
        public abstract void Run(GameObject runObject);

        public SkillJsonData GetJsonData()
        {
            return null;
        }

        public void SetJsonData(SkillJsonData json)
        {
            explain = json.explain;
            coolTime.Current = json.coolTime;

            damage.Current = json.GetStatusInt("Damage");
        }
    }
}