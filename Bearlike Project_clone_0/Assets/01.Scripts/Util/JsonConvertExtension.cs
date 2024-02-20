using System;
using System.IO;
using Script.Manager;
using UnityEngine;

namespace Util
{
    public class JsonConvertExtension
    {
        public static void Load(string fileName, Action<string> action = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var path = Application.persistentDataPath + $"/Json/{fileName}.json";
            if (File.Exists(path) == false)
            {
                DebugManager.LogWarning("존재하지 않는 Json입니다.\n" +
                                        $"파일 이름 : {fileName}\n" +
                                        $"저장 경로 : {path}\n");
                return;
            }
            var data = File.ReadAllText(path);

            action?.Invoke(data);
            
            DebugManager.Log("Json 데이터 불러오기 성공\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"저장 경로 : {path}\n" +
                             $"데이터 : {data}\n");
        }

        public static void Save(string data, string fileName, Action doneAction = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            var path = Application.persistentDataPath + $"/Json/{fileName}.json";
            File.WriteAllText(path, data);
            
            doneAction?.Invoke();
            
            DebugManager.Log("Json 데이터 저장 성공\n" +
                             $"파일 이름 : {fileName}\n" +
                             $"저장 경로 : {path}\n" +
                             $"데이터 : {data}\n");
        }
    }
}