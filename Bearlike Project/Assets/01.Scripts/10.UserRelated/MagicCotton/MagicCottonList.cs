using System;
using System.Collections.Generic;
using Fusion;
using Manager;
using Manager.FireBase;
using Photon;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace User.MagicCotton
{
    public class MagicCottonList : Singleton<MagicCottonList>
    {
        [SerializeField] private List<MagicCottonBase> magicCottonList = new List<MagicCottonBase>();
        
        private void OnEnable()
        {
            Load();
        }

        public void SetList(List<MagicCottonBase> list) => magicCottonList = list;
        public List<MagicCottonBase> GetList() => magicCottonList;
        
        private bool _isLoad = false;
        public void Load()
        {
            if(_isLoad) return;
            _isLoad = true;
            FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}").SnapShot(snapshot =>
            {
                if (snapshot.HasChild("MagicCottonContainer") == false)
                {
                    snapshot.Reference.SetChild("MagicCottonContainer", true);
                }
                
                var mcDict = new Dictionary<int, MagicCottonBase>();
                foreach (var mc in magicCottonList)
                    mcDict.Add(mc.Id, mc);
                
                var mcContainer = snapshot.Child("MagicCottonContainer");
                foreach (var snapshotChild in mcContainer.Children)
                {
                    var id = snapshotChild.Key();
                    var level = snapshotChild.Value();

                    var mc = mcDict[id];
                    mc.Level.Current = level;
                    mc.block.SetCurrentLevel(level);
                }

                _isLoad = false;
            });
        }
    }
}