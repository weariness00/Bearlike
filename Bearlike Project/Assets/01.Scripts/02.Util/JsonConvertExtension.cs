using System;
using System.Collections;
using System.IO;
using Manager;
using UnityEngine;

namespace Util
{
    public class JsonConvertExtension
    {
        public enum JsonDataType
        {
            StreamingAssetsData,
            PersistentData,
        }
        
        public static bool Load(string fileName, Action<string> action = null, JsonDataType dataType = JsonDataType.PersistentData)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string path = "";
            switch (dataType)
            {
                case JsonDataType.PersistentData:
                    path = Application.persistentDataPath + $"/Json/{fileName}.json";
                    break;
                case JsonDataType.StreamingAssetsData:
                    path = Application.streamingAssetsPath + $"/Json/{fileName}.json";
                    break;
            }
            
            if (File.Exists(path) == false)
            {
                DebugManager.LogWarning("존재하지 않는 Json입니다.\n" +
                                        $"파일 이름 : {fileName}\n" +
                                        $"저장 경로 : {path}\n");
                return false;
            }
            var data = File.ReadAllText(path);

            action?.Invoke(data);
            
            DebugManager.Log("Json 데이터 불러오기 성공\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"저장 경로 : {path}\n" +
                             $"데이터 : {data}\n");

            return true;
        }

        public static IEnumerator LoadCoroutine(string fileName, Action<string> action = null, bool isReLoad = false)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var path = Application.persistentDataPath + $"/Json/{fileName}.json";
            float time = 0f;
            
            string data;
            while (true)
            {
                yield return null;
                time += Time.deltaTime;
                if (File.Exists(path) == false)
                {
                    continue;
                }
                data = File.ReadAllText(path);
                try
                {
                    action?.Invoke(data);
                }
                catch (Exception e)
                {
                    DebugManager.LogError("Json 데이터 불러오기 실패\n" +
                                          $"파일 이름 : {fileName}\n" +
                                          $"저장 경로 : {path}\n" +
                                          $"걸린 시간 : {time}\n" +
                                          e);
                    yield break;
                }
                break;
            }

            DebugManager.Log("Json 데이터 불러오기 성공\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"저장 경로 : {path}\n" +
                             $"데이터 : {data}\n" +
                             $"걸린 시간 : {time}");
            
        }

        public static void Save(string data, string fileName, Action doneAction = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var path = Application.persistentDataPath + $"/Json/{fileName}.json";
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory) == false) Directory.CreateDirectory(directory);
            File.WriteAllText(path, data);
            
            doneAction?.Invoke();
            
            DebugManager.Log("Json 데이터 저장 성공\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"저장 경로 : {path}\n" +
                             $"데이터 : {data}\n");
        }
    }
}