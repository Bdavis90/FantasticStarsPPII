using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] WeaponStats weapon;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.GetComponent<CharacterSheet>() != null)
        {
            bool isPickedup = other.gameObject.GetComponent<CharacterSheet>().Weapon_Pickup(weapon.damage, weapon.rateOfFire, weapon.range, weapon);
            if (isPickedup)
            {
                Destroy(gameObject);
            }
        }
    }
}
