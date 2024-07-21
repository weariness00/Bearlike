using System;
using System.Collections;
using Fusion;
using Manager;
using Photon;
using Status;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    public class PlayerHP : NetworkBehaviourEx
    {
        public StatusBase statusBase;
        public TMP_Text hpText;
        public Image hpImage;
        public TMP_Text nameText;
        
        [SerializeField]private Animation damageAnimation;

        private int _currentHp;
        private float _ratio;

        void Start()
        {
            _currentHp = statusBase.hp.Current;
            _ratio = ((float)(_currentHp) / (float)(statusBase.hp.Max));
            hpText.text = _ratio * 100 + "%";
            hpImage.fillAmount = _ratio;
        }
        
        void Update()
        {
            if (statusBase.hp.Current != _currentHp)
            {
                if (statusBase.hp.Current < _currentHp)
                {
                    damageAnimation.Play();
                }
                
                _currentHp = statusBase.hp.Current;
                _ratio = _currentHp / (float)(statusBase.hp.Max);
                hpText.text = _ratio * 100 + "%";
                
                StartCoroutine(LerpHealth(_ratio));
            }
        }

        private IEnumerator LerpHealth(float targetHealth)
        {
            float startHealth = hpImage.fillAmount;
            
            float elapsedTime = 0f;
            float duration = 0.5f; // 보간에 걸리는 시간
            while (elapsedTime < duration)
            {
                // elapsedTime += Runner.DeltaTime;
                elapsedTime += Time.deltaTime;
                hpImage.fillAmount = Mathf.Lerp(startHealth, targetHealth, elapsedTime / duration);
                yield return null;
            }

            hpImage.fillAmount = targetHealth;
        }
    }
}
