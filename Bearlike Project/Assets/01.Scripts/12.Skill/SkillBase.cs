using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
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

        protected static string pattern = @"\[(.*?)\]";  // 대괄호 안의 문자열을 찾기 위한 정규 표현식
        
        #endregion

        #region Member Variable
        
        [HideInInspector] public PlayerController ownerPlayer;

        [Header("Skill 기본 정보")]
        public int id;
        public string skillName;
        private string _originExplain; // 원본 설명
        public string explain; // 텍스트로 보여줄 설명
        public SKillType type;
        public Sprite icon;
        public float coolTime;

        public bool isInvoke; // 현재 스킬이 발동 중인지

        public StatusBase status;
        public StatusValue<int> level = new StatusValue<int>() { Max = 1 };
        private TickTimer CoolTimeTimer { get; set; }
        // 쿨타임이 끝나서 사용할 수 있는 상태인지
        public bool IsUse => CoolTimeTimer.Expired(Runner);

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
        public virtual void Earn(GameObject earnTargetObject)
        {
            ownerPlayer = earnTargetObject.GetComponent<PlayerController>();
        }
        public abstract void MainLoop();
        public abstract void Run();

        /// <summary>
        /// 레벨업 (스킬 강화) 할시 동작하는 함수
        /// </summary>
        public virtual void LevelUp()
        {
            level.Current += 1;
            ownerPlayer.skillInventory.AddItem(this);
            explain = _originExplain;
        }

        public virtual void ExplainUpdate()
        {
            explain = _originExplain;
        }
        
        protected string ComputeAndReplace(Match m)
        {
            string expression = m.Groups[1].Value;
            try
            {
                var table = new DataTable();
                var result = table.Compute(expression, null);
                return result.ToString();  // 계산 결과를 문자열로 변환하여 반환합니다.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating expression '{expression}': {ex.Message}");
                return expression;  // 오류 발생 시 원래의 수식을 반환합니다.
            }
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
            explain = _originExplain;
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

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void SetSkillCoolTimerRPC(float time) => CoolTimeTimer = TickTimer.CreateFromSeconds(Runner, time);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void SetIsInvokeRPC(NetworkBool value) => isInvoke = value;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RunRPC() => Run();
        
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
            LevelUp();
        }

        #endregion
    }
}