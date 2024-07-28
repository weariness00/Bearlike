using System;
using GamePlay.Stage.Container;
using GamePlay.StageLevel.Container;
using Status;
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
        private int _killMax;
        private StatusValue<int> _monsterKill;

        private void Start()
        {
            _monsterKill = stage.monsterKillCount;
            _killCount = _monsterKill.Current;
            _killMax = _monsterKill.Max;
        }

        private void Update()
        {
            if (_monsterKill.Max != _killMax)
            {
                maxKillText.text = _monsterKill.Max.ToString();
                _killMax = _monsterKill.Max;
            }
                
            if (_monsterKill.Current != _killCount)
            {
                killText.text = _monsterKill.Current.ToString();
                _killCount = _monsterKill.Current;
            }

            if (stage.isStageClear)
            {
                Destroy(gameObject);
            }
        }
    }
}