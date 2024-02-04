using System;
using Fusion;
using Script.Util;
using Scripts.State.GameStatus;
using Unity.VisualScripting;
using UnityEngine;

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
    public Status status;

    public virtual void Awake()
    {
        status = gameObject.GetOrAddComponent<Status>();
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

