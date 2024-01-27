using Scripts.State.GameStatus;

namespace Item.Container
{
    public class ItemMoney : ItemBase
    {
        public StatusValue<int> moneyAmount = new StatusValue<int>();

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

