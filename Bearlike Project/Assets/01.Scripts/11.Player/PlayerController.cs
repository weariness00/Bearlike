using System;
using Data;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GamePlay;
using Item;
using Manager;
using Photon;
using Skill;
using Status;
using UI;
using UI.Skill;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Weapon;
using Weapon.Gun;

namespace Player
{
    [RequireComponent(typeof(PlayerCameraController), typeof(PlayerStatus))]
    public class PlayerController : NetworkBehaviourEx
    {
        public PlayerRef PlayerRef => Object.InputAuthority;
        public PlayerCharacterType playerType;
        
        // public Status status;
        [Header("컴포넌트")] 
        public PlayerStatus status;
        public PlayerCameraController cameraController;
        public SkillSystem skillSystem;
        public WeaponSystem weaponSystem;
        public Canvas gunUI;
        public Canvas hpUI;
        public ItemInventory itemInventory;
        public SkillInventory skillInventory;
        public SkillSelectUI skillSelectUI;
        public SkillCanvas skillCanvas;
        public Animator animator;
        [HideInInspector] public SimpleKCC simpleKcc;
        [HideInInspector] public RigBuilder rigBuilder;
        private HitboxRoot _hitboxRoot;
        private Rig _headRig;
        private StageSelectUI _stageSelectUI;

        public StatusValue<int> ammo = new StatusValue<int>();

        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;

        public Action<GameObject> MonsterKillAction;
        
        [Networked] public float W { get; set; }
        private TickTimer _uiKeyDownTimer;
        
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
            // 임시로 장비 착용
            // 상호작용으로 착요하게 바꿀 예정
            status = gameObject.GetComponent<PlayerStatus>();
            cameraController = GetComponent<PlayerCameraController>();
            weaponSystem = gameObject.GetComponentInChildren<WeaponSystem>();
            animator = GetComponentInChildren<Animator>();

            _stageSelectUI = FindObjectOfType<StageSelectUI>();

            _hitboxRoot = GetComponent<HitboxRoot>();
            rigBuilder = GetComponentInChildren<RigBuilder>();
            _headRig = rigBuilder.layers.Find(rig => rig.name == "Head Rig").rig;
        }
        
        public override void Spawned()
        {
            DebugManager.LogWarning("headRig를 Update에서 계속 바꿔주고 있는거고치기");
            
            _gunLayer = animator.GetLayerIndex("Gun Layer");

            StatusInit();

            _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 1);
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            
            // 무기 초기화
            weaponSystem.equipment?.EquipAction?.Invoke(gameObject);
            animator.SetLayerWeight(_gunLayer, 1);
            
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

                status.InjuryAction += () => cameraController.ChangeCameraMode(CameraMode.ThirdPerson);
                status.RecoveryFromInjuryAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);
                status.ReviveAction += () => cameraController.ChangeCameraMode(CameraMode.Free);
                status.RecoveryFromReviveAction += () => cameraController.ChangeCameraMode(CameraMode.FirstPerson);

                gunUI.gameObject.SetActive(true);
                hpUI.gameObject.SetActive(true);
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
            }
            else
            {
                name = "Remote Player";
            }
        }

        public override void FixedUpdateNetwork()
        {
            // 임시
            _headRig.weight = W;
            
            if(!GameManager.Instance.isControl)
                return;
            
            if (transform.position.y <= -1)
                UserData.SetTeleportPosition(Runner.LocalPlayer, Vector3.up);
            
            var spawnPosition = UserData.Instance.UserDictionary[Runner.LocalPlayer].TeleportPosition;
            if (spawnPosition.Count != 0)
            {
                simpleKcc.SetPosition(spawnPosition[0]);
                UserData.SetTeleportPosition(Runner.LocalPlayer, null);
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
        #endregion
        
        #region Member Function
        
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

            if (data.StageSelect)
            {
                var uiObj = _stageSelectUI.gameObject;
                var isActive = uiObj.activeSelf;
                GameUIManager.ActiveUIAllDisable();
                if (!isActive)
                {
                    uiObj.SetActive(true);
                    GameUIManager.AddActiveUI(uiObj);
                }
                
                _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 2);
            }
                
            if (data.ItemInventory)
            {
                var uiObj = itemInventory.canvas.gameObject;
                var isActive = uiObj.activeSelf;
                GameUIManager.ActiveUIAllDisable();
                if (!isActive)
                {
                    uiObj.SetActive(true);
                    GameUIManager.AddActiveUI(uiObj);
                }
                _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 2);
            }

            if (data.SkillInventory)
            {
                var uiObj = skillInventory.canvas.gameObject;
                var isActive = uiObj.activeSelf;
                GameUIManager.ActiveUIAllDisable();
                if (!isActive)
                {
                    uiObj.SetActive(true);
                    GameUIManager.AddActiveUI(uiObj);
                }
                _uiKeyDownTimer = TickTimer.CreateFromTicks(Runner, 2);
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

            animator.SetFloat(AniInjuryMove, isMoveX || isMoveY ? 1 : 0);
            animator.SetFloat(AniFrontMove, isMoveX ? 1 : 0);
            animator.SetFloat(AniSideMove, isMoveY ? 1 : 0);
            
            dir *= Runner.DeltaTime * status.moveSpeed * 110f;

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
                if (data.ChangeWeapon0) ChangeWeaponRPC(0);
                else if (data.ChangeWeapon1) ChangeWeaponRPC(1);
                else if (data.ChangeWeapon2) ChangeWeaponRPC(2);
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