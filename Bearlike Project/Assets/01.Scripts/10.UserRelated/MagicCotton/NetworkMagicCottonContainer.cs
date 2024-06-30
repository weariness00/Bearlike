using System.Collections.Generic;
using Fusion;
using Loading;
using Manager.FireBase;
using Photon;
using UnityEngine;
using User.MagicCotton;

namespace UserRelated.MagicCotton
{
    public class NetworkMagicCottonContainer : NetworkBehaviourEx
    {
        [SerializeField] private List<MagicCottonBase> magicCottonList;
        
        private bool _isLoad = false;
        
        public void SetList(List<MagicCottonBase> list) => magicCottonList = list;
        public List<MagicCottonBase> GetList() => magicCottonList;

        public override void Spawned()
        {
            base.Spawned();

            if (HasInputAuthority)
            {
                LoadRPC(FireBaseAuthManager.UserId);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void LoadRPC(string userId)
        {
            if(_isLoad) return;
            _isLoad = true;
            LoadingManager.AddWait();
            FireBaseDataBaseManager.RootReference.GetChild($"UserData/{userId}").SnapShot(snapshot =>
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
                }

                if(HasInputAuthority) ApplyRPC(Object.InputAuthority);
                _isLoad = false;
                
                LoadingManager.EndWait();
            });
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void ApplyRPC(PlayerRef pRef)
        {
            var pNetObj = Runner.GetPlayerObject(pRef);
            var pObj = pNetObj.gameObject;
            
            foreach (var mc in magicCottonList)
            {
                mc.Apply(pObj);
            }
        }
    }
}

