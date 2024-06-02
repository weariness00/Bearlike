using System;
using Script.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class SoundManagerCanvas : MonoBehaviour
    {
        [Header("BGM")] 
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Button bgmUpButton;
        [SerializeField] private Button bgmDownButton;
        private static readonly string PlayerPrefBGMVolume = "BGM Volume";
        
        [Header("Effect")] 
        [SerializeField] private Slider effectSlider;
        [SerializeField] private Button effectUpButton;
        [SerializeField] private Button effectDownButton;
        private static readonly string PlayerPrefEffectVolume = "Effect Volume";

        private void Start()
        {
            // BGM
            bgmSlider.minValue = 0;
            bgmSlider.maxValue = 100;
            bgmSlider.value = PlayerPrefs.GetFloat(PlayerPrefBGMVolume);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            bgmUpButton.onClick.AddListener(()=>
            {
                bgmSlider.value += 1f;
                SetBGMVolume(bgmSlider.value);
            });
            bgmDownButton.onClick.AddListener(()=>
            {
                bgmSlider.value -= 1f;
                SetBGMVolume(bgmSlider.value);
            });
            SetBGMVolume(bgmSlider.value);
            
            // Effect
            effectSlider.minValue = 0;
            effectSlider.maxValue = 100;
            effectSlider.value = PlayerPrefs.GetFloat(PlayerPrefEffectVolume);
            effectSlider.onValueChanged.AddListener(SetEffectVolume);
            effectUpButton.onClick.AddListener(()=>
            {
                bgmSlider.value += 1f;
                SetEffectVolume(bgmSlider.value);
            });
            effectDownButton.onClick.AddListener(()=>
            {
                bgmSlider.value -= 1f;
                SetEffectVolume(bgmSlider.value);
            });
            SetEffectVolume(bgmSlider.value);
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetFloat(PlayerPrefBGMVolume, bgmSlider.value);
            PlayerPrefs.SetFloat(PlayerPrefEffectVolume, bgmSlider.value);
        }

        public void SetBGMVolume(float value)
        {
            SoundManager.SetVolume(SoundManager.SoundType.BGM, SoundManager.LinearToDecibel(value));
        }

        public void SetEffectVolume(float value)
        {
            SoundManager.SetVolume(SoundManager.SoundType.Effect, SoundManager.LinearToDecibel(value));
        }
    }
}