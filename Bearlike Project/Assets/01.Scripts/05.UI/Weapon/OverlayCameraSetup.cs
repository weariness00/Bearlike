using Player;
using UnityEngine;
using Weapon.Gun;
using Weapon.Gun.Container;
using Weapon.Gun.Continer;

namespace UI.Weapon
{
    public class OverlayCameraSetup : MonoBehaviour
    {
        private PlayerController[] _players;
        private Camera _mainCameras;
        public Camera overlayCamera;

        public Camera mainCamera;
        public Light mainLight;

        public GameObject[] weapons;

        void Start()
        {
            mainCamera.gameObject.SetActive(false);
            mainLight.gameObject.SetActive(false);
            if (overlayCamera != null)
            {
                overlayCamera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
            }

            Init();
        }
    
        void Init()
        {
            _players = FindObjectsOfType<PlayerController>();
            foreach (var player in _players)
            {
                if (player.HasInputAuthority)
                {
                    player.cameraController.TargetCameraAddOverlay(overlayCamera);
                    if (player.weaponSystem.TryGetEquipGun(out GunBase gun))
                    {
                        if (gun is Shotgun)
                            ChangeWeapon(0);
                        else if(gun is HunterSniper)
                            ChangeWeapon(1);
                        else if(gun is Revolver)
                            ChangeWeapon(2);
                    }
                    break;
                }
            }
        }

        public void ChangeWeapon(int type)
        {
            for (int i = 0; i < 3; ++i)
            {
                if(i == type)
                    weapons[i].SetActive(true);
                else
                    weapons[i].SetActive(false);
            }
        }
    }
}
