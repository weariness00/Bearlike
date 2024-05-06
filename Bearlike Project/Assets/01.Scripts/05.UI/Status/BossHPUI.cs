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
            _ratio = ((float)(_currentHp) / (float)(statusBase.hp.Max));
            hpText.text = _ratio * 100 + " %";
            hpImage.fillAmount = _ratio;
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
                statusBase.hp.Current -= 10;
            if (statusBase.hp.Current != _currentHp)
            {
                _currentHp = statusBase.hp.Current;
                _ratio = ((float)(_currentHp) / (float)(statusBase.hp.Max));
                hpText.text = _ratio * 100 + " %";
                hpImage.fillAmount = _ratio;
            }
        }
    }
}
