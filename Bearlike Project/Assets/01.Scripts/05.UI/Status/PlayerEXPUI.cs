using System;
using System.Collections;
using Fusion;
using Manager;
using Photon;
using Player;
using Status;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Status
{
    public class PlayerEXP : NetworkBehaviourEx
    {
        public PlayerStatus playerStatus;
        
        public TMP_Text expText;
        public TMP_Text levelText;
        
        public Image expImage;
        
        [SerializeField]private Animation damageAnimation;

        private int _currentExp;
        private float _ratio;

        private int _currentLevel = 0;
        
        private void Awake()
        {
            playerStatus = GetComponentInChildren<PlayerStatus>();
            gameObject.SetActive(false);
        }

        void Start()
        {
            _currentExp = playerStatus.experience.Current;
            _ratio = ((float)(_currentExp) / (float)(playerStatus.experience.Max));
            expText.text = Mathf.Floor(_ratio * 10000f) /  100f + "%";
            expImage.fillAmount = _ratio;
        }
        
        void Update()
        {
            if (playerStatus.experience.Current != _currentExp)
            {
                if (playerStatus.experience.Current < _currentExp)
                {
                    damageAnimation.Play();
                }
                
                _currentExp = playerStatus.experience.Current;
                _ratio = _currentExp / (float)(playerStatus.experience.Max);
                expText.text = Mathf.Floor(_ratio * 10000f) /  100f + "%";
                
                StartCoroutine(LerpExp(_ratio));
            }
            
            if (_currentLevel != playerStatus.level.Current)
            {
                _currentLevel = playerStatus.level.Current;
                levelText.text = _currentLevel.ToString();
            }
        }
        
        private IEnumerator LerpExp(float targetExp)
        {
            float startExp = expImage.fillAmount;
            
            float elapsedTime = 0f;
            float duration = 0.5f; // 보간에 걸리는 시간
            while (elapsedTime < duration)
            {
                // elapsedTime += Runner.DeltaTime;
                elapsedTime += Time.deltaTime;
                expImage.fillAmount = Mathf.Lerp(startExp, targetExp, elapsedTime / duration);
                yield return null;
            }

            expImage.fillAmount = targetExp;
        }
    }
}
