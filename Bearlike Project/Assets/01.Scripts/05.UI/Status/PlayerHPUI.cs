using System;
using System.Collections;
using Status;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    public class PlayerHP : MonoBehaviour
    {
        public StatusBase statusBase;
        public TMP_Text hpText;
        public Image hpImage;

        private int _currentHp;
        private float _ratio;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
            _currentHp = statusBase.hp.Current;
            _ratio = ((float)(_currentHp) / (float)(statusBase.hp.Max));
            hpText.text = _ratio * 100 + " %";
            hpImage.fillAmount = _ratio;
        }
        
        void Update()
        {
            if (statusBase.hp.Current != _currentHp)
            {
                _currentHp = statusBase.hp.Current;
                _ratio = ((float)(_currentHp) / (float)(statusBase.hp.Max));
                
                // StartCoroutine(InterporationHPCoroutine(statusBase.hp.Current < _currentHp));
                // text 에니메이션 넣기
                hpText.text = _ratio * 100 + " %";
                hpImage.fillAmount = _ratio;
            }
        }

        IEnumerator InterporationHPCoroutine(bool type)
        {
            float rate = hpImage.fillAmount - _ratio;
            if (type)
            {
                while (hpImage.fillAmount >= _ratio)
                {
                    hpImage.fillAmount -= rate / 5;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                while (hpImage.fillAmount <= _ratio)
                {
                    hpImage.fillAmount -= rate / 5;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            hpImage.fillAmount = _ratio;
        }
    }
}
