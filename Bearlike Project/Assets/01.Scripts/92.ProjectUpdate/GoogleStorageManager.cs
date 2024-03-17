using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Manager;
using UnityEngine;
using Util;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace ProjectUpdate
{
    public class GoogleStorageManager : Singleton<GoogleStorageManager>
    {
        private StorageService _storageService;
        [SerializeField] private string _jsonKeyFilePath;

        protected override void Awake()
        {
            base.Awake();
            var credential = GoogleCredential.FromFile($"{Application.dataPath}/{_jsonKeyFilePath}").CreateScoped(StorageService.Scope.CloudPlatform);
            _storageService = new StorageService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Bearlike"
            });

        }
        
        public static void SetTemporaryEnvironmentVariable(string key, string value)
        {
            // 환경 변수 설정
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        }

        public static async Task<bool> IsConnect(string bucketName)
        {
            try
            {
                // StorageService 객체와 bucketName을 사용하여 버킷의 메타데이터 요청
                var bucket = await Instance._storageService.Buckets.Get(bucketName).ExecuteAsync();
        
                // 요청이 성공적으로 반환되면, 연결 및 인증이 성공적임을 의미
                return bucket != null;
            }
            catch (Exception e)
            {
                // 요청 중 에러 발생 (인증 실패, 연결 문제 등) 시
                DebugManager.LogError($"구글 스토리지에 연결을 실패했습니다.\n" +
                                      $"Bucket Name : {bucketName}\n" +
                                      $"Error : {e}");
                return false;
            }
        }
        
        public static bool UploadFile(string bucketName, string filePath, string objectName, string contentType = "application/octet-stream")
        {
            filePath = Path.Combine(filePath, objectName);
            using var stream = new FileStream(filePath, FileMode.Open);
            var request = Instance._storageService.Objects.Insert(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = objectName
            }, bucketName, stream, contentType);
            try
            {
                request.Upload();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static bool UploadJsonFile(string bucketName, string filePath, string objectName) => UploadFile(bucketName, filePath, objectName, "application/json");

        public static async Task<bool> DownloadFile(string bucketName, string objectName, string destinationPath)
        {
            if (await IsConnect(bucketName) == false)
            {
                return false;
            }
            
            var fileName = Path.Combine(destinationPath, objectName);
            var request = Instance._storageService.Objects.Get(bucketName, objectName);
            if (Directory.Exists(destinationPath) == false)
            {
                Directory.CreateDirectory(destinationPath);
            }
            if (File.Exists(fileName) && IsFileVersionDifferent(await request.ExecuteAsync(), fileName) == false) { return true; }
            
            await using var stream = new FileStream(fileName, FileMode.OpenOrCreate);
            request.Download(stream);
            return true;
        }

        public static string[] FileListFromBucket(string bucketName)
        {
            var request = Instance._storageService.Objects.List(bucketName);
            var fileList = new List<string>();
            
            do
            {
                var result = request.Execute();
                fileList.AddRange(result.Items.Select(item => item.Name));

                // 더 많은 결과가 있는 경우 페이지를 계속해서 가져옴
                request.PageToken = result.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));

            return fileList.ToArray();
        }

        public static bool IsFileVersionDifferent(Object objectData, string localFilePath)
        {
            // 객체의 세대 또는 ETag 가져오기
            var remoteGeneration = objectData.Generation;
            var remoteEtag = objectData.ETag;

            // 로컬 파일의 세대 또는 ETag 가져오기
            var localGeneration = GetLocalFileGeneration(localFilePath);
            var localEtag = GetLocalFileEtag(localFilePath);

            // 세대 또는 ETag를 비교하여 다른지 확인
            return remoteGeneration != localGeneration || remoteEtag != localEtag;
        }

        public static bool IsFileVersionDifferent(string bucketName, string objectName, string destinationPath)
        {
            var fileName = Path.Combine(destinationPath, objectName);
            var request = Instance._storageService.Objects.Get(bucketName, objectName);
            if (Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            if(File.Exists(fileName) == false) {return false;}

            return IsFileVersionDifferent(request.Execute(), fileName);
        }

        private static long GetLocalFileGeneration(string localFilePath)
        {
            // 로컬 파일의 세대를 가져오는 로직을 구현
            // 예시: 파일의 마지막 수정 시간을 사용
            var fileInfo = new FileInfo(localFilePath);
            return fileInfo.LastWriteTime.Ticks;
        }

        private static string GetLocalFileEtag(string localFilePath)
        {
            // 로컬 파일의 ETag를 가져오는 로직을 구현
            // 예시: 파일 내용을 해시하여 MD5로 계산
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(localFilePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}