using System;
using System.IO;
using System.Security.Cryptography;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
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

        public void UploadFile(string bucketName, string filePath, string objectName)
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            var request = _storageService.Objects.Insert(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = objectName
            }, bucketName, stream, "application/octet-stream");
            request.Upload();
        }

        public static void DownloadFile(string bucketName, string objectName, string destinationPath)
        {
            var fileName = Path.Combine(destinationPath, objectName);
            var request = Instance._storageService.Objects.Get(bucketName, objectName);
            if (Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            if(File.Exists(fileName) == false) {return;}
            if (IsFileVersionDifferent(request.Execute(), fileName)) { return; }
            
            using var stream = new FileStream(fileName, FileMode.Create);
            request.Download(stream);
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