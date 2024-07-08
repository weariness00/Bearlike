using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class BuffBlockHandle : MonoBehaviour
    {
        public Image icon;
        [SerializeField] private Image timeImage;
        public TMP_Text stackText;

        // 0~1 사이값을 Value로 줘야한다.
        public void SetTimer(float value) => timeImage.fillAmount = value;
    }
}