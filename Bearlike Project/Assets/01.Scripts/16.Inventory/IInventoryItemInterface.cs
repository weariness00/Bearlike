namespace UI.Inventory
{
    public interface IInventoryItemAdd
    {
        /// <summary>
        /// 인벤토리에 아이템이 추가될때 수행되어야 할 로직
        /// </summary>
        /// <param name="item"> Inventory에  해당 Item이 있으면 인자로 들어온다. 없으면 Null </param>
        public AddItem AddItem<AddItem>(AddItem addItem);
    }

    public interface IInventoryItemUse
    {
        public UseItem UseItem<UseItem>(UseItem useItem, out bool isDestroy);
    }

    public interface IInventoryUIUpdate
    {
        public void UIUpdateFromItem<UIItem>(UIItem uiItem);
    }
}