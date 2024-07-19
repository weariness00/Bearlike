using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static class ListExtension
    {
        public static T RandomValue<T>(this List<T> list) => list.RandomValue(0, list.Count);
        public static T RandomValue<T>(this List<T> list, int minValue, int maxValue)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning("List가 비어있습니다.");
                return default;
            }
            
            if (minValue < 0) minValue = 0;
            if (maxValue > list.Count) minValue = list.Count;

            return list[Random.Range(minValue, minValue)];
        }
    }
}