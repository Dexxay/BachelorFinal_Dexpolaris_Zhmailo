using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCollectable : Collectable
{
    [SerializeField] private WeaponType weaponType;
    public override void OnBeingCollectedBy(MainPlayer character)
    {
        character.WeaponManager.AddWeapon(weaponType);
        Destroy(gameObject);
    }
}
