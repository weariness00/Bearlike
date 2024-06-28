using System.Collections.Generic;
using Data;
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
        public CottonBlock block;
        
        public MagicCottonInfo info;
        public Sprite icon;

        #region Info Paramater

        public string Name => info.Name;
        public int Id => info.Id;
        public StatusValue<int> Level => info.Level;
        public void LevelUp() => info.LevelUp();
        public int NeedExperience => info.NeedExperience;

        #endregion

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

