using System.Collections.Generic;
using Newtonsoft.Json;

namespace User
{
    public struct MagicCottonInfoJsonData
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

