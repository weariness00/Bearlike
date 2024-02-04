using System;
using Fusion;
using Script.Util;
using Scripts.State.GameStatus;
using State.StateClass.Base;
using State.StateSystem;
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
    public StateBase state;

    public virtual void Awake()
    {
        state = gameObject.transform.root.GetOrAddComponent<StateSystem>().State;
    }

    public virtual void Start()
    {
        
    }

    public Action AttackAction { get; set; }
    public Action EquipAction { get; set; }
    public bool IsEquip { get; set; }
    public bool IsGun { get; set; }
    public virtual void Equip()
    {
    }
}

