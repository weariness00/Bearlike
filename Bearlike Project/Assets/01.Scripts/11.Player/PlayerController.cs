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
    public class PlayerController : NetworkBehaviourEx, IInteract
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
        public float interactLength = 1f; // 상호작용 범위

        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;

        #region Animation Parametar

        private readonly int _aniShoot = Animator.StringToHash("tShoot");
        private readonly int _aniFrontMove = Animator.StringToHash("fFrontMove");
        private readonly int _aniSideMove = Animator.StringToHash("fSideMove");

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

        public override void Spawned()
        {
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            
            if (HasInputAuthority)
            {
                Object.
                name = "Local Player";

                // Runner.SetPlayerObject(Runner.LocalPlayer, Object);
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
                if (data.Cursor)
                    return;

                MouseRotateControl(data.MouseAxis);
                MoveControl(data);
                WeaponControl(data);
                CheckInteract(data);

                if (data.ItemInventory)
                {
                    itemInventory.canvas.gameObject.SetActive(!itemInventory.canvas.gameObject.activeSelf);
                }

                if (data.SkillInventory)
                {
                    
                }
            }
            HpControl();
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

            _networkAnimator.Animator.SetFloat(_aniFrontMove, isMoveX ? 1 : 0);
            _networkAnimator.Animator.SetFloat(_aniSideMove, isMoveY ? 1 : 0);
            
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

        // 상호 작용
        void CheckInteract(PlayerInputData data)
        {
            if (HasInputAuthority == false)
            {
                return;
            }
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            DebugManager.DrawRay(ray.origin, ray.direction * interactLength, Color.red, 1.0f);
            if (Runner.LagCompensation.Raycast(ray.origin, ray.direction, interactLength, Object.InputAuthority, out var hit, Int32.MaxValue, hitOptions))
            {
                var interact = hit.GameObject.GetComponent<IInteract>();
                if (interact is { IsInteract: true })
                {
                    if (data.Interact)
                    {
                        interact.InteractEnterAction?.Invoke(gameObject);
                    }
                    else
                    {
                        interact.InteractExitAction?.Invoke(gameObject);
                    }
                    DebugManager.ToDo("상호작용이 가능할 경우 상호작용 키 UI 띄어주기");
                }
            }
        }

        void WeaponControl(PlayerInputData data)
        {
            if (data.ChangeWeapon0)
            {
                weaponSystem.gun = GetComponentInChildren<GunBase>();
            }

            if (data.Attack && weaponSystem.gun != null)
            {
                _networkAnimator.SetTrigger(_aniShoot);
                weaponSystem.gun.AttackAction?.Invoke();
            }

            if (data.ReLoad && weaponSystem.gun.IsGun)
            {
                var gun = weaponSystem.gun as GunBase;
                gun.ReLoadBullet();
            }
        }

        /// <summary>
        /// 체력이 0이면 부상
        /// 부상에서 일정 시간이 지나면 죽음으로 바뀌게 하는 로직
        /// </summary>
        void HpControl()
        {
            // 부상 상태 로직
            if (status.isInjury)
            {
                status.SetInjuryTimeRPC(status.injuryTime.Current - Runner.DeltaTime);
                if (status.injuryTime.isMin)
                {
                    status.isInjury = false;
                    status.isRevive = true;
                }
            }
            else if (status.isRevive)
            {
            }
            // 부상 상태로 전환
            else if (status.IsDie)
            {
                IsInteract = true;
                status.isInjury = true;
                status.injuryTime.Current = status.injuryTime.Max; // 이건 부상 상태를 유지하는 시간
            }
            else
            {
                IsInteract = false;
            }
        }

        #region Interact 

        #region Interface

        public void InteractInit()
        {
            IsInteract = false;

            InteractEnterAction += RecoveryInteract;
            InteractEnterAction += ReviveInteract;

            InteractExitAction += (targetObject) =>
            {
                var remotePlayerStatus = targetObject.GetComponent<PlayerStatus>();
                remotePlayerStatus.SetRecoveryTimeRPC(0);
            };
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
        
        #endregion

        /// <summary>
        /// 부상 상태에 빠진 것을 회복하는 상호작용
        /// </summary>
        void RecoveryInteract(GameObject targetObject)
        {
            if (status.isInjury)
            {
                var remotePlayerStatus = targetObject.GetComponent<PlayerStatus>();
                if (remotePlayerStatus.recoveryTime.isMax)
                {
                    // 부상 회복
                    status.hp.Current = status.hp.Max / 3;
                    
                    // remote Player의 부상 관련 스테이터스 초기화
                    remotePlayerStatus.SetIsInjuryRPC(false);
                    remotePlayerStatus.SetRecoveryTimeRPC(0);
                    IsInteract = false;
                    return;
                }
                remotePlayerStatus.SetRecoveryTimeRPC(remotePlayerStatus.recoveryTime.Current + Runner.DeltaTime);
            }
        }

        /// <summary>
        /// 다른 플레이어 소생하는 상호작용
        /// </summary>
        void ReviveInteract(GameObject targetObject)
        {
            if (status.isRevive)
            {
                DebugManager.ToDo("소생 아이템 생기면 소생 로직 만들기");
            }
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public new void SetPositionRPC(Vector3 pos) => simpleKcc.SetPosition(pos);
        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetLookRotationRPC(Vector2 look) => simpleKcc.SetLookRotation(look);

        #endregion
    }
}