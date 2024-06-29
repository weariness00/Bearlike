using System;
using System.Collections.Generic;
using Data;
using Manager.FireBase;
using Player;
using Status;
using UI.User;
using UnityEngine;
using UnityEngine.Serialization;

namespace User.MagicCotton
{
    /// <summary>
    /// 일명 : 마법이 각인된 솜
    /// 게임 시작전에 적용되는 능력들
    /// </summary>
    public abstract class MagicCottonBase : MonoBehaviour
    {
        #region Static

        // Info Data 캐싱
        private static Dictionary<int, MagicCottonInfoJsonData> _infoDataCash = new Dictionary<int, MagicCottonInfoJsonData>();
        public static void AddInfoData(int id, MagicCottonInfoJsonData data) => _infoDataCash.TryAdd(id, data);
        public static MagicCottonInfoJsonData GetInfoData(int id) => _infoDataCash.TryGetValue(id, out var data) ? data : new MagicCottonInfoJsonData();
        public static void ClearInfosData() => _infoDataCash.Clear();

        #endregion
        
        public CottonBlock block;
        
        public MagicCottonInfo info;
        public Sprite icon;

        #region Info Paramater

        public string Name => info.Name;
        public int Id => info.Id;
        public StatusValue<int> Level => info.Level;
        public void LevelUp() => info.LevelUp();
        public void SetLevel(int level) => info.Level.Current = level;
        public int NeedExperience => info.NeedExperience;

        #endregion

        public virtual void Awake()
        {
            info.SetJsonData(GetInfoData(Id));
            
            block.SetMaxLevel(info.Level.Max);
        }

        public abstract void Apply(GameObject applyObj);
    }
    
    [System.Serializable]
    public struct MagicCottonInfo : IJsonData<MagicCottonInfoJsonData>
    {
        public int Id;
        public string Name;
        public StatusValue<int> Level;
        public int[] ExperienceArray; // 필요 경험치

        public int NeedExperience => Level.isMax ? -1 :  ExperienceArray[Level.Current];

        public void LevelUp()
        {
            if (Level.isMax) return;
            ++Level.Current;
            var id = Id.ToString();
            var l = Level.Current;
            FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}/MagicCottonContainer").SnapShot(snapshot =>
            {
                snapshot.Reference.SetChild(id, l);
            });
        }

        public MagicCottonInfoJsonData GetJsonData()
        {
            return new MagicCottonInfoJsonData();
        }

        public void SetJsonData(MagicCottonInfoJsonData json)
        {
            Id = json.id;
            Name = json.name;
            Level ??= new StatusValue<int>();
            Level.Max = json.maxLevel;
            ExperienceArray = json.GetNeedCoinArray();
        }
    }
}

