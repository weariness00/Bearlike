using System.Collections;
using Data;
using Fusion;
using Loading;
using Photon;
using UnityEngine;

namespace GamePlay.UI
{
    public class GameProgressCanvas : NetworkBehaviourEx
    {
        [SerializeField] private NetworkObject progressBlockParent;
        [SerializeField] private NetworkPrefabRef playerProgressBlockRef;

        public override void Spawned()
        {
            LoadingManager.AddWait();
            
            base.Spawned();
            progressBlockParent.gameObject.SetActive(false);
            
            StartCoroutine(InitProgressCoroutine());
        }

        private IEnumerator InitProgressCoroutine()
        {
            if (HasStateAuthority)
            {
                while (UserData.Instance.IsSpawnPlayer == false) yield return null;
                
                foreach (var data in UserData.GetAllUserData())
                {
                    SpawnBlockRPC(data.PlayerRef, data.NetworkId);
                }
            }
            
            progressBlockParent.gameObject.SetActive(true);
            gameObject.SetActive(false);
            LoadingManager.EndWait();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void SpawnBlockRPC(PlayerRef playerRef, NetworkId playerId)
        {
            var obj = await Runner.SpawnAsync(playerProgressBlockRef, null, null, playerRef, (runner, o) =>
            {
                var block = o.GetComponent<PlayerProgressBlock>();
                block.PlayerId = playerId;
                block.ParentId = progressBlockParent.Id;
            });
        }
    }
}