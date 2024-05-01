using System;
using Data;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Item;
using Manager;
using Monster;
using Photon;
using Skill;
using Status;
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
        
        // public Status status;
        [Header("컴포넌트")] 
        public PlayerStatus status;
        public PlayerCameraController cameraController;
        public SkillSystem skillSystem;
        public WeaponSystem weaponSystem;
        public Canvas GunUI;
        public ItemInventory itemInventory;
        public SkillInventory skillInventory;
        public SkillSelectUI skillSelectUI;
        private NetworkMecanimAnimator _networkAnimator;
        [HideInInspector] public SimpleKCC simpleKcc;
        [HideInInspector] public RigBuilder rigBuilder;
        private Rig _headRig;

        public StatusValue<int> ammo = new StatusValue<int>();

        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;

        public Action<GameObject> MonsterKillAction;
        
        #region Animation Parametar

        private int _gunLayer;

        private static readonly int AniShoot = Animator.StringToHash("tShoot");
        private static readonly int AniFrontMove = Animator.StringToHash("fFrontMove");
        private static readonly int AniSideMove = Animator.StringToHash("fSideMove");
        private static readonly int AniJump = Animator.StringToHash("tJump");
        private static readonly int AniInjury = Animator.StringToHash("tInJury");
        private static readonly int AniRevive = Animator.StringToHash("tRevive");
        private static readonly int AniInjuryMove = Animator.StringToHash("Faint");
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

            rigBuilder = GetComponentInChildren<RigBuilder>();
            _headRig = rigBuilder.layers.Find(rig => rig.name == "Head Rig").rig;
        }

        public override void Spawned()
        {
            _gunLayer = _networkAnimator.Animator.GetLayerIndex("Gun Layer");
                
            // Status 관련 초기화
            status.InjuryAction += () =>
            {
                _networkAnimator.SetTrigger(AniInjury); 
                _networkAnimator.Animator.SetLayerWeight(_gunLayer, 0);
                _headRig.weight = 0;
                if(weaponSystem.equipment is WeaponBase weapon)
                    weapon.gameObject.SetActive(false);
            };
            status.RecoveryFromInjuryAction += () =>
            {
                _networkAnimator.SetTrigger(AniRevive);
                _networkAnimator.Animator.SetLayerWeight(_gunLayer, 1);
                _headRig.weight = 1;
                
                if(weaponSystem.equipment is WeaponBase weapon)
                    weapon.gameObject.SetActive(true);
            };
            status.ReviveAction += () => { _networkAnimator.SetTrigger(AniDie); };
            
            Cursor.lockState = CursorLockMode.Locked;
            simpleKcc = gameObject.GetOrAddComponent<SimpleKCC>();
            simpleKcc.Collider.tag = "Player";
            
            // 무기 초기화
            weaponSystem.equipment?.EquipAction?.Invoke(gameObject);
            _networkAnimator.Animator.SetLayerWeight(_gunLayer, 1);
            
            // 스킬 초기화
            foreach (var skillBase in skillSystem.skillList)
            {
                skillBase.Earn(gameObject);
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

                GunUI.gameObject.SetActive(true);
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
            
            if(status.isRevive)
                return;

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
                    skillInventory.canvas.gameObject.SetActive(!skillInventory.canvas.gameObject.activeSelf);
                }

                if (data.DebugKeyF1)
                {
                    status.ApplyDamageRPC(10, Object.Id);
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

            _networkAnimator.Animator.SetFloat(AniInjuryMove, isMoveX || isMoveY ? 1 : 0);
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
                    _networkAnimator.SetTrigger(AniJump);
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

            xRotate = Mathf.Clamp(xRotate, -30, 30); // 위, 아래 제한 
            simpleKcc.SetLookRotation(new Vector3(-xRotate, yRotate, 0));
            mouseRotateObject.transform.rotation = Quaternion.Euler(-xRotate, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        }

        void WeaponControl(PlayerInputData data)
        {
            if(status.isInjury)
                return;
            
            if (data.ChangeWeapon0)
            {
                // 장비 변경
                weaponSystem.ChangeEquipment(0, gameObject);
                
                // 장비에 맞는 애니메이터 Layer Weight 주기
                _networkAnimator.Animator.SetLayerWeight(_gunLayer, 0);
                if (weaponSystem.equipment.IsGun)
                    _networkAnimator.Animator.SetLayerWeight(_gunLayer, 1);
                
                // 변경된 장비에 스킬이 적용되도록 스킬 초기화
                foreach (var skillBase in skillSystem.skillList)
                {
                    skillBase.Earn(gameObject);
                }
            }

            if (data.Attack && weaponSystem.equipment.IsGun)
            {
                _networkAnimator.SetTrigger(AniShoot);
                weaponSystem.equipment.AttackAction?.Invoke();
            }

            if (data.ReLoad && weaponSystem.equipment.IsGun)
            {
                ((GunBase)weaponSystem.equipment).ReloadBulletRPC();
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