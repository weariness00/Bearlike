using System;
using Script.Util;
using Scripts.State.GameStatus;
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

public class WeaponBase : MonoBehaviour, IEquipment
{
    public State.StateClass.Base.State state;

    public virtual void Awake()
    {
        var stateSystem = gameObject.GetComponent<StateSystem>();
        state = stateSystem.GetState();
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

