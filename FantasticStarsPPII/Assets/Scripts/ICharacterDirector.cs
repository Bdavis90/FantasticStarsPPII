using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterDirector
{
    public void onDeath();
    public void onHit();
    public void onShoot(WeaponStats _equippedWeapon);
}
