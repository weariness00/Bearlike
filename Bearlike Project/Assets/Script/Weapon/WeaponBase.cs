using System;
using Script.GameStatus;
using Script.Util;
using UnityEngine;

public interface IEquipment
{
    public Action AttackAction { get; set; }
    public bool IsEquip { get; set; }
    public bool IsGun { get; set; }

    public void Equip();
}

public class WeaponBase : MonoBehaviour
{
    public Status status;

    protected virtual void Start()
    {
        status = ObjectUtil.GetORAddComponet<Status>(gameObject);
    }
}

