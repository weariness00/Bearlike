﻿using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Item.Looting;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace GamePlay
{
    [RequireComponent(typeof(LootingTable))]
    public class TreasureBox : MonoBehaviour, IInteract, IJsonData<TreasureBoxJsonData>
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
        public bool isOpen; // 상자가 열려있는 상태인지
        public bool isUse; // 상자가 사용되었는지
        
        [Header("Component")]
        [SerializeField] private Transform itemDropPosition;
        [SerializeField] private LootingTable lootingTable;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip openClip;

        [Header("Sound")]
        [SerializeField] private AudioSource openSound;

        private static readonly int AniBoxOpen = Animator.StringToHash("t Open");

        private void Start()
        {
            lootingTable = GetComponent<LootingTable>();
            
            SetJsonData(GetTreasureBoxData(id));

            BoxEnable();
            InteractInit();
        }

        #region Member Function

        public void BoxEnable()
        {
            isActive = IsInteract = true;
        }

        public void BoxDisable()
        {
            isActive = IsInteract = false;
        }
        
        public void OnBox()
        {
            if(isActive == false || isUse) return;

            if (isOpen && _afterOpenBoxCoroutine == null)
            {
                TreasureBoxCanvas.Instance.InitConditionBlock(this);
            }
            else
            {
                if (openSound) openSound.Play();
                if (animator) animator.SetTrigger(AniBoxOpen);
                _afterOpenBoxCoroutine = StartCoroutine(AfterOpenBoxCoroutine());
                isOpen = true;
            }
        }

        private Coroutine _afterOpenBoxCoroutine;
        private IEnumerator AfterOpenBoxCoroutine()
        {
            yield return new WaitForSeconds(openClip.length);
            
            TreasureBoxCanvas.Instance.InitConditionBlock(this);
            lootingTable.SpawnDropItem(itemDropPosition.position);

            _afterOpenBoxCoroutine = null;
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

        #region Interact Interface
        
        public void InteractInit()
        {
            InteractEnterAction += SetInteractUI;
            InteractKeyDownAction += BoxInteractKeyDown;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
        public Action<GameObject> InteractKeyDownAction { get; set; }
        public Action<GameObject> InteractKeyUpAction { get; set; }

        void SetInteractUI(GameObject targetObject)
        {
            InteractUI.SetKeyActive(true);
            InteractUI.KeyCodeText.text = "F";
        }
        
        private void BoxInteractKeyDown(GameObject targetObject)
        {
            OnBox();
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
        [JsonProperty("Item ID")]public int ItemID;
        [JsonProperty("Money Amount")]public int MoneyAmount;
        [JsonProperty("Open Condition Explain")] public string Explain;

        public string GetExplain()
        {
            if (Explain.Contains("(Money)"))
                Explain = Explain.Replace("(Money)", $"{MoneyAmount}");
            
            return StringExtension.Replace(Explain);
        }
    }
    
    public struct TreasureBoxJsonData
    {
        [JsonProperty("ID")] public int ID;
        [JsonProperty("Explain")] public string Explain;
        [JsonProperty("LootingTable")] public LootingItem[] LootingItems;
        [JsonProperty("Condition")] public TreasureBoxOpenCondition[] OpenConditions;
    }
}