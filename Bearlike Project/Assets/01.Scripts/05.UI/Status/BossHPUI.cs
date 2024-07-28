using Status;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Status
{
    public class BossHP : MonoBehaviour
    {
        public StatusBase statusBase;
        public TMP_Text hpText;
        public Image hpImage;

        private int _currentHp;
        private float _ratio;
        
        void Start()
        {
            _currentHp = statusBase.hp.Current;
            _ratio = Mathf.Ceil((float)(_currentHp) / (float)(statusBase.hp.Max) * 100) / 100;
            hpText.text = _ratio * 100 + " %";
            hpImage.fillAmount = _ratio;
        }
        
        void Update()
        {
            if (statusBase.hp.Current != _currentHp)
            {
                _currentHp = statusBase.hp.Current;
                _ratio = Mathf.Ceil((float)(_currentHp) / (float)(statusBase.hp.Max) * 100) / 100;
                hpText.text = _ratio * 100 + " %";
                hpImage.fillAmount = _ratio;
            }
        }
    }
}
