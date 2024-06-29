using System;
using System.Collections.Generic;
using Fusion;
using Manager;
using Manager.FireBase;
using Photon;
using UnityEngine;
using Util;

namespace User.MagicCotton
{
    public class MagicCottonList : Singleton<MagicCottonList>
    {
        public NetworkPrefabRef applyStatusRef;

        [SerializeField] private List<MagicCottonBase> _magicCottonList = new List<MagicCottonBase>();

        
        private void OnEnable()
        {
            Load();
        }

        public void SetList(List<MagicCottonBase> list) => _magicCottonList = list;

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
                
                DebugManager.ToDo("이중 for문 사용중 나중에 바꿀 수 있으면 바꾸기");
                var mcContainer = snapshot.Child("MagicCottonContainer");
                foreach (var snapshotChild in mcContainer.Children)
                {
                    var id = snapshotChild.Key();
                    var level = snapshotChild.Value();
                    
                    foreach (var mc in _magicCottonList)
                    {
                        if (mc.Id == id)
                        {
                            mc.Level.Current = level;
                            mc.block.SetCurrentLevel(level);
                        }
                    }
                }

                _isLoad = false;
            });
        }
    }
}