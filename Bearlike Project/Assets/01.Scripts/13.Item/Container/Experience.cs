using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    public class Experience : ItemBase
    {
        public StatusValue<int> experienceAmount = new StatusValue<int>();

        public override ItemJsonData GetJsonData()
        {
            var data = base.GetJsonData();
            data.name = "Experience";
            data.iStatusValueDictionary.Add("ExperienceAmount", experienceAmount);

            return data;
        }

        public override void SetJsonData(ItemJsonData json)
        {
            base.SetJsonData(json);
            experienceAmount = json.GetStatusValueInt("ExperienceAmount");
        }
    }
}
