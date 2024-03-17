using System;
using System.IO;
using Manager;
using Newtonsoft.Json;

namespace Data
{
    public interface IJsonData<T>
    {
        public T GetJsonData();
        public void SetJsonData(T json);

        public static bool SaveJsonData(T json, string name, string path)
        {
            try
            {
                if (!Directory.Exists(path)){Directory.CreateDirectory(path);}
                string data = JsonConvert.SerializeObject(json);
                File.WriteAllText($"{path}/{name}", data);
                return true;
            }
            catch (Exception e)
            {
                DebugManager.LogError( "json 데이터 저장 실패\n" + e);
                return false;
            }
        }
    }
}