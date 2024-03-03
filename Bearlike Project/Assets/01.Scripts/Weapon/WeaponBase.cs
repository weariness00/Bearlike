using System;
using Fusion;
using Script.Util;
using State;
using State.StateClass;
using State.StateClass.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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

