﻿using System;
using Fusion;
using Script.Util;
using Scripts.State.GameStatus;
using State;
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
    [FormerlySerializedAs("state")] public StatusBase status;

    public virtual void Awake()
    {
        status = gameObject.transform.root.GetOrAddComponent<StatusSystem>().Status;
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
    }
}

