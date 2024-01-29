using Script.Player;
using Scripts.State.GameStatus;

namespace Item.Container
{
    public class ItemExperience : ItemBase
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

        public override void GetItem<T>(T target)
        {
            if (target is PlayerController)
            {
                var pc = (PlayerController)(object)target;
                pc.status.IncreaseExp(experienceAmount);
                
                Destroy(gameObject);
            }
        }
    }
}

