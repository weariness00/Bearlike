using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using Manager;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Util;

namespace ProjectUpdate 
{
    public class WebManager : Singleton<WebManager>
    {
        public WebServerInfo webServerInfo;
        
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

        public static void DownloadJson(string url, string fileName,  Action<string> action = null, bool isLoop = false, bool isSave = false) => Instance.StartCoroutine(Instance.DownloadJsonCoroutine(new WebDownInfo(url, fileName), action, isLoop, isSave));
        public static void DownloadJson(WebDownInfo info,  Action<string> action = null, bool isLoop = false, bool isSave = false) => Instance.StartCoroutine(Instance.DownloadJsonCoroutine(info, action, isLoop, isSave));
        
        /// <summary>
        /// 웹에서 Json 파일 다운로드
        /// </summary>
        /// <param name="webDownInfo">웹 URL과 저장할 Json의 FileName</param>
        /// <param name="isLoop">저장할 때까지 무한히 시도 할 것인지</param>
        /// <returns></returns>
        IEnumerator DownloadJsonCoroutine(WebDownInfo webDownInfo, Action<string> action = null, bool isLoop = false, bool isSave = false)
        {
            var url = webDownInfo.url;
            var fileName = webDownInfo.fileName;
            if (webServerInfo.DefaultURL.Contains($"/{url}") == false)
            {
                url = $"{webServerInfo.DefaultURL}/{url}";
            }
            
            while (true)
            {
                // 요청 보내기
                using UnityWebRequest webRequest = UnityWebRequest.Get(url);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // JSON 데이터 처리
                    string json = webRequest.downloadHandler.text;
                    action?.Invoke(json);
                    if(isSave) JsonConvertExtension.Save(json, fileName);
                    
                    DebugManager.Log($"웹에서 불러오기 요청 성공\n" +
                                     $"URL : {url}\n" +
                                     $"Download Byte : {webRequest.downloadedBytes}\n" +
                                     $"data : {json}\n" +
                                     $"Save : {isSave}");
                    yield break;
                }

                if (isLoop == false)
                {
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        DebugManager.LogError("Error: " + webRequest.error);
                    }
                    break;
                }
            }
        }

        [Serializable]
        public struct WebServerInfo
        {
            [JsonProperty("DefaultURL")]public string DefaultURL;
        }

        public struct WebDownInfo
        {
            public string url;
            public string fileName;

            public WebDownInfo(string _url, string _fileName)
            {
                url = _url;
                fileName = _fileName;
            }
        }
    }
}