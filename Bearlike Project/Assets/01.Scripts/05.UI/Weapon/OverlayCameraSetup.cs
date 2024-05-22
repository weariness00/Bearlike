using Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UI.Weapon
{
    public class OverlayCameraSetup : MonoBehaviour
    {
        private PlayerController[] _players;
        private Camera _mainCameras;
        public Camera overlayCamera;

        public Camera mainCamera;
        public Light mainLight;

        void Start()
        {
            mainCamera.gameObject.SetActive(false);
            mainLight.gameObject.SetActive(false);
            if (overlayCamera != null)
            {
                overlayCamera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
            }

            Init();

            TargetCameraAddOverlay(overlayCamera);
        }
    
        void Init()
        {
            _players = FindObjectsOfType<PlayerController>();
            foreach (var player in _players)
            {
                if (player.HasInputAuthority)
                {
                    var cameras = player.GetComponentsInChildren<Camera>();
                    foreach (var camera in cameras)
                    {
                        if (camera.name == "Camera")
                        {
                            _mainCameras = camera;
                            break;
                        }
                    }
                    break;
                }
            }
        }
    
        public void TargetCameraAddOverlay(Camera overlayCamera)
        {
            var cameraData = _mainCameras.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(overlayCamera);
        }
    }
}
