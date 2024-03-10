using System.Collections.Generic;
using System.IO;
using Manager;
using Newtonsoft.Json;
using Script.Data;
using UnityEngine;
using Util;

namespace ProjectUpdate
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneStart)]
    public class ProjectUpdateManager : Singleton<ProjectUpdateManager>
    {
        private readonly string _sessionLobby = "session-lobby";
        private readonly string _json = "bearlike-json";

        
        public readonly string downloadList = "Download_List";
        public readonly string serverInfo = "Server Information"; // 서버의 정보를 담고 있다.
        public readonly string monsterLootingTableList = "Monster Looting Table List.json";
        public readonly string stageLootingTableList = "Stage Looting Table List.json";

        public readonly WebManager.WebDownInfo download = new WebManager.WebDownInfo("DownloadList", "");
        public List<DownloadInfo> DownloadInfoList = new List<DownloadInfo>();
        
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
            DownLoadJsonToStorage(serverInfo); // 스토리지에서 웹 서버 정보 가져오기
            JsonConvertExtension.Load(serverInfo, (data) =>
            {
                // 웹 서버 정보를 토대로 다운 받아야할 json 데이터들 다운 받기
                WebManager.Instance.webServerInfo = JsonConvert.DeserializeObject<WebManager.WebServerInfo>(data);
                WebManager.DownloadJson(download, (json) =>
                {
                    DownloadInfoList = JsonConvert.DeserializeObject<List<DownloadInfo>>(json);
                    foreach (var downloadInfo in DownloadInfoList)
                    {
                        WebManager.DownloadJson(downloadInfo.URL, downloadInfo.Name, json =>{}, true, true);
                    }
                }, true);
            });
        }

        public struct DownloadInfo
        {
            [JsonProperty("URL")] public string URL;
            [JsonProperty("JsonName")] public string Name;
            [JsonProperty("Explain")] public string Explain;
        }
    }
}