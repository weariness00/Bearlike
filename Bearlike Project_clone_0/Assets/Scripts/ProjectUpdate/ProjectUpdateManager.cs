using Script.Data;
using UnityEngine;
using Util;

namespace ProjectUpdate
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class ProjectUpdateManager : Singleton<ProjectUpdateManager>
    {
        private readonly string _json = "bearlike-json";

        public readonly string monsterLootingTableList = "Monster Looting Table List.json";
        
        void Start()
        {
            GoogleStorageManager.DownloadFile("bearlike-json", "DefaultKeyData.json",  $"{Application.dataPath}/Json/KeyManager");
            GoogleStorageManager.DownloadFile(_json, monsterLootingTableList,$"{Application.dataPath}/Json/Looting Table");
        }
    }
}

