using System.Globalization;
using GamePlay.StageLevel.Container;
using TMPro;
using UnityEngine;
using Util;

namespace UI
{
    public class StageSurviveUI : MonoBehaviour
    {   
        public StageSurvive stage;
        [Header("Canvas")]
        public TMP_Text timeText;

        public void Update()
        {
            timeText.text = (stage.surviveTime - (int)stage.currentTime).TimeString();

            if (stage.isStageClear)
            {
                timeText.text = "00:00";
                Destroy(gameObject);
            }
        }
    }
}