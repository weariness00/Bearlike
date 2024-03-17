using System.Collections.Generic;

namespace Script.Util
{
    public static class DictionaryUtil
    {
        public class MultiKey<TKey1, TKey2>
        {
            public TKey1 Key1 { get; set; }
            public TKey2 Key2 { get; set; }
            
            public MultiKey(TKey1 key)
            {
                Key1 = key;
            }
            
            public MultiKey(TKey2 key)
            {
                Key2 = key;
            }
            
            public MultiKey(TKey1 key1, TKey2 key2)
            {
                Key1 = key1;
                Key2 = key2;
            }
            
            public override int GetHashCode()
            {
                return 0;
            }
            
            public override bool Equals(object obj)
            {
                if (obj is MultiKey<TKey1, TKey2> multiKey)
                {
                    return Equals(Key1, multiKey.Key1) || Equals(Key2, multiKey.Key2);
                }
                if (obj is TKey1 key1)
                {
                    return Equals(Key1, key1);
                }
                if (obj is TKey2 key2)
                {
                    return Equals(Key2, key2);
                }
                return false;
            }
        }
        
        public static List<TValue> GetDictionaryItemList<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            List<TValue> values = new List<TValue>();

            foreach (var (key, value) in dict)
                values.Add(value);

            return values;
        }
        public static TValue[] GetDictionaryItemArray<TKey, TValue>(Dictionary<TKey, TValue> dict) => GetDictionaryItemList(dict).ToArray();
        
        public static List<TKey> GetDictionaryKeyList<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            List<TKey> keys = new List<TKey>();

            foreach (var (key, value) in dict)
                keys.Add(key);

            return keys;
        }
        public static TKey[] GetDictionaryKeyArray<TKey, TValue>(Dictionary<TKey, TValue> dict) => GetDictionaryKeyList(dict).ToArray();

    }
}