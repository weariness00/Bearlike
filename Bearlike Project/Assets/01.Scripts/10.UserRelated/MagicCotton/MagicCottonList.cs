using System.Collections.Generic;
using Fusion;
using Photon;
using UnityEngine;
using Util;

namespace User.MagicCotton
{
    public class MagicCottonList : Singleton<MagicCottonList>
    {
        public NetworkPrefabRef applyStatusRef;
        
        [SerializeField] private List<MagicCottonBase> _magicCottonList = new List<MagicCottonBase>();

        public void SetList(List<MagicCottonBase> list) => _magicCottonList = list;
    }
}