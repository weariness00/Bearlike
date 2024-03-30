using System;
using System.Collections.Generic;
using Data;
using Fusion;
using Photon;
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
    public abstract class SkillBase : NetworkBehaviourEx, IJsonData<SkillJsonData>, IJsonData<StatusJsonData>
    {
        #region Static

        // Info Data 캐싱
        private static Dictionary<int, SkillJsonData> _infoDataCash = new Dictionary<int, SkillJsonData>();
        public static void AddInfoData(int id, SkillJsonData data) => _infoDataCash.TryAdd(id, data);
        public static SkillJsonData GetInfoData(int id) => _infoDataCash.TryGetValue(id, out var data) ? data : new SkillJsonData();
        public static void ClearInfosData() => _infoDataCash.Clear();
        
        // Status Data 캐싱
        private static Dictionary<int, StatusJsonData> _statusDataChasing = new Dictionary<int, StatusJsonData>();
        public static void AddStatusData(int id, StatusJsonData data) => _statusDataChasing.TryAdd(id, data);
        public static StatusJsonData GetStatusData(int id) => _statusDataChasing.TryGetValue(id, out var data) ? data : new StatusJsonData();
        public static void ClearStatusData() => _statusDataChasing.Clear();

        #endregion

        #region Member Variable

        [Header("Skill 기본 정보")]
        public int id;
        public string skillName;
        public string explain;
        public SKillType type;
        public StatusValue<float> coolTime = new StatusValue<float>();

        public Texture2D icon;
        public bool isInvoke; // 현재 스킬이 발동 중인지
        
        public StatusValue<int> damage = new StatusValue<int>(){Max = 99999};
        public StatusValue<float> duration = new StatusValue<float>();

        #endregion

        #region Unity Event Function

        public virtual void Start()
        {
            SetJsonData(GetInfoData(id));
            SetJsonData(GetStatusData(id));
        }

        #endregion
        
        public abstract void MainLoop();
        public abstract void Run(GameObject runObject);

        public SkillJsonData GetJsonData()
        {
            
            return new SkillJsonData();
        }
        public void SetJsonData(SkillJsonData json)
        {
            explain = json.Explain;
            coolTime.Current = coolTime.Max = json.CoolTime;
            type = json.Type;
        }
                
        StatusJsonData IJsonData<StatusJsonData>.GetJsonData()
        {
            return new StatusJsonData();
        }
        public void SetJsonData(StatusJsonData json)
        {
            damage.Max = json.GetInt("Damage Max");
            damage.Current = json.GetInt("Damage Current");
        }
        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsInvokeRPC(NetworkBool value) => isInvoke = value;

        #endregion
    }
}