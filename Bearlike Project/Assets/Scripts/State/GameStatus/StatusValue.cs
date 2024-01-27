using System;
using UnityEngine;

namespace Scripts.State.GameStatus
{
    [System.Serializable]
    public class StatusValue<T> where T : struct, IComparable
    {
        public static implicit operator T(StatusValue<T> value)
        {
            return value.Current;
        }
        
        public T Current
        {
            get => _current;
            set
            {
                _current = value;
                CheckCurrent();
            }
        }
        public T Min
        {
            get => _min;
            set => _min = value;
        }
        
        public T Max
        {
            get => _max;
            set => _max = value;
        }
        
        [SerializeField] private T _min;
        [SerializeField] private T _max;
        [SerializeField] private T _current;
        
        public bool isMin;
        public bool isMax;
        
        void CheckCurrent()
        {
            if (_current.CompareTo(_min) < 0)
            {
                _current = _min;
            }
            else if (_current.CompareTo(_max) > 0)
            {
                _current = _max;
            }
        }
    }
}