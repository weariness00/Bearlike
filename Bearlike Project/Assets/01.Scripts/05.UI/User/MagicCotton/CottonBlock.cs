using TMPro;
using UnityEngine;
using UnityEngine.UI;
using User;
using User.MagicCotton;

namespace UI.User
{
    public class CottonBlock : MonoBehaviour
    {
        [SerializeField] private MagicCottonBase magicCottonBase;
        
        [SerializeField] private RectTransform blockRect;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text levelText;

        private int _maxLevel = 0;
        private void Awake()
        {
            levelUpButton.onClick.AddListener(LevelUp);
        }

        public void SetMagicCotton(MagicCottonBase mc)
        {
            icon.sprite = mc.icon;
            SetMaxLevel(mc.Level.Max);
            
            magicCottonBase = mc;
        }

        public void SetIcon(Sprite sprite) => icon.sprite = sprite;

        public void SetLevel()
        {
            levelText.text = $"{magicCottonBase.Level.Current} / {magicCottonBase.Level.Max}";
        }
        
        public void SetMaxLevel(int maxLevel)
        {
            _maxLevel = maxLevel;
            levelText.text = $"0 / {maxLevel}";
        }

        public void SetCurrentLevel(int level)
        {
            levelText.text = $"{level} / {_maxLevel}";
        }

        private void LevelUp()
        {
            var coin = UserInformation.Instance.cottonInfo.GetCoin();
            if (magicCottonBase.NeedExperience <= coin)
            {
                UserInformation.Instance.cottonInfo.SetCoin(coin - magicCottonBase.NeedExperience);
                
                magicCottonBase.LevelUp();
                levelText.text = $"{magicCottonBase.Level.Current} / {magicCottonBase.Level.Max}";
            
                if(magicCottonBase.Level.isMax)
                    levelUpButton.onClick.RemoveListener(LevelUp);
            }
        }
    }
}

