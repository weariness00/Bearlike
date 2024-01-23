using System;
using UnityEngine;

namespace Inho.Scripts.Skill.TimeValue
{
    [global::System.Serializable]
    public class TimeValue
    {
        public TimeValue()
        {
            
        }
        
        public float current
        {
            get => mCurrent;
            set
            {
                mCurrent = value;
                CheckCurrent();
            }
        }
        public float min
        {
            get => mMin;
            set => mMin = value;
        }
        
        public float max
        {
            get => mMax;
            set => mMax = value;
        }
        
        [SerializeField] private float mMin;
        [SerializeField] private float mMax;
        [SerializeField] private float mCurrent;
        
        public bool IsMin;
        public bool IsMax;
        
        void CheckCurrent()
        {
            if (mCurrent.CompareTo(mMin) < 0)
            {
                mCurrent = mMin;
            }
            else if (mCurrent.CompareTo(mMax) > 0)
            {
                mCurrent = mMax;
            }
        }
    }
}