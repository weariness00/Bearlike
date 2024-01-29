using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Script.Manager;

namespace ProjectUpdate
{
    public class WebDownLoader
    {
        public static async Task<byte[]> DownloadByteAsync(string url)
        {
            HttpClient client = new HttpClient();
            try
            {
                byte[] data = await client.GetByteArrayAsync(url);
                return data;
            }
            catch (Exception e)
            {
                DebugManager.LogError("URL을 통한 데이터 불러오기 실패" + e.Message);
                return null;
            }
        }

        public static string ConvertJsonFromByte(byte[] data)
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return json;
        }

        public static async Task<T> DownLoadJson<T>(string url)
        {
            var data = await DownloadByteAsync(url);
            var json = ConvertJsonFromByte(data);
            T value = JsonConvert.DeserializeObject<T>(json);
            return value;
        }
    }
}