using System.Collections.Generic;
using Data;
using Fusion;
using Photon;
using Player;
using Status;
using UI.Inventory;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public enum SKillType
    {
        Active,
        Passive
    }
    
    /// <summary>
    /// 스킬은 한번 습득하면 절대 버리지 못한다.
    /// 레벨업 & 다운은 가능하다 버리기는 불가능
    /// </summary>
    [System.Serializable]
    [RequireComponent(typeof(StatusBase))]
    public abstract class SkillBase : NetworkBehaviourEx, IJsonData<SkillJsonData>, IInventoryItemAdd
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
        
        [HideInInspector] public PlayerController ownerPlayer;

        [Header("Skill 기본 정보")]
        public int id;
        public string skillName;
        private string _originExplain; // 원본 설명
        public string explain; // 텍스트로 보여줄 설명
        public SKillType type;
        public Texture2D icon;
        public float coolTime;

        public bool isInvoke; // 현재 스킬이 발동 중인지

        public StatusBase status;
        public StatusValue<int> level = new StatusValue<int>() { Max = 1 };
        [Networked] public TickTimer CoolTimeTimer { get; set; }

        #endregion

        #region Unity Event Function

        public virtual void Awake()
        {
            status = GetComponent<StatusBase>();
            
            SetJsonData(GetInfoData(id));
            var statusData = GetStatusData(id);
            status.SetJsonData(statusData);
            if(statusData.HasInt("Level Max")) level.Max = statusData.GetInt("Level Max");
        }

        public virtual void Start()
        {
        }

        public override void Spawned()
        {
            CoolTimeTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        #endregion

        #region Member Function

        /// <summary>
        /// 스킬을 습득 했을때 발동하도록 하는 함수
        /// </summary>
        /// <param name="earnTargetObject">해당 스킬을 습득한 대상</param>
        public abstract void Earn(GameObject earnTargetObject);
        public abstract void MainLoop();
        public abstract void Run(GameObject runObject);

        /// <summary>
        /// 레벨업 (스킬 강화) 할시 동작하는 함수
        /// </summary>
        public virtual void LevelUp()
        {
            level.Current += 1;
            var pc = ownerPlayer.GetComponent<PlayerController>();
            pc.skillInventory.AddItem(this);
            explain = _originExplain;
        }

        public virtual void ExplainUpdate()
        {
            explain = _originExplain;
        }
        #endregion
        
        #region Inventory Interface

        public AddItem AddItem<AddItem>(AddItem addItem)
        {
            if (addItem is SkillBase skillBase)
            {
                level.Current = skillBase.level.Current;
            }

            return addItem;
        }

        #endregion

        #region Json Datra Interface

        public SkillJsonData GetJsonData()
        {
            return new SkillJsonData();
        }
        public void SetJsonData(SkillJsonData json)
        {
            skillName = json.Name;
            _originExplain = json.Explain;
            coolTime = json.CoolTime;
            type = json.Type;
        }
        
        #endregion
        
        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetOwnerIdRPC(NetworkId owner)
        {
            var obj = Runner.FindObject(owner);
            var pc = obj.GetComponent<PlayerController>();
            ownerPlayer = pc;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsInvokeRPC(NetworkBool value) => isInvoke = value;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetLevelRPC(StatusValueType type, int value)
        {
            switch (type)
            {
                case StatusValueType.Current:
                    level.Current = value;
                    break;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void LevelUpRPC()
        {
            level.Current += 1;
            LevelUp();
        }

        #endregion
    }
}