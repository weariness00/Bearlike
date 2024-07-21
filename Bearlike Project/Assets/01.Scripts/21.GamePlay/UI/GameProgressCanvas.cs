using System.Collections;
using System.Linq;
using Data;
using Fusion;
using Loading;
using Photon;
using Player;
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
            
            StartCoroutine(InitProgressCoroutine());
        }

        private IEnumerator InitProgressCoroutine()
        {
            PlayerController[] players;
            var playerCount = Runner.ActivePlayers.ToArray().Length;
            while (true)
            {
                players = FindObjectsOfType<PlayerController>();
                if (players.Length == playerCount) break;
                yield return null;
            }

            while (true)
            {
                bool isPCSpawn = true;
                foreach (var pc in players)
                    if (!pc.IsSpawnSuccess) isPCSpawn = false;

                if (isPCSpawn) break;
                yield return null;
            }
            
            if (HasStateAuthority)
            {
                foreach (var data in UserData.GetAllUserData())
                {
                    // SpawnBlockRPC(data.PlayerRef, data.NetworkId);
                    
                    Runner.SpawnAsync(playerProgressBlockRef, null, null, data.PlayerRef, (runner, o) =>
                    {
                        var block = o.GetComponent<PlayerProgressBlock>();
                        block.PlayerId = data.NetworkId;
                        block.ParentId = progressBlockParent.Id;
                    });
                }
            }

            progressBlockParent.gameObject.SetActive(true);
            gameObject.SetActive(false);
            LoadingManager.EndWait();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void SpawnBlockRPC(PlayerRef playerRef, NetworkId playerId)
        {
            var obj = Runner.SpawnAsync(playerProgressBlockRef, null, null, playerRef, (runner, o) =>
            {
                var block = o.GetComponent<PlayerProgressBlock>();
                block.PlayerId = playerId;
                block.ParentId = progressBlockParent.Id;
            });
        }
    }
}