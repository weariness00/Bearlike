using System.Collections.Generic;
using Manager;
using TMPro;
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
            
            var treasureBoxData = TreasureBox.GetTreasureBoxData(treasureBox.id);
            foreach (var condition in treasureBoxData.OpenConditions)
            {
                blockExplainText.text = condition.GetExplain();

                var obj =Instantiate(blockObject, blockParentTransform);
                var objButton = obj.GetComponent<Button>();
                objButton.onClick.AddListener(() =>
                {
                    if (treasureBox.ConditionSatisfaction(condition))
                    {
                        treasureBox.isUse = true;
                        gameObject.SetActive(false);
                    }
                });
                
                obj.SetActive(true);
                
                _instanceBlockList.Add(obj);
            }
            
            UIManager.AddActiveUI(gameObject);
        }
    }
}