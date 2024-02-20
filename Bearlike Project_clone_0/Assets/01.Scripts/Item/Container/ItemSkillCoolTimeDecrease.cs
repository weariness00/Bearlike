using Player;
using Scripts.State.GameStatus;

namespace Item.Container
{
    /// <summary>
    /// 플레이어의 스킬 쿨타임을 일정 비율로 줄여주는 아이템
    /// ex ) 쿨타임 100초 x 감소 비율 10% = 쿨타임 90초
    /// </summary>
    public class ItemSkillCoolTimeDecrease : ItemBase
    {
        public StatusValue<float> decreasePercentage = new StatusValue<float>();

        public override void GetItem<T>(T target)
        {
            base.GetItem(target);
            if (target is PlayerController)
            {
                var pc = (PlayerController)(object)target;
                if (pc.itemList.TryGetValue(id, out var item))
                {
                    item.amount.Current += amount.Current;
                }
                else
                {
                    pc.itemList.Add(id, this);
                }
            }
        }
    }
}

