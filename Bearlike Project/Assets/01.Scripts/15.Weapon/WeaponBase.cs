using System;
using Status;
using Fusion;
using Manager;
using Player;
using UnityEngine;
using UnityEngine.VFX;

namespace Weapon
{
    public interface IWeaponHitEffect
    {
        public void OnWeaponHitEffect(Vector3 hitPosition);
    }
    
    public interface IEquipment
    {
        public Action AttackAction { get; set; }
        public Action<GameObject> EquipAction { get; set; }
        public Action<GameObject> ReleaseEquipAction { get; set; }
    
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }
    }

    [RequireComponent(typeof(StatusBase))]
    public class WeaponBase : NetworkBehaviour, IEquipment
    {
        [Networked] public NetworkId OwnerId { get; set; }
        public StatusBase status;
        public LayerMask includeCollide;
        
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
            gameObject.SetActive(true);
            
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

                DebugManager.ToDo("Muzzle Layer 설정 변경해야함");
                var muzzle_transform = transform.Find("Muzzle");
                
                if(null != muzzle_transform)
                    muzzle_transform.gameObject.layer = LayerMask.NameToLayer("Weapon");
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