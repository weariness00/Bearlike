using UnityEngine;

namespace Util
{
    public static class ProbabilityExtension
    {
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