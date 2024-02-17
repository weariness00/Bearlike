using System.IO;
using Script.Data;
using Script.Manager;
using UnityEngine;
using Util;

namespace ProjectUpdate
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class ProjectUpdateManager : Singleton<ProjectUpdateManager>
    {
        private readonly string _sessionLobby = "session-lobby";
        private readonly string _json = "bearlike-json";
        

        public readonly string monsterLootingTableList = "Monster Looting Table List.json";


        #region Static Function

        public static void DownLoadToStorage(string bucketName, string fileName, string savePath)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = $"{fileName}.json";
            savePath = Path.Combine($"{Application.persistentDataPath}/{savePath}");
            var value = GoogleStorageManager.DownloadFile(bucketName, fileName, savePath);
            DebugManager.Log($"구글 스토리지에서 다운 상태 : {value}\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"경로 : {savePath}");
        }
        public static void DownLoadJsonToStorage(string fileName) => DownLoadToStorage(Instance._json, fileName, "Json");
        public static bool UploadJsonToStorage(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = $"{fileName}.json";
            var savePath = Path.Combine($"{Application.persistentDataPath}/Json");
            var value = IsJsonFileVersionDifferent(fileName);
            if (value)
            {
                value = GoogleStorageManager.UploadJsonFile(Instance._json, savePath, fileName);
            }
            DebugManager.Log($"구글 스토리지에서 업로드 상태 : {value}\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"경로 : {savePath}");

            return value;
        }

        public static bool IsJsonFileVersionDifferent(string fileName)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = $"{fileName}.json";
            var savePath = Path.Combine($"{Application.persistentDataPath}/Json");
            return GoogleStorageManager.IsFileVersionDifferent(Instance._json, fileName, savePath);
        }

        #endregion

        void Start()
        {
            DownLoadJsonToStorage("DefaultKeyData.json");
            DownLoadJsonToStorage(monsterLootingTableList);
            // GoogleStorageManager.DownloadFile(_json, monsterLootingTableList, $"{Application.dataPath}/Json/Monster Looting Table List");
        }
    }
}