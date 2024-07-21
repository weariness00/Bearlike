using System.Linq;
using Util;

namespace Item.Container
{
    // 랜덤한 아이템을 떨구는 아이템
    public class RandomItem : ItemBase
    {
        public override void Awake()
        {
            base.Awake();

            var idList = ItemObjectList.ItemIDArray().ToList();
            idList.Remove(-1);
            
            var idIndex = idList.RandomValue();
            var spawnItem = ItemObjectList.GetFromId(idIndex);
            if (spawnItem) Instantiate(spawnItem, transform.position, transform.rotation);
            
            // 아이템을 스폰한 후 즉시 제거
            Destroy(gameObject);
        }
    }
}

