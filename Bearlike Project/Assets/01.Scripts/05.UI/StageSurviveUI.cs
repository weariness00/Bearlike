using System.Globalization;
using GamePlay.StageLevel.Container;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StageSurviveUI : MonoBehaviour
    {   
        public StageSurvive stage;
        
        [Header("Canvas")]
        public TMP_Text timeText;

        public void Update()
        {
            timeText.text = TimeString((int)stage.currentTime);

            if (stage.isStageClear)
            {
                Destroy(gameObject);
            }
        }

        string TimeString(int minute)
        {
            int hour = minute / 60;
            minute %= 60;
            
            // 시간과 분을 "00:00" 형식의 문자열로 포맷팅
            return $"{hour}:{minute:00}";
        }
    }
}