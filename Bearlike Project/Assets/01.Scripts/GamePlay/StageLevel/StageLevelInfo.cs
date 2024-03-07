﻿using Fusion;
using UnityEngine;

namespace GamePlay.StageLevel
{
    [System.Serializable]
    public struct StageLevelInfo
    {
        public StageLevelType StageLevelType;
        public string title;
        public string explain;
        public Texture2D image;
    }
}