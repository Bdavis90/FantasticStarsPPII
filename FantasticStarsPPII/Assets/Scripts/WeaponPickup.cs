using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] WeaponStats weapon;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            bool isPickedup = other.gameObject.GetComponent<CharacterSheet>().Weapon_Pickup(weapon);
            if (isPickedup)
            {
                Destroy(gameObject);
            }
        }
    }
}
