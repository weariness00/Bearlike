using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GoodsCanvas : MonoBehaviour
    {
        [Header("Coin")] 
        [SerializeField] private TMP_Text coinAmountText;
        private int _currentCoinAmount;
        [SerializeField] private float coinScaleUpAnimationThreshold = 1.2f;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void CoinUpdate(int amount)
        {
            int startValue = int.Parse(coinAmountText.text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            _currentCoinAmount = amount;
            
            // 숫자가 올라가는 효과
            DOTween.To(() => startValue, x => startValue = x, _currentCoinAmount, 0.5f)
                .OnUpdate(() => coinAmountText.text = startValue.ToString("N0"))
                .SetEase(Ease.Linear);
            
            // Text가 커졌다 작아지는 효과
            coinAmountText.transform.DOScale(coinScaleUpAnimationThreshold, 0.5f / 2)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => coinAmountText.transform.DOScale(1f, 0.5f / 2).SetEase(Ease.OutQuad));
        }
    }
}