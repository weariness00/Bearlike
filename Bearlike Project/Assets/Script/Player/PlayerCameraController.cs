using System;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    private Camera _camera;

    public Vector3 offset;

    private void Start()
    {
        _camera = Camera.main;
        Init();
    }
    
    void Init()
    {
        _camera.transform.parent = transform;
        _camera.transform.position = transform.position + offset;
        _camera.transform.rotation = transform.rotation;
        
        _camera.transform.LookAt(transform);
    }
    
    
}

