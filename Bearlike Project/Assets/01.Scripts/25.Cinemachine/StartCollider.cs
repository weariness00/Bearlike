using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class StartCollider : NetworkBehaviourEx
{
    [SerializeField] private TimelineAsset[] timelineAssets;
    private PlayableDirector pd;
    
    private GameObject[] _players; 
    
    private void Start()
    {
        pd = GetComponent<PlayableDirector>();
        
        // InGame Player 대입
        List<GameObject> playerObjects = new List<GameObject>();
        foreach (var playerRef in Runner.ActivePlayers.ToArray())
        {
            playerObjects.Add(Runner.GetPlayerObject(playerRef).gameObject);
        }
        _players = playerObjects.ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OffGameobjectRPC();
            ActivePlayersRPC(false);
            PlayCutSceneRPC(0);
        }
    }

    #region RPC Function

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void OffGameobjectRPC()
    {
        gameObject.SetActive(false);
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ActivePlayersRPC(bool value)
    {
        foreach (var player in _players)
        {
            player.SetActive(value);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void PlayCutSceneRPC(int index)
    {
        pd.Play(timelineAssets[index]);
    }

    #endregion
}
