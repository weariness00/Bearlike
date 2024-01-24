using Script.Player;
using Scripts.State.GameStatus;

namespace Item.Container
{
    public class ItemExperience : ItemBase
    {
        public StatusValue<int> experienceAmount;

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

