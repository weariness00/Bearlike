using System.Collections.Generic;
using Status;

namespace Item
{
    public class ItemJsonData
    {
        public int id;
        public string name;
        public string explain;
        public int amount;

        public Dictionary<string, StatusValue<int>> iStatusValueDictionary = new Dictionary<string, StatusValue<int>>();
        public Dictionary<string, StatusValue<float>> fStatusValueDictionary = new Dictionary<string, StatusValue<float>>();

        public StatusValue<int> GetStatusValueInt(string statusName)
        {
            if(iStatusValueDictionary.TryGetValue(statusName, out var statusValue)) {return statusValue;}
            return null;
        }
        public StatusValue<float> GetStatusValueFloat(string statusName)
        {
            if(fStatusValueDictionary.TryGetValue(statusName, out var statusValue)) {return statusValue;}
            return null;
        }
    }
}