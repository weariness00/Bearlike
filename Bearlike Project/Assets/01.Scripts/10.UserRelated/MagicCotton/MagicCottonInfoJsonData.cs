using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace User
{
    public class MagicCottonInfoJsonData
    {
        [JsonProperty("ID")] public int id;
        [JsonProperty("Name")] public string name;
        [JsonProperty("Max Level")] public int maxLevel;
        [JsonProperty("Need Coin")] private Dictionary<int, int> needCoinDict;

        public int[] GetNeedCoinArray()
        {
            int[] array = new int[maxLevel];
            foreach (var (level, needCoin) in needCoinDict)
            {
                array[level - 1] = needCoin;
            }

            return array;
        }
    }
}

