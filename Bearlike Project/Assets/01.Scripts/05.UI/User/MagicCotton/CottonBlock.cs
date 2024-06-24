using TMPro;
using UnityEngine;
using UnityEngine.UI;
using User;
using User.MagicCotton;

namespace UI.User
{
    public class CottonBlock : MonoBehaviour
    {
        public RectTransform blockRect;
        public Button levelUpButton;
        public Image icon;
        public TMP_Text levelText;

        private MagicCottonBase _magicCottonBase;

        private void Awake()
        {
            levelUpButton.onClick.AddListener(LevelUp);
        }

        public void SetMagicCotton(MagicCottonBase mc)
        {
            icon.sprite = mc.icon;
            SetLevel(mc.Level.Max);
            
            _magicCottonBase = mc;
        }
        
        public void SetLevel(int maxLevel)
        {
            levelText.text = $"0 / {maxLevel}";
        }

        private void LevelUp()
        {
            _magicCottonBase.LevelUp(UserInformation.Instance.cottonCoin);
            levelText.text = $"{_magicCottonBase.Level.Current} / {_magicCottonBase.Level.Max}";
            
            if(_magicCottonBase.Level.isMax)
                levelUpButton.onClick.RemoveListener(LevelUp);
        }
    }
}

