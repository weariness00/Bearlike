using System.Collections.Generic;
using Player;
using Status;
using UI.User;
using UnityEngine;

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
        public void LevelUp(int exp) => info.LevelUp(exp);
        public int NeedExperience => info.NeedExperience;

        #endregion

        public abstract void Apply(PlayerController playerController);

    }
    
    [System.Serializable]
    public struct MagicCottonInfo
    {
        public int Id;
        public string Name;
        public StatusValue<int> Level;
        public List<int> Experience; // 필요 경험치

        public int NeedExperience => Level.isMax ? -1 :  Experience[Level.Current];

        public void LevelUp(int exp)
        {
            if(Level.isMax) return;

            if (Experience[Level.Current] <= exp)
            {
                ++Level.Current;
            }
        }
    }
}

