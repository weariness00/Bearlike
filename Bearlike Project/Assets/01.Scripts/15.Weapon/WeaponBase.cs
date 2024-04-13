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
            EquipAction += SetLayer;
            EquipAction += SetOwnerID;
            status = GetComponent<StatusBase>();
        }

        public virtual void Start()
        {
        
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        void SetOwnerID(GameObject equipObject)
        {
            OwnerId = equipObject.GetComponent<NetworkObject>().Id;
        }
        
        void SetLayer(GameObject equipObject)
        {
            if (HasInputAuthority)
            {
                gameObject.layer = LayerMask.NameToLayer("Weapon"); 
            }
            else
            {
                gameObject.layer = 0;
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