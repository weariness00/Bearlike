using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PerformanceCanvas : MonoBehaviour
    {
        private static readonly string FPSPrefs = "FPS";

        [SerializeField] private PerformanceDisplayCanvas display;
        [SerializeField] private Toggle fpsToggle;

        private void Awake()
        {
            if (PlayerPrefs.HasKey(FPSPrefs))
            {
                fpsToggle.isOn = PlayerPrefs.GetString(FPSPrefs) == "T";
                display.OnFPS(fpsToggle.isOn);
            }
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString(FPSPrefs, fpsToggle.isOn ? "T" : "F");
        }
    }
}