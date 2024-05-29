using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Item.Looting;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay
{
    [RequireComponent(typeof(LootingTable))]
    public class TreasureBox : MonoBehaviour, IJsonData<TreasureBoxJsonData>
    {
        #region Static

        // Json Data 캐싱
        private static readonly Dictionary<int, TreasureBoxJsonData> LootingDataChasing = new Dictionary<int, TreasureBoxJsonData>();
        public static void AddTreasureBoxData(int id, TreasureBoxJsonData data) => LootingDataChasing.TryAdd(id, data);
        public static TreasureBoxJsonData GetTreasureBoxData(int id) => LootingDataChasing.TryGetValue(id, out var data) ? data : new TreasureBoxJsonData();
        public static void ClearTreasureBoxData() => LootingDataChasing.Clear();

        #endregion
        
        [Header("Info")]
        public int id;
        public string explain;
        public bool isActive; // 상자가 활성화 되어 열 수 있는 상태인지
        public bool isUse; // 상자가 사용되었는지
        
        [Header("Component")]
        [SerializeField] private Transform itemDropPosition;
        [SerializeField] private LootingTable lootingTable;

        [Header("Animation")]
        [SerializeField] private new Animation animation;
        [SerializeField] private AnimationClip openClip;

        [Header("Sound")]
        [SerializeField] private AudioSource openSound;
        
        private void Start()
        {
            lootingTable = GetComponent<LootingTable>();
            animation = GetComponent<Animation>();

            openClip = animation.GetClip("Box Open");
            
            SetJsonData(GetTreasureBoxData(id));
        }

        #region Member Function

        public void BoxEnable()
        {
            isActive = true;
        }

        public void BoxDisable()
        {
            isActive = false;
        }
        
        public void OnBox()
        {
            if(isActive == false || isUse) return;

            if (openSound) openSound.Play();
            if (animation) animation.Play(); 
            StartCoroutine(AfterOpenBoxCoroutine());
        }

        private IEnumerator AfterOpenBoxCoroutine()
        {
            yield return new WaitForSeconds(openClip.length);
            
            lootingTable.SpawnDropItem(itemDropPosition.position);
        }
        
        #endregion

        #region Json Data Interface

        public TreasureBoxJsonData GetJsonData()
        {
            return new TreasureBoxJsonData();
        }

        public void SetJsonData(TreasureBoxJsonData json)
        {
            id = json.ID;
            explain = json.Explain;
            lootingTable.CalLootingItem(json.LootingItems);
        }
        
        #endregion

    }

    // 상자 개방 조건중에 사용되는 타입은 무엇인지
    public enum TreasureBoxOpenConditionType
    {
        Money,
    }
    
    // 상자 개방 조건
    public struct TreasureBoxOpenCondition
    {
        [JsonProperty("Open Condition Type")]public TreasureBoxOpenConditionType ConditionType;
        [JsonProperty("Target ID")]public int TargetID;
        [JsonProperty("Amount")]public float Amount;
        [JsonProperty("Open Condition Explain")] public string Explain;
    }
    
    public struct TreasureBoxJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Explain")] public string Explain;
        [JsonProperty("LootingTable")] public LootingItem[] LootingItems;
        [JsonProperty("Open Condition")] public TreasureBoxOpenCondition[] OpenConditions;
    }
}