using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Photon.Realtime;
using Script.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TsetServer : MonoBehaviour
{
    private NetworkRunner _runner;
    public SceneReference s;
    private void Start()
    {
        Matching(GameMode.Host, "aa");  
    }
    
    void Matching(GameMode mode, string sessionName)
    {
        // Create the Fusion runner and let it know that we will be providing user inpuz

        gameObject.GetOrAddComponent<RunnerSimulatePhysics3D>();
        _runner = gameObject.GetOrAddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
            
        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromPath(s.ScenePath);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name   
        _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = scene,
            MatchmakingMode = MatchmakingMode.FillRoom,
            SceneManager = gameObject.GetOrAddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 3,
            IsVisible = true,
            IsOpen = true,
        });
    }
}
