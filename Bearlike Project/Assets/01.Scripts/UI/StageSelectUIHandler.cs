using System;
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

        public void Setting(StageLevelInfo stageInfo)
        {
            // image.texture = stageInfo.image;
            
            titleText.text = stageInfo.title;
            explainText.text = stageInfo.explain;

            explainText.rectTransform.position += new Vector3(0, explainText.preferredHeight, 0);
        }
    }
}

