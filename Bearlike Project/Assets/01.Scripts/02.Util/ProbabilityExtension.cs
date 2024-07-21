using UnityEngine;

namespace Util
{
    public static class ProbabilityExtension
    {
        /// <summary>
        /// 0~maxProbability에서 랜덤으로 뽑은 숫자가 probability보다 낮으면 성공
        /// </summary>
        /// <param name="probability">현재 확률</param>
        /// <param name="maxProbability">확률의 최대치</param>
        /// <returns></returns>
        public static bool IsProbability(this float probability, float maxProbability)
        {
            var value = Random.Range(0f, maxProbability);
            
            return value < probability;
        }
        
        public static bool IsProbability(this int probability, int maxProbability)
        {
            var value = Random.Range(0, maxProbability);
            
            return value < probability;
        }
    }
}