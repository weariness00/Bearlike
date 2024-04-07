using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    public class PlayerCameraController : NetworkBehaviour
    {
        public GameObject ownerObject;

        [Header("카메라")] public Camera targetCamera;
        public Camera weaponCamera;
        public Vector3 offset;

        public override void Spawned()
        {
            if (HasInputAuthority == false)
            {
                targetCamera.GetComponent<AudioListener>().enabled = false;
                targetCamera.enabled = false;
                weaponCamera.enabled = false;

                return;
            }

            targetCamera.tag = "MainCamera";

            SetPlayerCamera();
            WeaponClipping();
        }

        public void SetPlayerCamera()
        {
            if(ownerObject == null) return;
            
            Transform targetCameraTransform = targetCamera.transform;
            Transform ownerTransform = ownerObject.transform;
            targetCameraTransform.SetParent(ownerTransform);
            targetCameraTransform.position = ownerTransform.position + offset;
            targetCameraTransform.rotation = ownerTransform.rotation;
        }

        public void WeaponClipping()
        {
            var cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            cameraData.cameraStack.Add(weaponCamera);
        }
    }
}