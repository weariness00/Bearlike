﻿using System;
using System.Collections.Generic;
using Data;
using Fusion;
using Inventory;
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
    public abstract class SkillBase : NetworkBehaviourEx, IJsonData<SkillJsonData>, IJsonData<StatusJsonData>, IInventoryItemAdd
    {
        #region Static

        // Info Data 캐싱
        private static Dictionary<int, SkillJsonData> _infoDataCash = new Dictionary<int, SkillJsonData>();
        public static void AddInfoData(int id, SkillJsonData data) => _infoDataCash.TryAdd(id, data);
        public static SkillJsonData GetInfoData(int id) => _infoDataCash.TryGetValue(id, out var data) ? data : new SkillJsonData();
        public static void ClearInfosData() => _infoDataCash.Clear();
        public static bool IsInfoChasing;
        
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
        public Texture2D icon;
        public StatusValue<float> coolTime = new StatusValue<float>();

        public bool isInvoke; // 현재 스킬이 발동 중인지

        public StatusValue<int> level = new StatusValue<int>() { Max = 1 };
        public StatusValue<int> damage = new StatusValue<int>() { Max = 99999 };
        public StatusValue<float> duration = new StatusValue<float>();

        #endregion

        #region Unity Event Function

        public virtual void Start()
        {
            SetJsonData(GetInfoData(id));
            SetJsonData(GetStatusData(id));
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
            explain = json.Explain;
            coolTime.Max = json.CoolTime;
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
        
        #endregion
        
        #region RPC Function

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
        #endregion
    }
}