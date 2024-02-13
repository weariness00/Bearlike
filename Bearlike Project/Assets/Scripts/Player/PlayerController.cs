using System;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Item;
using Script.Data;
using Script.Manager;
using Script.Photon;
using Script.Weapon.Gun;
using Scripts.State.GameStatus;
using State.StateClass;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Player
{
    public class PlayerController : NetworkBehaviour
    {
        // public Status status;
        public PlayerStatus status;

        public IEquipment equipment;
        public StatusValue<int> ammo = new StatusValue<int>();

        [Header("아이템")] 
        public Dictionary<int, ItemBase> itemList = new Dictionary<int, ItemBase>();
        
        [HideInInspector] public SimpleKCC simpleKcc;
        private NetworkMecanimAnimator _networkAnimator;
        private void Awake()
        {
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            equipment = GetComponentInChildren<IEquipment>();
            status = gameObject.GetOrAddComponent<PlayerStatus>();
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
        }

        public override void Spawned()
        {
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            if (HasInputAuthority)
            {
                name = "Local Player";

                Runner.SetPlayerObject(Runner.LocalPlayer, Object);
                
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
                DebugManager.ToDo("임시 처방 SetPlayerObject가 안되는 이유 알아내야함");
                if (Runner.GetPlayerObject(Runner.LocalPlayer) == null)
                    GetComponent<PlayerCameraController>().SetPlayerCamera(Object);
            }
            else
                name = "Remote Player";
        }

        public override void FixedUpdateNetwork()
        {
            // return을 하면 플레이어가 고정됨 이유 모름
            // if (HasInputAuthority == false)
            // {
            //     MouseRotateControl();
            //     MoveControl();
            //     return;
            // }
            //
            // if (Cursor.lockState == CursorLockMode.None)
            // {
            //     return;
            // }
            DebugManager.ToDo("return 되면 플레이어의  위치가 고정되는 문제 해결 찾기");

            var spawnPosition = UserData.Instance.UserDictionary[Runner.LocalPlayer].TeleportPosition;
            if (spawnPosition.Count != 0)
            {
                simpleKcc.SetPosition(spawnPosition[0]); 
                UserData.SetTeleportPosition(Runner.LocalPlayer, null);
            }

            if (GetInput(out PlayerInputData data))
            {       
                MouseRotateControl(data.MouseAxis);
                MoveControl(data);
                WeaponControl(data);
            }
        }

        private void MoveControl(PlayerInputData data = default)
        {
            Vector3 dir = Vector3.zero;
            Vector3 jumpImpulse = default;
            if (data.MoveFront)
                dir += transform.forward;
            if (data.MoveBack)
                dir += -transform.forward;
            if (data.MoveLeft)
                dir += -transform.right;
            if (data.MoveRight)
                dir += transform.right;

            dir *= Runner.DeltaTime * 100f;
            
            if (data.Jump) 
            {
                jumpImpulse = Vector3.up * 100;
            }

            transform.position += dir;
            
            simpleKcc.Move(dir, jumpImpulse);
        }
        
        public float rotateSpeed = 500.0f;
        float xRotate, yRotate, xRotateMove, yRotateMove;
        public void MouseRotateControl(Vector2 mouseAxis = default)
        {
            if (mouseAxis == Vector2.zero)
            {
                return;
            }
            
            xRotateMove = mouseAxis.y * Runner.DeltaTime * rotateSpeed;
            yRotateMove = mouseAxis.x * Runner.DeltaTime * rotateSpeed;

            yRotate = transform.eulerAngles.y + yRotateMove;
            xRotate = xRotate + xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
            var angle = new Vector3(xRotate, yRotate, 0);
            
            simpleKcc.SetLookRotation(angle);
        }

        void WeaponControl(PlayerInputData data)
        {
            if (data.ChangeWeapon0)
            {
                equipment = GetComponentInChildren<WeaponBase>();
            }
            
            if (data.Attack && equipment != null)
            {
                equipment.AttackAction?.Invoke();
            }

            if (data.ReLoad && equipment.IsGun)
            {
                var gun = equipment as GunBase;
                gun.ReLoadBullet();
            }
        }
    }
}