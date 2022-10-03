using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponItem : DefaultItem
{
    public int _rangeLevel = 0;
    public int _damageLevel = 0;
    public int _explosionLevel = 0;

    public abstract void UseWeapon(Vector3 aimDirection);
    public abstract void UseWeaponAI(Vector3 aimDirection, float power);
    public abstract void CancelWeapon();

}