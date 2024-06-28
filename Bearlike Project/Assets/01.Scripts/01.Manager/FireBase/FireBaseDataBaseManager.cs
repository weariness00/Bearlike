using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Manager;

namespace Manager.FireBase
{
    public class FireBaseDataBaseManager
    {
        public static FireBaseDataBaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FireBaseDataBaseManager();
                    _instance.Init();
                }

                return _instance;
            }
        }
        private static FireBaseDataBaseManager _instance;
        
        public static void ReadRealTimeDataBase(string referenceName, Action<DataSnapshot> readAction) => Instance.ReadRealTimeData(referenceName, readAction);
        public static void WriteRealTimeDataBase(string referenceName, Dictionary<string, object> data) => Instance.WriteRealTimeData(referenceName, data);

        public static DatabaseReference RootReference => Instance._rootReference;
        
        private FirebaseDatabase _database;
        private DatabaseReference _rootReference;
        
        private void Init()
        {
            _database = FirebaseDatabase.DefaultInstance;
            _rootReference = _database.RootReference;
        }
        
        /// <summary>
        /// 데이터 읽기
        /// </summary>
        /// <param name="referenceName"> 읽을 데이터의 이름 </param>
        /// <param name="readAction"> 데이터 읽기에 성공했을 경우 실행할 델리데이트</param>
        private void ReadRealTimeData(string referenceName, Action<DataSnapshot> readAction)
        {
            var dbReference = _database.GetReference(referenceName);
            dbReference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    DebugManager.LogError("데이터 읽기 실패");
                    return;
                }

                var snapShot = task.Result;
                readAction.Invoke(snapShot);
                DebugManager.Log("데이터 읽기 성공");
            });
        }

        /// <summary>
        /// 데이터 쓰기
        /// 하위에 데이터를 쓰는 것임으로 무조건 referenceName에 해당하는 데이터가 존재해야된다.
        /// </summary>
        /// <param name="referenceName">쓸 데이터의 상위 데이터</param>
        /// <param name="data"> referenceName밑에 새롭게 추가할 데이터</param>
        private void WriteRealTimeData(string referenceName, Dictionary<string, object> data)
        {
            var dbReference = _database.GetReference(referenceName);

            if (dbReference == null)
            {
                DebugManager.LogError($"{referenceName}라는 이름의 Reference가 존재하지 않습니다.");
                return;
            }

            dbReference.UpdateChildrenAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    DebugManager.LogError("RealTime Data 저장 실패");
                }
                else if(task.IsCompleted)
                {
                    DebugManager.Log("RealTime Data 저장 성공");
                }
            });
        }
    }

    /// <summary>
    /// 편의기능 확장
    /// </summary>
    public static class FireBaseDataBaseExtension
    {
        public static DatabaseReference GetChild(this DatabaseReference reference, string child) => reference.Child(child);

        public static void SetChild(this DatabaseReference reference, string child, object value)
        {
            reference.Child(child).SetValueAsync(value).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DebugManager.Log($"{reference}에 {child} 저장 성공");
                }
                else
                {
                    DebugManager.LogError($"{reference}에 {child} 저장 실패");
                }
            });
        }

        public static int Value(this DataSnapshot snapshot) => int.Parse(snapshot.Value.ToString());
    } 
}