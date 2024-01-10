using UnityEngine;

namespace Script.GameStatus
{
    [System.Serializable]
    public class StatusValue
    {
        public StatusValue()
        {
            Max = 100;
            Current = 100;
        }
        
        public int Current
        {
            get => _current;
            set
            {
                _current = value;
                CheckCurrent();
            }
        }
        public int Min
        {
            get => _min;
            set => _min = value;
        }
        
        public int Max
        {
            get => _max;
            set => _max = value;
        }
        
        [SerializeField] private int _min;
        [SerializeField] private int _max;
        [SerializeField] private int _current;
        
        public bool isMin;
        public bool isMax;
        
        void CheckCurrent()
        {
            if (_current < _min)
            {
                _current = _min;
            }
            else if (_current > _max)
            {
                _current = _max;
            }
        }
    }
}