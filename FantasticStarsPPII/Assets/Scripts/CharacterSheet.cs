using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour, IDamageable
{
    [Header("----- Adjustable Fields -----")]
    public Entity_Class classType;
    public Entity_Faction faction;
    public bool isAlive { get; set; }

    [Header("----- Object Manager -----")]
    [SerializeField] ushort spawnID;
    //[SerializeField] string objectName = null;

    [Header("----- Attributes -----")]
    [SerializeField] int baseMax;
    [SerializeField] int health;

    [Header("----- Inventory -----")]
    [SerializeField] WeaponStats rightHand = null;
    [SerializeField] List<WeaponStats> gunBag = new List<WeaponStats>();
    

    [Header("----- Weapon -----")]
    [SerializeField] bool isShooting;
    [SerializeField] float weaponDamage;
    [Range(1, 60)] [SerializeField] int weaponFireRate;
    [SerializeField] float weaponRange;


    void Start()
    {
        
        //Generate Unique character ID and Add to gameManager
        spawnID = gameManager.instance.GenerateCharacterID();
        //objectName = gameObject.name + spawnID;
        //Intiialize Properties
        isAlive = true;
        gameManager.instance.AddCharacter_to_GameManager(spawnID, new CharacterManager(gameObject, this));

    }

    public ushort GetSpawnID()
    {
        return spawnID;
    }

    public void ResetHealth()
    {
        health = baseMax;
    }

    public void takeDamage(int _damage)
    {
        health -= _damage;
        if (GetComponent<ICharacterDirector>() != null)
        {
            ICharacterDirector manager = GetComponent<ICharacterDirector>();
            manager.onHit();
            if (health <= 0)
            {
                gameManager.instance.character_Spawns.Remove(spawnID);
                isAlive = false;
                manager.onDeath();
            }
        }
    }

    public bool Weapon_Pickup(int _weaponDamage, int _rateOfFire, float _weaponRange, WeaponStats _weapon)
    {
        bool isPickedup = false;
        if(rightHand == null)
        {
            rightHand = _weapon;
            SwapWeapons(_weapon);
            isPickedup = true;
        }
        
        if (!gunBag.Contains(_weapon))
        {
            gunBag.Add(_weapon);
            isPickedup = true;
        }
        return isPickedup;
    }

    public void SwapWeapons(WeaponStats _Weapon)
    {
        weaponDamage = _Weapon.damage;
        weaponFireRate = _Weapon.rateOfFire;
        weaponRange = _Weapon.range;
    }
}



public enum Entity_Class
{
    None = 0,
    Warrior = 10,
    Mystic = 12,
    Archer = 13
}

public enum Entity_Faction
{
    Dwarves = 100,
    Orglings = 200
}

