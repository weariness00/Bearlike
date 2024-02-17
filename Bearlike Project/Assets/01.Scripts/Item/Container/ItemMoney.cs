using Script.Player;
using Scripts.State.GameStatus;

namespace Item.Container
{
    public class ItemMoney : ItemBase
    {
        public StatusValue<int> moneyAmount = new StatusValue<int>();

        public override void GetItem<T>(T target)
        {
            base.GetItem(target);
            if (target is PlayerController)
            {
                var pc = (PlayerController)(object)target;
                if (pc.itemList.TryGetValue(id, out var item))
                {
                    item.amount.Current += item.amount.Current;
                }
                else
                {
                    pc.itemList.Add(id, this);
                }
            }
        }

        public override ItemJsonData GetJsonData()
        {
            var json = base.GetJsonData();
            json.iStatusValueDictionary.Add("MoneyAmount", moneyAmount);
            return json;
        }

        public override void SetJsonData(ItemJsonData json)
        {
            base.SetJsonData(json);
            moneyAmount = json.GetStatusValueInt("MoneyAmount");
        }
    }
}

