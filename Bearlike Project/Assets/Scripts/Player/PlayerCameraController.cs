using Fusion;
using Script.Photon;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    private Camera _camera;
    public Vector3 offset;

    public void Awake()
    {
        NetworkManager.Instance.IsSetPlayerObjectEvent += SetPlayerCamera;
    }
    
    public void SetPlayerCamera(NetworkObject playerObject)
    {
        // Local Player에 해당하는 클라이언트인지 확인
        if (playerObject.gameObject != gameObject) return;
        
        _camera = Camera.main;
        
        _camera.transform.parent = transform;
        _camera.transform.position = transform.position + offset;
        _camera.transform.rotation = transform.rotation;
        
        _camera.transform.LookAt(transform);
    }
}