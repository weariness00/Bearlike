using System;
using Firebase.Database;
using Firebase.Extensions;

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
        
        public static DatabaseReference RootReference => Instance._rootReference;
        
        private FirebaseDatabase _database;
        private DatabaseReference _rootReference;
        
        private void Init()
        {
            _database = FirebaseDatabase.DefaultInstance;
            _rootReference = _database.RootReference;
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

        public static void SnapShot(this DatabaseReference reference, Action<DataSnapshot> readAction)
        {
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    DebugManager.LogError("데이터 읽기 실패");
                    return;
                }

                var snapShot = task.Result;
                if (snapShot.Exists == false)
                {
                    DebugManager.LogError($"{reference}의 SnapShot이 존재하지 않습니다.");
                    return;
                }
                
                readAction?.Invoke(snapShot);
                DebugManager.Log("데이터 읽기 성공");
            });
        }

        public static int Key(this DataSnapshot snapshot) => int.Parse(snapshot.Key);
        public static int Value(this DataSnapshot snapshot) => int.Parse(snapshot.Value.ToString());
    } 
}