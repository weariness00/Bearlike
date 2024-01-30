using Newtonsoft.Json;
using ProjectUpdate;
using Script.Manager;
using UnityEngine;
using Util;

namespace Item.Looting
{
    public class LootingSystem : Singleton<LootingSystem>
    {
        [SerializeField]private LootingItem[] _lootingItems;

        public void Start()
        {
            DebugManager.ToDo("루팅 테이블을 어떻게 캐싱 할 것인지 방법 정하기");
            JsonConvertExtension.Load(ProjectUpdateManager.Instance.monsterLootingTableList,
                (data) =>
                {
                    _lootingItems = JsonConvert.DeserializeObject<LootingItem[]>(data);
                    
                    DebugManager.Log("Monster Looting Table List를 불러왔습니다.");
                }
                );
        }
    }
}

