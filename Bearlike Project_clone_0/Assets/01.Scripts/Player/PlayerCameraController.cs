using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    public class PlayerCameraController : NetworkBehaviour
    {
        public Camera targetCamera;
        public Camera weaponCamera;
        public Vector3 offset;

        public override void Spawned()
        {
            if (HasInputAuthority == false)
            {
                Destroy(targetCamera.gameObject);
                Destroy(weaponCamera.gameObject);
                return;
            }
            
            if(Camera.main)
            {
                Destroy(Camera.main.gameObject);
            }

            targetCamera.tag = "MainCamera";
            
            SetPlayerCamera();
            WeaponClipping();
        }

        public void SetPlayerCamera()
        {
            // Local Player에 해당하는 클라이언트인지 확인
            targetCamera.transform.parent = transform;
            targetCamera.transform.position = transform.position + offset;
            targetCamera.transform.rotation = transform.rotation;
        
            targetCamera.transform.LookAt(transform.forward + offset);
        }

        public void WeaponClipping()
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(weaponCamera);
        }
    }
}