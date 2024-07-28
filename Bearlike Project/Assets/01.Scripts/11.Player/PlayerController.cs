using System;
using System.Collections;
using System.Collections.Generic;
using Aggro;
using Data;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GamePlay;
using Loading;
using Manager;
using Photon;
using Player.Container;
using Skill;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using Weapon;
using Weapon.Gun;

namespace Player
{
    [RequireComponent(typeof(PlayerCameraController), typeof(PlayerStatus))]
    public class PlayerController : NetworkBehaviourEx, IAfterApplyDamage
    {
        #region Static

        public static bool CheckPlayer(GameObject obj, out PlayerController pc)
        {
            pc = null;
            if (obj.CompareTag("Player") && obj.transform.parent.TryGetComponent(out pc))
            {
                return true;
            }

            return false;
        }

        #endregion
        
        public PlayerRef PlayerRef => Object.InputAuthority;
        public PlayerCharacterType playerType;
        public Sprite icon;
        
        // public Status status;
        [Header("Player Related")] 
        public PlayerStatus status;
        public PlayerUIController uiController;
        public PlayerCameraController cameraController;
        public PlayerWeaponCameraController weaponCameraController;
        public PlayerSoundController soundController;
        public PlayerRigController rigController;
        public SkillSystem skillSystem;
        public WeaponSystem weaponSystem;
        public AggroTarget aggroTarget;
        public NetworkMecanimAnimator networkAnimator;

        private HitboxRoot _hitboxRoot;
        
        [HideInInspector] public SimpleKCC simpleKcc;
        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public List<GameObject> mouseRotateObjects;

        public Action<GameObject> MonsterKillAction;
        public Action<int> AfterApplyDamageAction { get; set; }
        
        [Networked] public NetworkBool IsCursor { get; private set; } = false;
        [Networked] public float W { get; set; } = 1f;
        private TickTimer _dashTimer;

        public float _dashAmount = 100.0f;
        
        private ChangeDetector _changeDetector;
        
        #region Animation

        [Networked] private NetworkBool IsMove { get; set; }
        
        private int OneHandGunLayer = 1;
        private int TwoHandGunLayer = 2;

        private static readonly int AniMovement = Animator.StringToHash("f Movement");
        private static readonly int AniFainMove = Animator.StringToHash("f Faint Move");
        private static readonly int AniShoot = Animator.StringToHash("tShoot");
        private static readonly int AniJump = Animator.StringToHash("tJump");
        private static readonly int AniInjury = Animator.StringToHash("tInJury");
        private static readonly int AniDie = Animator.StringToHash("tDead");
        private static readonly int AniRevive = Animator.StringToHash("tRevive");

        public void SetLayer(float weight = 1)
        {
            networkAnimator.Animator.SetLayerWeight(OneHandGunLayer, 0);
            networkAnimator.Animator.SetLayerWeight(TwoHandGunLayer, 0);
            if (weaponSystem.TryGetEquipGun(out GunBase gun))
            {
                switch (gun.handType)
                {
                    case GunBase.GunHandType.OneHand:
                        networkAnimator.Animator.SetLayerWeight(OneHandGunLayer, weight);
                        break;
                    case GunBase.GunHandType.TwoHand:
                        networkAnimator.Animator.SetLayerWeight(TwoHandGunLayer, weight);
                        break;
                }
            }
        }
        
        #endregion

        #region Unity Event Function
        private void Awake()
        {
            status = gameObject.GetComponent<PlayerStatus>();
            uiController = GetComponentInChildren<PlayerUIController>();
            cameraController = GetComponent<PlayerCameraController>();
            weaponCameraController = GetComponentInChildren<PlayerWeaponCameraController>();
            soundController = GetComponent<PlayerSoundController>();
            rigController = GetComponentInChildren<PlayerRigController>();
            weaponSystem = gameObject.GetComponentInChildren<WeaponSystem>();
            networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
            aggroTarget = GetComponent<AggroTarget>();

            _hitboxRoot = GetComponent<HitboxRoot>();
            
            // 애니메이터
            OneHandGunLayer = networkAnimator.Animator.GetLayerIndex("One Hand Gun");
            TwoHandGunLayer = networkAnimator.Animator.GetLayerIndex("Two Hand Gun");
        }

        private void Start()
        {
            aggroTarget.AddCondition(AggroCondition);
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                if (KeyManager.InputActionDown(KeyToAction.LockCursor))
                {
                    switch (Cursor.lockState)
                    {
                        case CursorLockMode.None:
                            Cursor.lockState = CursorLockMode.Locked;
                            SetIsCursorRPC(false);
                            break;
                        case CursorLockMode.Locked:
                            Cursor.lockState = CursorLockMode.None;
                            SetIsCursorRPC(true);
                            break;
                    }
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            LoadingManager.AddWait();

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            
            StatusInit();

            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            
            // 무기 초기화
            ChangeWeaponRPC(0);
            SetLayer();

            // 스킬 초기화
            foreach (var skill in skillSystem.skillList)
            {
                skill.ownerPlayer = this;
                skill.Earn(gameObject);
            }
            
            // 권한에 따른 초기화
            if (HasInputAuthority)
            {
                Object.
                name = "Local Player";
                Runner.SetPlayerObject(Runner.LocalPlayer, Object);
                
                rigController.EnableArmMesh();
                
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
            }
            else
            {
                name = "Remote Player";
            }
            
            StartCoroutine(InitCoroutine());

            _dashTimer = TickTimer.CreateFromTicks(Runner, 0);
        }

        public override void FixedUpdateNetwork()
        {
            // 임시
            rigController.RigWeight = W;
            
            if(!GameManager.Instance.isControl)
                return;

            if (HasStateAuthority)
            {
                if (transform.position.y <= -1)
                    UserData.SetTeleportPosition(Object.InputAuthority, Vector3.up);
            
                if (UserData.HasTeleportPosition(Object.InputAuthority))
                {
                    simpleKcc.SetPosition(UserData.GetTeleportPosition(Object.InputAuthority));
                    UserData.SetTeleportPosition(Object.InputAuthority, null);
                }
            }
            
            if(status.isRevive)
                return;

            if (GetInput(out PlayerInputData data))
            {
                if (!IsCursor)
                {
                    MouseRotateControl(data.MouseAxis);
                    MoveControl(data);
                    WeaponControl(data);
                }
                else
                    simpleKcc.Move(Vector3.zero, Vector3.zero);
            }
        }

        public override void Render()
        {
            base.Render();
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change) 
                {
                    case nameof(IsSpawnSuccess):
                        if(IsSpawnSuccess) LoadingManager.EndWait();
                        break;
                }
            }
            
            networkAnimator.Animator.SetFloat(AniFainMove, IsMove ? 1 : 0);
            networkAnimator.Animator.SetFloat(AniMovement, IsMove ? 1 : 0);
        }

        #endregion
        
        #region Member Function

        private IEnumerator InitCoroutine()
        {
            LoadingManager.AddWait();
            
            GameManager.Instance.isControl = false;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;

                simpleKcc.SetLookRotation(Vector3.Lerp(Vector3.zero, new Vector3(0,720,0), t));
                yield return null;
            }

            GameManager.Instance.isControl = true;
            
            LoadingManager.EndWait();
            SpawnedSuccessRPC(UserData.ClientNumber, true);
        }
        
        private void StatusInit()
        {
            if (HasInputAuthority)
            {
                status.InjuryAction += rigController.DisableArmMesh;
                status.RecoveryFromInjuryAction += rigController.EnableArmMesh;
                status.RecoveryFromReviveAction += rigController.EnableArmMesh;

                status.InjuryAction += () => cameraController.ChangeCameraMode(CameraMode.ThirdPerson);
                status.RecoveryFromInjuryAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);
                status.ReviveAction += () => cameraController.ChangeCameraMode(CameraMode.Free);
                status.RecoveryFromReviveAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);
            }
            
            // Status 관련 초기화
            status.InjuryAction += () =>
            {
                GameManager.Instance.AlivePlayerCount--;

                networkAnimator.SetTrigger(AniInjury, true);
                SetLayer(0);
                // _headRig.weight = 0;
                W = 0;
                simpleKcc.Collider.transform.localPosition = new Vector3(0.05f, 0.33f, -0.44f);
                simpleKcc.Collider.transform.Rotate(90,0,0);
                foreach (var hitbox in _hitboxRoot.Hitboxes)
                {
                    hitbox.transform.localPosition = new Vector3(0.05f, 0.33f, -0.44f);
                    hitbox.transform.Rotate(90,0,0);
                }
                
                if(weaponSystem.equipment is WeaponBase weapon)
                    weapon.gameObject.SetActive(false);
            };
            status.RecoveryFromInjuryAction += () =>
            {
                GameManager.Instance.AlivePlayerCount++;
                
                networkAnimator.SetTrigger(AniRevive,true);
                SetLayer(0);
                W = 1;
                simpleKcc.Collider.transform.localPosition = Vector3.zero;
                simpleKcc.Collider.transform.localRotation = Quaternion.identity;
                foreach (var hitbox in _hitboxRoot.Hitboxes)
                {
                    hitbox.transform.localPosition = Vector3.zero;
                    hitbox.transform.localRotation = Quaternion.identity;
                }
                
                if(weaponSystem.equipment is WeaponBase weapon)
                    weapon.gameObject.SetActive(true);
            };
            status.ReviveAction += () =>
            {
                networkAnimator.SetTrigger(AniDie, true);
            };
            status.RecoveryFromReviveAction += () =>
            {
                GameManager.Instance.AlivePlayerCount++;
                
                networkAnimator.SetTrigger(AniRevive, true);
                SetLayer(1);
                W = 1;
                simpleKcc.Collider.transform.localPosition = Vector3.zero;
                simpleKcc.Collider.transform.localRotation = Quaternion.identity;
                foreach (var hitbox in _hitboxRoot.Hitboxes)
                {
                    hitbox.transform.localPosition = Vector3.zero;
                    hitbox.transform.localRotation = Quaternion.identity;
                }
                
                if(weaponSystem.equipment is WeaponBase weapon)
                    weapon.gameObject.SetActive(true);
            };
        }

        private void MoveControl(PlayerInputData data = default)
        {
            if (HasStateAuthority == false)
            {
                return;
            }
            
            Vector3 dir = Vector3.zero;
            Vector3 jumpImpulse = Vector3.zero;
            bool isMoveX = false, isMoveY = false;
            bool isDash = false;
            
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
            
            if (data.Dash && !status.isInjury && !status.isRevive)
            {
                if (_dashTimer.Expired(Runner))
                {
                    _dashTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);                
                    dir += transform.forward * _dashAmount;
                    isMoveX = true;
                }
            }

            IsMove = isMoveX || isMoveY;
            
            dir *= Runner.DeltaTime * status.GetMoveSpeed() * 110f;

            // if (isDash)
            //     dir *= 2;
            
            if (data.Jump)
            {
                var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
                DebugManager.DrawRay(transform.position + new Vector3(0,0.03f,0), -transform.up * 0.1f, Color.blue, 1f);
                if (Runner.LagCompensation.Raycast(transform.position + new Vector3(0,0.03f,0), -transform.up, 0.1f, Object.InputAuthority, out var hit,Int32.MaxValue , hitOptions))
                {
                    jumpImpulse = Vector3.up * status.jumpPower;
                    networkAnimator.SetTrigger(AniJump);
                }
            }

            var dirY = simpleKcc.RealVelocity.y;
            if ( 0 < dirY && dirY < 0.2f)
                jumpImpulse = -Vector3.up * 100f;   
            simpleKcc.Move(dir, jumpImpulse);
        }

        public float rotateSpeed = 500.0f;
        float xRotate, yRotate, xRotateMove, yRotateMove;
        public void MouseRotateControl(Vector2 mouseAxis = default)
        {
            if (HasStateAuthority == false || status.isRevive)
                return;
            
            xRotateMove = mouseAxis.y * Runner.DeltaTime * rotateSpeed;
            yRotateMove = mouseAxis.x * Runner.DeltaTime * rotateSpeed;

            yRotate += yRotateMove;
            xRotate += xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -45, 45); // 위, 아래 제한 
            simpleKcc.SetLookRotation(new Vector3(-xRotate, yRotate, 0));
            foreach (var mouseRotateObject in mouseRotateObjects)
                mouseRotateObject.transform.rotation = Quaternion.Euler(-xRotate, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        }

        void WeaponControl(PlayerInputData data)
        {
            if(status.isInjury)
                return;

            // if (HasInputAuthority)
            // {
            //     var overlayCameraSetups = FindObjectsOfType<OverlayCameraSetup>();
            //     
            //     if (data.ChangeWeapon0)
            //     {
            //         foreach (var overlayCameraSetup in overlayCameraSetups)
            //         {
            //             overlayCameraSetup.ChangeWeapon(0);
            //         }
            //         ChangeWeaponRPC(0);
            //     }
            //     else if (data.ChangeWeapon1)
            //     {
            //         foreach (var overlayCameraSetup in overlayCameraSetups)
            //         {
            //             overlayCameraSetup.ChangeWeapon(1);
            //         }
            //         ChangeWeaponRPC(1);
            //     }
            //     else if (data.ChangeWeapon2)
            //     {
            //         foreach (var overlayCameraSetup in overlayCameraSetups)
            //         {
            //             overlayCameraSetup.ChangeWeapon(2);
            //         }
            //         ChangeWeaponRPC(2);
            //     }
            // }

            if (data.Attack && weaponSystem.equipment.IsGun)
            {
                networkAnimator.Animator.SetBool(AniShoot, true);
                if(HasStateAuthority) weaponSystem.equipment.AttackAction?.Invoke();
            }
            else if (networkAnimator.Animator.GetBool(AniShoot) == true)
            {
                networkAnimator.Animator.SetBool(AniShoot, false);
            }

            if (data.ReLoad && weaponSystem.equipment.IsGun)
            {
                var gun = ((GunBase)weaponSystem.equipment);
                gun.ReloadBullet();
            }
        }

        #region Aggro

        private bool AggroCondition()
        {
            if (status.isInjury ||
                status.isRevive ||
                status.IsDie)
            {
                return false;
            }

            return true;
        }

        #endregion
        
        #endregion
        
        #region RPC Function

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void SetIsCursorRPC(NetworkBool value) => IsCursor = value;

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public new void SetPositionRPC(Vector3 pos) => simpleKcc.SetPosition(pos);
        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetLookRotationRPC(Vector2 look) => simpleKcc.SetLookRotation(look);

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ChangeWeaponRPC(int index)
        {
            DebugManager.ToDo("Muzzle 임시 처리");
            weaponSystem.weaponList[index].transform.Find("Muzzle").gameObject.SetActive(false);

            if (weaponSystem.ChangeEquipment(index, gameObject))
            {
                DebugManager.Log($"{name}이 총을 [ {index} ]로 변경");
                
                // 장비에 맞는 애니메이터 Layer Weight 주기
                SetLayer();
                
                // 변경된 장비에 스킬이 적용되도록 스킬 초기화
                foreach (var skillBase in skillSystem.skillList)
                {
                    skillBase.Earn(gameObject);
                }

                // ik 설정
                var ik = weaponSystem.equipment as IWeaponIK;
                if (ik.LeftIK)
                {
                    rigController.LeftArmWeight = 1;
                    rigController.SetLeftArmIK(ik.LeftIK);
                }
                else
                    rigController.LeftArmWeight = 0;

                if (ik.RightIK)
                {
                    rigController.RightArmWeight = 1;
                    rigController.SetRightArmIK(ik.RightIK);
                }
                else
                    rigController.RightArmWeight = 0;
                
                // Weapon Type에 따른 Camera 위치 변경
                weaponCameraController.ChangeType(weaponSystem.equipment);
            }
        }
        
        #endregion
    }
}