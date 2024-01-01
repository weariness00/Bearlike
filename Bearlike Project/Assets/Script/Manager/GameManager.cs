using Fusion;
using Script.Data;

public class GameManager : NetworkBehaviour
{
    private UserData _userData;
    private NetworkRunner _runner;
    
    private void Start()
    {
        _userData = FindObjectOfType<UserData>();
        _runner = FindObjectOfType<NetworkRunner>();
        
        _userData.SpawnPlayers();
    }

    public override void FixedUpdateNetwork()
    {
    }
}

