using System;
using System.IO;
using System.Linq;
using GamePlay;
using GamePlay.Stage;
using Item;
using Item.Looting;
using Loading;
using Manager;
using Monster;
using Newtonsoft.Json;
using Script.Data;
using Skill;
using Status;
using UnityEngine;
using User;
using User.MagicCotton;
using Util;
using Weapon.Gun;

namespace ProjectUpdate
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.LobbySceneEnd)]
    public class ProjectUpdateManager : Singleton<ProjectUpdateManager>
    {
        private readonly string _json = "bearlike-json";

        public TextAsset serverInfoJson;
        
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

        public void Init()
        {
            LoadingManager.AddWait();
            // DownLoadJsonToStorage(serverInfo); // 스토리지에서 웹 서버 정보 가져오기
            JsonConvertExtension.Save(serverInfoJson.text, serverInfoJson.name);
            
            LoadingManager.AddWait();
            JsonConvertExtension.Load(serverInfoJson.name, (data) =>
            {
                WebManager.Instance.webServerInfo = JsonConvert.DeserializeObject<WebManager.WebServerInfo>(data);
                LoadingManager.EndWait("서버 정보 불러오기 성공");
            });
            
            WebManager.DownloadJson("KeySetting/Default", "DefaultKeyData", json =>
            {
                KeyManager.Instance.DefaultLoad();
            }, true, true);
                  
            // Difficult 업데이트
            Difficult.ClearSDifficultData();
            WebRequestJson("Difficult", "Difficult", json =>
            {
                var data = JsonConvert.DeserializeObject<StatusJsonData[]>(json);
                foreach (var difficultData in data)
                {
                    Difficult.AddDifficultData(difficultData.Name.ToLower(), difficultData);
                }
                
                LoadingManager.EndWait("난이도 불러오기 성공");
            }, true, true);
            
            // Skill 업데이트
            SkillBase.ClearInfosData();
            SkillBase.ClearStatusData();
            WebRequestJson("Skill", "Skill", json =>
            {
                var data = JsonConvert.DeserializeObject<SkillJsonData[]>(json);
                foreach (var skillJsonData in data)
                {
                    SkillBase.AddInfoData(skillJsonData.ID, skillJsonData);
                }
                
                LoadingManager.EndWait("스킬 불러오기 성공");
            }, true, true);
            WebRequestJson("Skill/Status", "SkillStatus", json =>
            {
                var data = JsonConvert.DeserializeObject<StatusJsonData[]>(json);
                foreach (var statusJsonData in data)
                {
                    SkillBase.AddStatusData(statusJsonData.ID, statusJsonData);
                }
                
                LoadingManager.EndWait("스킬 정보 (Status) 불러오기 성공");
            }, true, true);
            
            // Item 업데이트
            ItemBase.ClearInfosData();
            ItemBase.ClearStatusData();
            WebRequestJson("Item", "Item", json =>
            {
                var data = JsonConvert.DeserializeObject<ItemJsonData[]>(json);
                foreach (var itemJsonData in data)
                {
                    ItemBase.AddInfoData(itemJsonData.id, itemJsonData);
                }
                LoadingManager.EndWait("아이템 정보 불러오기 성공");
            }, true, true);
            WebRequestJson("Item/Status", "ItemStatus", json =>
            {
                var data = JsonConvert.DeserializeObject<StatusJsonData[]>(json);
                foreach (var itemStatusJsonData in data)
                {
                    ItemBase.AddStatusData(itemStatusJsonData.ID, itemStatusJsonData);
                }
                LoadingManager.EndWait("아이템 정보 (Status) 불러오기 성공");
            }, true, true);
            
            // Monster 업데이트
            MonsterBase.ClearInfosData();
            MonsterBase.ClearStatusData();
            MonsterBase.ClearLootingData();
            WebRequestJson("Monster", "Monster", json =>
            {
                var data = JsonConvert.DeserializeObject<MonsterJsonData[]>(json);
                foreach (var monsterJsonData in data)
                {
                    MonsterBase.AddInfoData(monsterJsonData.ID, monsterJsonData);
                }
                LoadingManager.EndWait("몬스터 정보 불러오기 성공");
            }, true, true);
            WebRequestJson("Monster/Status", "MonsterStatus", json =>
            {
                var data = JsonConvert.DeserializeObject<StatusJsonData[]>(json);
                foreach (var statusJsonData in data)
                {
                    MonsterBase.AddStatusData(statusJsonData.ID, statusJsonData);
                }
                LoadingManager.EndWait("몬스터 정보 (Status) 불러오기 성공");
            }, true, true);
            WebRequestJson("Monster/LootingTable", "MonsterLootingTable", json =>
            {
                var data = JsonConvert.DeserializeObject<LootingJsonData[]>(json);
                foreach (var lootingData in data)
                {
                    MonsterBase.AddLootingData(lootingData.TargetID, lootingData);
                }
                LoadingManager.EndWait("몬스터 아이템 드랍율 불러오기 성공");
            }, true, true);
            
            // Weapon
            GunBase.ClearInfosData();
            GunBase.ClearStatusData();
            WebRequestJson("Gun", "Gun", json =>
            {
                var data = JsonConvert.DeserializeObject<GunJsonData[]>(json);
                foreach (var gunData in data)
                {
                    GunBase.AddInfoData(gunData.Id, gunData);
                }
                LoadingManager.EndWait("총 정보 불러오기 성공");
            }, true, true);
            WebRequestJson("Gun/Status", "GunStatus", json =>
            {
                var data = JsonConvert.DeserializeObject<StatusJsonData[]>(json);
                foreach (var statusJsonData in data)
                {
                    GunBase.AddStatusData(statusJsonData.ID, statusJsonData);
                }
                LoadingManager.EndWait("총 정보 (Status) 불러오기 성공");
            }, true, true);
            
            // Treasure Box 업데이트
            TreasureBox.ClearTreasureBoxData();
            WebRequestJson("TreasureBox", "TreasureBox", json =>
            {
                var data = JsonConvert.DeserializeObject<TreasureBoxJsonData[]>(json);
                foreach (var treasureBoxJsonData in data)
                {
                    TreasureBox.AddTreasureBoxData(treasureBoxJsonData.ID, treasureBoxJsonData);
                }
                
                LoadingManager.EndWait("보물 상자 정보 불러오기 성공");
            }, true, true);
            
            // Stage 업데이트
            StageBase.ClearInfosData();
            StageBase.ClearLootingData();
            WebRequestJson("Stage", "Stage", json =>
            {
                var data = JsonConvert.DeserializeObject<StageJsonData[]>(json);
                foreach (var stageInfo in data)
                {
                    StageBase.AddInfoData(stageInfo.id, stageInfo);
                }
                LoadingManager.EndWait("스테이지 정보 불러오기 성공");
            }, true, true);
            WebRequestJson("Stage/LootingTable", "StageLootingTable", json =>
            {
                var data = JsonConvert.DeserializeObject<LootingJsonData[]>(json);
                foreach (var lootingData in data)
                {
                    StageBase.AddLootingData(lootingData.TargetID, lootingData);
                }
                LoadingManager.EndWait("스테이지 드랍률 불러오기 성공");
            }, true, true);
            
            MagicCottonBase.ClearInfosData();
            WebRequestJson("MagicCotton","MagicCotton", json =>
            {
                var data = JsonConvert.DeserializeObject<MagicCottonInfoJsonData[]>(json);
                foreach (var infoData in data)
                {
                    MagicCottonBase.AddInfoData(infoData.id, infoData);
                }
                
                LoadingManager.EndWait("마법의 천 불러오기 성공");
            });
            
            LoadingManager.EndWait();
        }

        public struct DownloadInfo
        {
            [JsonProperty("URL")] public string URL;
            [JsonProperty("JsonName")] public string Name;
            [JsonProperty("Explain")] public string Explain;
        }

        private void WebRequestJson(string url, string fileName, Action<string> action = null, bool isLoop = false, bool isSave = false)
        {
            LoadingManager.AddWait();
            // json의 version 정보 가져오기
            WebManager.DownloadJson($"{url}/Version", $"{fileName}_Version", nowVersionJson =>
            {
                // 컴퓨터에 저장된 Version 데이터 가져오기
                var hasVersion = JsonConvertExtension.Load($"{fileName}_Version", prevVersionJson =>
                {
                    TableVersion nowVersionData = JsonConvert.DeserializeObject<TableVersion[]>(nowVersionJson).First();
                    TableVersion prevVersionData;
                    try
                    {
                        prevVersionData = JsonConvert.DeserializeObject<TableVersion[]>(prevVersionJson).First();
                    }
                    catch (Exception e)
                    {
                        prevVersionData.UnixTime = 0;
                    }

                    // Version이 최신이 아니면 다운로드
                    if (nowVersionData.UnixTime > prevVersionData.UnixTime)
                    {
                        WebManager.DownloadJson(url, fileName, action, isLoop, isSave);
                    }
                    // 버전이 최신이라면 load
                    else
                    {
                        if (JsonConvertExtension.Load(fileName, action) == false)
                        {
                            WebManager.DownloadJson(url, fileName, action, isLoop, isSave);
                        }
                    }
                });
                // 버전이 없다면 다운로드
                if (hasVersion == false)
                {
                    WebManager.DownloadJson(url, fileName, action, isLoop, isSave);
                }
            }, true, true);
        }

        #region Struct

        private struct TableVersion
        {
            [JsonProperty("Table Name")]public string TableName;
            [JsonProperty("Time")] public string ZTimeData;
            [JsonProperty("Second")]public ulong UnixTime;
        }

        #endregion
    }
}