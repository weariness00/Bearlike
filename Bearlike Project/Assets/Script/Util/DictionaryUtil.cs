using System.Collections.Generic;

namespace Script.Util
{
    public static class DictionaryUtil
    {
        public static List<TValue> GetDictionaryItemList<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            List<TValue> values = new List<TValue>();

            foreach (var (key, value) in dict)
                values.Add(value);

            return values;
        }
        public static TValue[] GetDictionaryItemArray<TKey, TValue>(Dictionary<TKey, TValue> dict) => GetDictionaryItemList(dict).ToArray();

    }
}