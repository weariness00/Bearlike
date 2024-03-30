using System;
using Fusion;

namespace Photon
{
    public class NetworkUtil
    {
        public static TItem[] DictionaryItems<TKey, TItem>(NetworkDictionary<TKey, TItem> dictionary)
        {
            if (dictionary.Count == 0) return Array.Empty<TItem>();
            TItem[] items = new TItem[dictionary.Count];

            int i = 0;
            foreach (var (key, item) in dictionary)
                items[i++] = item;

            return items;
        }
    }
}