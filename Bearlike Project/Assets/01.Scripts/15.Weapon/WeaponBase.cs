using System;
using Status;
using Fusion;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapon
{
    #region Weapon Interfaec
    
    public interface IWeaponHitEffect
    {
        public void OnWeaponHitEffect(Vector3 hitPosition);
    }

    public interface IWeaponHitSound
    {
        public void PlayWeaponHit();
    }

    public interface IWeaponHit
    {
        /// <summary>
        /// 자신을 첫번째 인자인 GameObject로
        /// 맟춘 대상을 두번째 인자 GameObject로
        /// </summary>
        public Action<GameObject, GameObject> BeforeHitAction { get; set; } 
        public Action<GameObject, GameObject> AfterHitAction { get; set; } 
    }

    public interface IWeaponIK
    {
        public GameObject LeftIK { get; set; }
        public GameObject RightIK { get; set; }
    }
    
    #endregion
    
    public interface IEquipment
    {
        public Action AttackAction { get; set; }
        public Action<GameObject> EquipAction { get; set; }
        public Action<GameObject> ReleaseEquipAction { get; set; }
    
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }
    }

    [RequireComponent(typeof(StatusBase))]
    public class WeaponBase : NetworkBehaviour, IEquipment, IWeaponIK
    {
        [Networked] public NetworkId OwnerId { get; set; }
        public StatusBase status;
        public LayerMask includeCollide;
        
        [Header("IK")]
        [SerializeField] private GameObject leftIK;
        [SerializeField] private GameObject rightIK;
        public GameObject LeftIK { get => leftIK; set => leftIK = value; }
        public GameObject RightIK { get => rightIK; set => rightIK = value; }
        
        [HideInInspector] public PlayerCameraController playerCameraController;
        
        private Action<GameObject> _equipAction;

        public virtual void Awake()
        {
            EquipAction += SetEquip;
            ReleaseEquipAction += (equipObject) => { gameObject.SetActive(false); };
            status = GetComponent<StatusBase>();
        }

        public virtual void Start()
        {
        
        }

        public override void Spawned()
        {
            base.Spawned();
        }
        
        public void SetEquip(GameObject equipObject)
        {
            // 카메라 셋팅
            playerCameraController = equipObject.GetComponent<PlayerCameraController>();
            
            // 주인 설정
            OwnerId = equipObject.GetComponent<NetworkObject>().Id;
            
            // 주인의 스테이터스 추가
            status.AddAdditionalStatus(equipObject.GetComponent<StatusBase>());
            
            // 레이어 설정
            if (HasInputAuthority)
            {
                var renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    r.gameObject.layer = LayerMask.NameToLayer("Weapon");
                }
            }
        }
        
        #region Equipment Interface

        public Action AttackAction { get; set; }
        public Action<GameObject> EquipAction { get; set; }
        public Action<GameObject> ReleaseEquipAction { get; set; }
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }
        
        #endregion
    }
}