using System;
using Aggro;
using Data;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GamePlay;
using GamePlay.UI;
using Item;
using Loading;
using Manager;
using Photon;
using Skill;
using Status;
using UI;
using UI.Skill;
using UI.Status;
using UI.Weapon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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
        [Header("컴포넌트")] 
        public PlayerStatus status;
        public PlayerCameraController cameraController;
        public PlayerSoundController soundController;
        public SkillSystem skillSystem;
        public WeaponSystem weaponSystem;
        public Canvas gunUI;
        public Canvas hpUI;
        public ItemInventory itemInventory;
        public SkillInventory skillInventory;
        public SkillSelectUI skillSelectUI;
        public SkillCanvas skillCanvas;
        public PlayerEXP levelCanvas;
        public BuffCanvas buffCanvas;
        public GoodsCanvas goodsCanvas;
        public GameProgressCanvas progressCanvas;
        public AggroTarget aggroTarget;
        
        public Animator animator;
        [HideInInspector] public SimpleKCC simpleKcc;
        [HideInInspector] public RigBuilder rigBuilder;
        private HitboxRoot _hitboxRoot;
        private Rig _headRig;
        private StageSelectUI _stageSelectUI;
        
        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;

        public Action<GameObject> MonsterKillAction;
        public Action<int> AfterApplyDamageAction { get; set; }

        [Networked] public float W { get; set; } = 1f;
        private TickTimer _uiKeyDownTimer;
        
        private ChangeDetector _changeDetector;
        
        #region Animation Parametar

        private int _gunLayer;

        private static readonly int AniShoot = Animator.StringToHash("isShoot");
        private static readonly int AniFrontMove = Animator.StringToHash("fFrontMove");
        private static readonly int AniSideMove = Animator.StringToHash("fSideMove");
        private static readonly int AniJump = Animator.StringToHash("tJump");
        private static readonly int AniInjury = Animator.StringToHash("tInJury");
        private static readonly int AniRevive = Animator.StringToHash("tRevive");
        private static readonly int AniInjuryMove = Animator.StringToHash("Faint");
        private static readonly int AniDie = Animator.StringToHash("tDead");
        
        #endregion

        #region Unity Event Function
        private void Awake()
        {
            LoadingManager.AddWait();
            
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            status = gameObject.GetComponent<PlayerStatus>();
            cameraController = GetComponent<PlayerCameraController>();
            soundController = GetComponent<PlayerSoundController>();
            weaponSystem = gameObject.GetComponentInChildren<WeaponSystem>();
            animator = GetComponentInChildren<Animator>();
            aggroTarget = GetComponent<AggroTarget>();

            _stageSelectUI = FindObjectOfType<StageSelectUI>();

            _hitboxRoot = GetComponent<HitboxRoot>();
            rigBuilder = GetComponentInChildren<RigBuilder>();
            _headRig = rigBuilder.layers.Find(rig => rig.name == "Head Rig").rig;
        }

        private void Start()
        {
            status.LevelUpAction += () =>
            {
                if (HasInputAuthority)
                {
                    if (skillSelectUI.GetSelectCount() <= 0)
                        skillSelectUI.SpawnSkillBlocks(3);
                    skillSelectUI.AddSelectCount();
                }
            };
                
            aggroTarget.AddCondition(AggroCondition);
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            DebugManager.LogWarning("headRig를 Update에서 계속 바꿔주고 있는거고치기");
            
            _gunLayer = animator.GetLayerIndex("Gun Layer");

            StatusInit();

            _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 1);
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            // simpleKcc.SetGravity(new Vector3(0, -9.8f, 0));
            
            // 무기 초기화
            weaponSystem.equipment?.EquipAction?.Invoke(gameObject);
            animator.SetLayerWeight(_gunLayer, 1);
            
            // 스킬 초기화
            foreach (var skill in skillSystem.skillList)
            {
                skill.ownerPlayer = this;
                skill.Earn(gameObject);
            }
            
            progressCanvas = FindObjectOfType<GameProgressCanvas>();
            
            // 권한에 따른 초기화
            if (HasInputAuthority)
            {
                Object.
                name = "Local Player";
                Runner.SetPlayerObject(Runner.LocalPlayer, Object);

                status.InjuryAction += () => cameraController.ChangeCameraMode(CameraMode.ThirdPerson);
                status.RecoveryFromInjuryAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);
                status.ReviveAction += () => cameraController.ChangeCameraMode(CameraMode.Free);
                status.RecoveryFromReviveAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);

                CanvasActive(true);
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
            }
            else
            {
                CanvasActive(false);
                name = "Remote Player";
            }

            SpawnedSuccessRPC(UserData.ClientNumber, true);
        }

        public override void FixedUpdateNetwork()
        {
            // 임시
            _headRig.weight = W;
            
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
                if (!data.Cursor)
                {
                    MouseRotateControl(data.MouseAxis);
                    MoveControl(data);
                    WeaponControl(data);
                }
                else
                    simpleKcc.Move();

                if (HasInputAuthority)
                    UISetting(data);
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
        }

        #endregion
        
        #region Member Function

        private void CanvasActive(bool value)
        {
            gunUI.gameObject.SetActive(value);
            hpUI.gameObject.SetActive(value);
            levelCanvas.gameObject.SetActive(value);
            buffCanvas.gameObject.SetActive(value);
            goodsCanvas.gameObject.SetActive(value);
            skillCanvas.gameObject.SetActive(value);
        }
        
        private void StatusInit()
        {
            // Status 관련 초기화
            status.InjuryAction += () =>
            {
                GameManager.Instance.AlivePlayerCount--;

                animator.SetTrigger(AniInjury); 
                animator.SetLayerWeight(_gunLayer, 0);
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
                
                animator.SetTrigger(AniRevive);
                animator.SetLayerWeight(_gunLayer, 1);
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
                animator.SetTrigger(AniDie);
            };
            status.RecoveryFromReviveAction += () =>
            {
                GameManager.Instance.AlivePlayerCount++;
                
                animator.SetTrigger(AniRevive);
                animator.SetLayerWeight(_gunLayer, 1);
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

        private void UISetting(PlayerInputData data)
        {
            if(_uiKeyDownTimer.Expired(Runner) == false)
                return;

            void UIActive(GameObject uiObj)
            {
                var isActive = uiObj.activeSelf;
                UIManager.ActiveUIAllDisable();
                if (!isActive)
                {
                    uiObj.SetActive(true);
                    UIManager.AddActiveUI(uiObj);
                }
                
                _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 2);
            }

            if (data.StageSelect)
            {
                UIActive(_stageSelectUI.gameObject);
            }
            else if (data.ItemInventory)
            {
                UIActive(itemInventory.canvas.gameObject);
            }
            else if (data.SkillInventory)
            {
                UIActive(skillInventory.canvas.gameObject);
            }
            else if (data.SkillSelect)
            {
                UIActive(skillSelectUI.canvas.gameObject);
            }
            else if (data.GameProgress)
            {
                UIActive(progressCanvas.gameObject);
            }
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

            animator.SetFloat(AniInjuryMove, isMoveX || isMoveY ? 1 : 0);
            animator.SetFloat(AniFrontMove, isMoveX ? 1 : 0);
            animator.SetFloat(AniSideMove, isMoveY ? 1 : 0);
            
            dir *= Runner.DeltaTime * status.GetMoveSpeed() * 110f;

            if (data.Jump)
            {
                var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
                DebugManager.DrawRay(transform.position + new Vector3(0,0.03f,0), -transform.up * 0.1f, Color.blue, 1f);
                if (Runner.LagCompensation.Raycast(transform.position + new Vector3(0,0.03f,0), -transform.up, 0.1f, Runner.LocalPlayer, out var hit,Int32.MaxValue , hitOptions))
                {
                    jumpImpulse = Vector3.up * status.jumpPower;
                    animator.SetTrigger(AniJump);
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
            mouseRotateObject.transform.rotation = Quaternion.Euler(-xRotate, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        }

        void WeaponControl(PlayerInputData data)
        {
            if(status.isInjury)
                return;

            if (HasInputAuthority)
            {
                var overlayCameraSetups = FindObjectsOfType<OverlayCameraSetup>();
                
                if (data.ChangeWeapon0)
                {
                    foreach (var overlayCameraSetup in overlayCameraSetups)
                    {
                        overlayCameraSetup.ChangeWeapon(0);
                    }
                    ChangeWeaponRPC(0);
                }
                else if (data.ChangeWeapon1)
                {
                    foreach (var overlayCameraSetup in overlayCameraSetups)
                    {
                        overlayCameraSetup.ChangeWeapon(1);
                    }
                    ChangeWeaponRPC(1);
                }
                else if (data.ChangeWeapon2)
                {
                    foreach (var overlayCameraSetup in overlayCameraSetups)
                    {
                        overlayCameraSetup.ChangeWeapon(2);
                    }
                    ChangeWeaponRPC(2);
                }
            }
            

            if (data.Attack && weaponSystem.equipment.IsGun)
            {
                animator.SetBool(AniShoot, true);
                if(HasStateAuthority) weaponSystem.equipment.AttackAction?.Invoke();
            }
            else if (animator.GetBool(AniShoot) == true)
            {
                animator.SetBool(AniShoot, false);
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

        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public new void SetPositionRPC(Vector3 pos) => simpleKcc.SetPosition(pos);
        [Rpc(RpcSources.All,RpcTargets.StateAuthority)]
        public void SetLookRotationRPC(Vector2 look) => simpleKcc.SetLookRotation(look);

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void UISettingRPC(PlayerInputData data) => UISetting(data);

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ChangeWeaponRPC(int index)
        {
            DebugManager.ToDo("Muzzle 임시 처리");
            weaponSystem.weaponList[index].transform.Find("Muzzle").gameObject.SetActive(false);

            if (weaponSystem.ChangeEquipment(index, gameObject))
            {
                DebugManager.Log($"{name}이 총을 [ {index} ]로 변경");
                
                // 장비에 맞는 애니메이터 Layer Weight 주기
                animator.SetLayerWeight(_gunLayer, 0);
                if (weaponSystem.equipment.IsGun)
                    animator.SetLayerWeight(_gunLayer, 1);
                
                // 변경된 장비에 스킬이 적용되도록 스킬 초기화
                foreach (var skillBase in skillSystem.skillList)
                {
                    skillBase.Earn(gameObject);
                }
            }
        }
        
        #endregion
    }
}