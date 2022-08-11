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
    [SerializeField] List<GameObject> mainHand = new List<GameObject>();

    [Header("----- Weapon -----")]
    [SerializeField] bool isShooting;
    [Range(1, 60)] [SerializeField] int rateOfFire;
    [SerializeField] float weaponRange;
    [SerializeField] float weaponDamage;

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

            if (health <= 0)
            {
                gameManager.instance.character_Spawns.Remove(spawnID);
                isAlive = false;
                manager.onDeath();
            }
            else
            {
                manager.onHit();
            }
        }
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

