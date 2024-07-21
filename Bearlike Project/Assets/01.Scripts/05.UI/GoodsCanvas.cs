using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class GoodsCanvas : MonoBehaviour
    {
        [Header("Coin")] 
        [SerializeField] private TMP_Text bearCoinText;
        [SerializeField] private TMP_Text cottonCoinText;
        [SerializeField] private float coinScaleUpAnimationThreshold = 1.2f;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void CoinUpdate(int amount, TMP_Text targetText)
        {
            int startValue = int.Parse(targetText.text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            
            // 숫자가 올라가는 효과
            DOTween.To(() => startValue, x => startValue = x, amount, 0.5f)
                .OnUpdate(() => targetText.text = startValue.ToString("N0"))
                .SetEase(Ease.Linear);
            
            // Text가 커졌다 작아지는 효과
            targetText.transform.DOScale(coinScaleUpAnimationThreshold, 0.5f / 2)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => targetText.transform.DOScale(1f, 0.5f / 2).SetEase(Ease.OutQuad));
        }

        public void BearCoinUpdate(int amount) => CoinUpdate(amount, bearCoinText);
        public void CottonCoinUpdate(int amount) => CoinUpdate(amount, cottonCoinText);
    }
}