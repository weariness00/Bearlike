using System;
using GamePlay.Stage;
using GamePlay.StageLevel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StageSelectUIHandler : MonoBehaviour
    {
        public Toggle toggle;
        public RawImage image;
        public TMP_Text titleText;
        public TMP_Text explainText;
        public TMP_Text voteText;

        public void Setting(StageInfo stageInfo)
        {
            // image.texture = stageInfo.image;
            
            titleText.text = stageInfo.title;
            explainText.text = stageInfo.explain;
            image.texture = stageInfo.image;
        }
    }
}

