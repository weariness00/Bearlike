using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PerformanceDisplayCanvas : MonoBehaviour
    {
        [SerializeField] private TMP_Text fpsText;
        private bool isUpdateFPS;
        private float fpsTime = 0f;

        private void Update()
        {
            if (isUpdateFPS) UpdateFPS();
        }

        private void UpdateFPS()
        {
            fpsTime += (Time.unscaledDeltaTime - fpsTime) * 0.1f;
            float fps = 1.0f / fpsTime;
            fpsText.text = $"FPS : {fps:0.}";
        }

        public void OnFPS(bool value)
        {
            isUpdateFPS = value;
            fpsText.gameObject.SetActive(value);
        }
    }
}