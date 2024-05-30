using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace GamePlay
{
    public class TreasureBoxCanvas : Singleton<TreasureBoxCanvas>
    {
        [SerializeField] private Transform blockParentTransform;
        [SerializeField] private GameObject blockObject;
        [SerializeField] private Button blockButton;
        [SerializeField] private TMP_Text blockExplainText;

        private readonly List<GameObject> _instanceBlockList = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            blockObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void InitConditionBlock(TreasureBox treasureBox)
        {
            if(gameObject.activeSelf) return;
            
            gameObject.SetActive(true);
            
            foreach (var obj in _instanceBlockList)
                Destroy(obj);
            
            blockObject.SetActive(true);
            var treasureBoxData = TreasureBox.GetTreasureBoxData(treasureBox.id);
            foreach (var condition in treasureBoxData.OpenConditions)
            {
                blockExplainText.text = condition.GetExplain();
                blockButton.onClick.AddListener(() =>
                {
                    treasureBox.isUse = true;
                    gameObject.SetActive(false);
                });

                var obj =Instantiate(blockObject, blockParentTransform);
                _instanceBlockList.Add(obj);
                blockButton.onClick.RemoveAllListeners();
            }
            blockObject.SetActive(false);
            
            GameUIManager.AddActiveUI(gameObject);
        }
    }
}