using System;
using System.IO;
using UnityEngine;

namespace Util
{
    public class JsonConvertExtension
    {
        public static void Load(string fileName, Action<string> action)
        {
            var path = Application.dataPath + $"/Json/KeyManager/{fileName}.json";
            if (File.Exists(path) == false) return;
            var data = File.ReadAllText(path);

            action?.Invoke(data);
        }
    }
}