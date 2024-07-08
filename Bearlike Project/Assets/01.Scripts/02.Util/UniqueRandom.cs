using System.Collections.Generic;
using Manager;
using Random = UnityEngine.Random;

namespace Util
{
    /// <summary>
    /// Random을 생성할때 이미 전에 생성된 값은 안나오게 해주는 클래스
    /// </summary>
    public class UniqueRandom
    {
        public UniqueRandom(int min, int max)
        {
            Initialize(min, max);
        }
        
        private List<int> _uniqueIntList;
        public int Length => _uniqueIntList.Count;
        
        /// <summary>
        /// [min, max) 를 포함한 랜덤
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Initialize(int min, int max)
        {
            _uniqueIntList = new List<int>();
            for (int i = 0; i < max; i++)
            {
                _uniqueIntList.Add(i);
            }
        }
        
        public int RandomInt()
        {
            if (_uniqueIntList == null)
            {
                DebugManager.LogError("UniqueRandom의 Array를 초기화 하기 위해 먼저 Initialize 메서드를 호출해주세요");
                return -1;
            }
            else if (_uniqueIntList.Count == 0)
            {
                DebugManager.LogWarning("UniqueRandom에서 더이상 얻을 Random 값이 없습니다.");
                return -1;
            }

            var index = Random.Range(0, _uniqueIntList.Count);
            var value = _uniqueIntList[index];
            _uniqueIntList.RemoveAt(index);
            return value;
        }

    }
}

