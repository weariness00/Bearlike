using System.Collections.Generic;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Item;
using Script.Data;
using Script.Manager;
using Script.Photon;
using Script.Weapon.Gun;
using Scripts.State.GameStatus;
using Skill;
using Skill.Container;
using State.StateClass;
using Unity.Mathematics;
using State.StateClass.Base;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerCameraController), typeof(PlayerStatus))]
    public class PlayerController : NetworkBehaviour
    {
        // public Status status;
        [Header("컴포넌트")]
        public PlayerStatus status;
        public PlayerCameraController cameraController;
        public SkillSystem skillSystem;
        private NetworkMecanimAnimator _networkAnimator;
        [HideInInspector] public SimpleKCC simpleKcc;
        
        public IEquipment equipment;
        public StatusValue<int> ammo = new StatusValue<int>();

        [Tooltip("마우스 움직임에 따라 회전할 오브젝트")] public GameObject mouseRotateObject;
        
        [Header("아이템")] 
        public Dictionary<int, ItemBase> itemList = new Dictionary<int, ItemBase>();

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
            skillSystem = gameObject.GetOrAddComponent<SkillSystem>();
            
            equipment = GetComponentInChildren<IEquipment>();
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
                equipment?.Equip();
                
                DebugManager.Log($"Set Player Object : {Runner.LocalPlayer} - {Object}");
            }
            else
                name = "Remote Player";
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
                if(data.Cursor)
                    return;

                MouseRotateControl(data.MouseAxis);
                MoveControl(data);
                WeaponControl(data);
                SkillControl(data);
            }
        }

        private void MoveControl(PlayerInputData data = default)
        {
            Vector3 dir = Vector3.zero;
            Vector3 jumpImpulse = default;
            bool isMoveX= false, isMoveY = false;
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
            xRotateMove = mouseAxis.y * Runner.DeltaTime * rotateSpeed;
            yRotateMove = mouseAxis.x * Runner.DeltaTime * rotateSpeed;

            yRotate += yRotateMove;
            xRotate += xRotateMove;

            xRotate = Mathf.Clamp(xRotate, -30, 30); // 위, 아래 제한 
            simpleKcc.SetLookRotation(new Vector3(-xRotate, yRotate, 0));
            mouseRotateObject.transform.localEulerAngles = new Vector3(-xRotate, 0, 0);
        }

        void WeaponControl(PlayerInputData data)
        {
            if (data.ChangeWeapon0)
            {
                equipment = GetComponentInChildren<WeaponBase>();
            }
            
            if (data.Attack && equipment != null)
            {
                _networkAnimator.SetTrigger(_aniShoot);
                equipment.AttackAction?.Invoke();
            }

            if (data.ReLoad && equipment.IsGun)
            {
                var gun = equipment as GunBase;
                gun.ReLoadBullet();
            }
        }

        void SkillControl(PlayerInputData data)
        {
            if(Input.GetKeyDown(KeyCode.F1))
            // if (data.FirstSkill)
            {
                skillSystem.skillList[0].Run();
            }
        }
    }
}