using System;
using GamePlay.StageLevel.Container;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StageDestroyUI : MonoBehaviour
    {
        public StageDestroy stage;

        public TMP_Text maxKillText;
        public TMP_Text killText;

        private int _killCount;

        private void Start()
        {
            maxKillText.text = stage.monsterKillCount.Max.ToString();
            killText.text = stage.monsterKillCount.Current.ToString();
        }

        private void Update()
        {
            if (stage.monsterKillCount.Current != _killCount)
            {
                killText.text = stage.monsterKillCount.Current.ToString();
                _killCount = stage.monsterKillCount.Current;
            }

            if (stage.isStageClear)
            {
                Destroy(gameObject);
            }
        }
    }
}