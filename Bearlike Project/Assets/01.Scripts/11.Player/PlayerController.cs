using System;
using System.Collections.Generic;
using Data;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Item;
using Manager;
using Photon;
using Script.Weapon.Gun;
using Skill;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using Util;
using Weapon;

namespace Player
{
    [RequireComponent(typeof(PlayerCameraController), typeof(PlayerStatus))]
    public class PlayerController : NetworkBehaviourEx
    {
        public PlayerRef PlayerRef => Object.InputAuthority;
        
        // public Status status;
        [Header("컴포넌트")] public PlayerStatus status;
        public PlayerCameraController cameraController;
        public SkillSystem skillSystem;
        public WeaponSystem weaponSystem;
        public ItemInventory itemInventory;
        private NetworkMecanimAnimator _networkAnimator;
        [HideInInspector] public SimpleKCC simpleKcc;
        [HideInInspector] public Rigidbody rigidBody;

        public IEquipment equipment;
        public StatusValue<int> ammo = new StatusValue<int>();

        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;

        #region Animation Parametar

        private static readonly int AniShoot = Animator.StringToHash("tShoot");
        private static readonly int AniFrontMove = Animator.StringToHash("fFrontMove");
        private static readonly int AniSideMove = Animator.StringToHash("fSideMove");
        private static readonly int AniDie = Animator.StringToHash("isDead");


        #endregion

        private void Awake()
        {
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            status = gameObject.GetComponent<PlayerStatus>();
            cameraController = GetComponent<PlayerCameraController>();
            weaponSystem = gameObject.GetComponentInChildren<WeaponSystem>();
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();

            equipment = GetComponentInChildren<IEquipment>();
        }

        private void Start()
        {
            status.injuryAction += () => { _networkAnimator.Animator.SetBool(AniDie, true); };
        }

        public override void Spawned()
        {
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            
            if (HasInputAuthority)
            {
                Object.
                name = "Local Player";
                
                Runner.SetPlayerObject(Runner.LocalPlayer, Object);
                // equipment?.Equip();
                weaponSystem.gun?.Equip();

                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
            }
            else
            {
                name = "Remote Player";
            }
        }

        public override void FixedUpdateNetwork()
        {
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

                if (data.ItemInventory)
                {
                    itemInventory.canvas.gameObject.SetActive(!itemInventory.canvas.gameObject.activeSelf);
                }

                if (data.SkillInventory)
                {
                    
                }
            }
        }

        private void MoveControl(PlayerInputData data = default)
        {
            if (HasStateAuthority == false)
            {
                return;
            }
            
            Vector3 dir = Vector3.zero;
            Vector3 jumpImpulse = default;
            bool isMoveX = false, isMoveY = false;
            if (data.MoveFront)
            {
                dir += transform.forward;
                isMoveX = true;
            }

            if (data.MoveBack)
            {
                dir += -transform.forward;
                isMoveX = true;
            }

            if (data.MoveLeft)
            {
                dir += -transform.right;
                isMoveY = true;
            }

            if (data.MoveRight)
            {
                dir += transform.right;
                isMoveY = true;
            }

            _networkAnimator.Animator.SetFloat(AniFrontMove, isMoveX ? 1 : 0);
            _networkAnimator.Animator.SetFloat(AniSideMove, isMoveY ? 1 : 0);
            
            dir *= Runner.DeltaTime * status.moveSpeed;

            if (data.Jump)
            {
                var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
                DebugManager.DrawRay(transform.position + new Vector3(0,0.03f,0), -transform.up * 0.1f, Color.blue, 1f);
                if (Runner.LagCompensation.Raycast(transform.position + new Vector3(0,0.03f,0), -transform.up, 0.1f, Runner.LocalPlayer, out var hit,Int32.MaxValue , hitOptions))
                {
                    jumpImpulse = Vector3.up * status.jumpPower;
                }
            }

            simpleKcc.Move(dir, jumpImpulse);
        }

        public float rotateSpeed = 500.0f;
        float xRotate, yRotate, xRotateMove, yRotateMove;
        public void MouseRotateControl(Vector2 mouseAxis = default)
        {
            if (HasStateAuthority == false)
            {
                return;
            }
            
            xRotateMove = mouseAxis.y * Runner.DeltaTime * rotateSpeed;
            yRotateMove = mouseAxis.x * Runner.DeltaTime * rotateSpeed;

            yRotate += yRotateMove;
            xRotate += xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -30, 30); // 위, 아래 제한 
            simpleKcc.SetLookRotation(new Vector3(-xRotate, yRotate, 0));
            mouseRotateObject.transform.rotation = Quaternion.Euler(-xRotate, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        }

        void WeaponControl(PlayerInputData data)
        {
            if (data.ChangeWeapon0)
            {
                weaponSystem.gun = GetComponentInChildren<GunBase>();
            }

            if (data.Attack && weaponSystem.gun != null)
            {
                _networkAnimator.SetTrigger(AniShoot);
                weaponSystem.gun.AttackAction?.Invoke();
            }

            if (data.ReLoad && weaponSystem.gun.IsGun)
            {
                var gun = weaponSystem.gun as GunBase;
                gun.ReLoadBullet();
            }
        }
        
        #region RPC Function

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public new void SetPositionRPC(Vector3 pos) => simpleKcc.SetPosition(pos);
        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetLookRotationRPC(Vector2 look) => simpleKcc.SetLookRotation(look);

        #endregion
    }
}