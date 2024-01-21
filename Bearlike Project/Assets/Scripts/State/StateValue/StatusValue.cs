using System;
using UnityEngine;

namespace Script.GameStatus
{
    [System.Serializable]
    public class StatusValue<T> where T : struct, IComparable
    {
        public StatusValue()
        {
            
        }
        
        public T current
        {
            get => mCurrent;
            set
            {
                mCurrent = value;
                CheckCurrent();
            }
        }
        public T min
        {
            get => mMin;
            set => mMin = value;
        }
        
        public T max
        {
            get => mMax;
            set => mMax = value;
        }
        
        [SerializeField] private T mMin;
        [SerializeField] private T mMax;
        [SerializeField] private T mCurrent;
        
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