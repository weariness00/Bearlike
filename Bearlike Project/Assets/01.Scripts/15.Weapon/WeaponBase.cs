using System;
using Status;
using Fusion;
using UnityEngine;
using UnityEngine.VFX;

namespace Weapon
{
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
        
        [Header("기본 이펙트")]
        public VisualEffect hitEffect; // 타격 이펙트

        [Header("기본 사운드")] 
        public AudioSource hitSound;

        private Action<GameObject> _equipAction;

        public virtual void Awake()
        {
            EquipAction += SetEquip;
            status = GetComponent<StatusBase>();
        }

        public virtual void Start()
        {
        
        }

        public override void Spawned()
        {
            base.Spawned();
        }
        
        void SetEquip(GameObject equipObject)
        {
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