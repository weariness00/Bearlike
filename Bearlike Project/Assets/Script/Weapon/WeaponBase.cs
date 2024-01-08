using System;
using UnityEngine;

public interface IEquipment
{
    public Action Action { get; set; }
    public bool IsEquip { get; set; }

    public void Equip();
}

public class WeaponBase : MonoBehaviour
{
}

