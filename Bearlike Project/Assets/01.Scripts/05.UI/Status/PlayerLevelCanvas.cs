using System;
using Player;
using TMPro;
using UnityEngine;

namespace UI.Status
{
    public class PlayerLevelCanvas : MonoBehaviour
    {
        public PlayerStatus playerStatus;

        public TMP_Text levelText;

        private int _currentLevel = 0;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_currentLevel != playerStatus.level.Current)
            {
                _currentLevel = playerStatus.level.Current;
                levelText.text = _currentLevel.ToString();
            }
        }
    }
}

