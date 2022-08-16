using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour, IDamageable
{
    // player and AI attribiutes derive from the Character Sheet ie health and weapons


    [Header("----- Adjustable Fields -----")]
    public Entity_Class classType;

    //faction dictates if the AI will kill the player aka team
    public Entity_Faction faction; 
    public bool isAlive { get; set; }
    
    [Header("----- Object Manager -----")]
    //for the dictionary - at spawn character is given a unique id for AI to find AI
    [SerializeField] ushort spawnID;

    [Header("----- Attributes -----")]
    [SerializeField] int baseHealth;
    [SerializeField] int health;

    [Header("----- Inventory -----")]
    // this means what weapon is equipped
    [SerializeField] public WeaponStats rightHand = null;
    //[SerializeField] public WeaponStats leftHand = null;
    // this is where weapons are stored aka inventory
    [SerializeField] public List<WeaponStats> gunBag = new List<WeaponStats>();
    [SerializeField] public int gunBagIterator;

    [Header("----- Weapon -----")]
    [SerializeField] bool isShooting;
    //public bool openGate = true;

    void Start()
    {
        
        //Generate Unique character ID and Add to gameManager
        spawnID = gameManager.instance.GenerateCharacterID();
        //objectName = gameObject.name + spawnID;
        //Intiialize Properties
        isAlive = true;
        gameManager.instance.AddCharacter_to_GameManager(spawnID, new CharacterManager(gameObject, this));

    }

    public void IterateGunBagIterator(int _iteration)
    {
        gunBagIterator++;
        if(gunBag.Count != 0)
        {
            if(gunBagIterator >= gunBag.Count)
            {
                gunBagIterator = 0;
            }
            if(gunBagIterator < 0)
            {
                gunBagIterator = gunBag.Count - 1;
            }
            rightHand = gunBag[gunBagIterator];
        }
    }

    

    public ushort GetSpawnID()
    {
        return spawnID;
    }

    public void ResetCharacter()
    {
        health = baseHealth;
        StartCoroutine(addPlayerToDictionary());

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
                //Remove all Dead Characters from Dictionary -- Moved to AICharacter Script
                gameManager.instance.character_Spawns.Remove(spawnID);
                isAlive = false;
                manager.onDeath();
            }
        }
    }

    public void ShootWeapon()
    {

        StartCoroutine(FireWeapon());     
      
    }
    public IEnumerator addPlayerToDictionary()
    {
            yield return new WaitForSeconds(1);
            gameManager.instance.AddCharacter_to_GameManager(spawnID, new CharacterManager(gameObject, this));
            isAlive = true;
    }

    IEnumerator FireWeapon()
    {
        if (!isShooting && (rightHand != null))
        {
            
            isShooting = true;
            if (GetComponent<ICharacterDirector>() != null)
            {
                for (int i = 0; i < rightHand.shotQuantity; i++)
                {
                    //Pass equippedWeapon in Parameters
                    GetComponent<ICharacterDirector>().onShoot(rightHand);
                }

            }
            yield return new WaitForSeconds(1 / rightHand.rateOfFire);
            isShooting = false;
        }  
    }

    public bool Weapon_Pickup(WeaponStats _weapon)
    {
        bool isPickedup = false;
        if(rightHand == null)
        {
            
            rightHand = _weapon;
            //SwapWeapons(_weapon);
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
        //weaponDamage = _Weapon.damage;
        //weaponFireRate = _Weapon.rateOfFire;
        //weaponRange = _Weapon.range;
    }

    public void healthPickUp(int Hp)
    {
        health += Hp;
        gameManager.instance.playerHPBar.fillAmount = (float)health / (float)baseHealth;
    }
    public int HPCheck()
    {
        return health;
    }

    public int MaxHPCheck()
    {
        return baseHealth;
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

