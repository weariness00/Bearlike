using Script.Data;
using UnityEngine;

namespace ProjectUpdate
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class ProjectUpdateManager : MonoBehaviour
    {
        void Start()
        {
            GoogleStorageManager.DownloadFile("bearlike-json", "DefaultKeyData.json",  $"{Application.dataPath}/Json/KeyManager");
        }
    }
}

