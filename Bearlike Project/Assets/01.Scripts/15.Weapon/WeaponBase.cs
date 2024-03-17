using System;
using Fusion;
using State.StateClass.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Weapon
{
    public interface IEquipment
    {
        public Action AttackAction { get; set; }
        public Action EquipAction { get; set; }
    
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }

        public void Equip();
    }

    public class WeaponBase : NetworkBehaviour, IEquipment
    {
        public StatusBase status;
        public LayerMask includeCollide;
        
        [Header("기본 이펙트")]
        public VisualEffect hitEffect; // 타격 이펙트

        [Header("기본 사운드")] 
        public AudioSource hitSound;

        public virtual void Awake()
        {
            status = gameObject.transform.root.GetOrAddComponent<StatusBase>();
        }

        public virtual void Start()
        {
        
        }

        public override void Spawned()
        {
            base.Spawned();
            // state = gameObject.GetOrAddComponent<StateBase>();
        }

        public Action AttackAction { get; set; }
        public Action EquipAction { get; set; }
        public bool IsEquip { get; set; }
        public bool IsGun { get; set; }
        public virtual void Equip()
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
    }
}