using Fusion;

namespace Script.Util
{
    public class NetworkUtil
    {
        public static TItem[] DictionaryItems<TKey, TItem>(NetworkDictionary<TKey, TItem> dictionary)
        {
            TItem[] items = new TItem[dictionary.Count];

            int i = 0;
            foreach (var (key, item) in dictionary)
                items[i++] = item;

            return items;
        }
    }
}