using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    public Entity_Class classType;
    public Entity_Faction faction;
    //public Entity_Type type;

    [SerializeField] ushort spawnID;

    [Header("----- Attributes -----")]
    [SerializeField] bool isAlive;
    [SerializeField] int Health;

    void Start()
    {
        //Generate ID and Add to GameManager
        spawnID = gameManager.instance.GenerateCharacterID();

        
        
        gameManager.instance.AddCharacter_to_GameManager(spawnID, new EntityManager(gameObject, this));
        //if Spawn is a NPC
        if(GetComponent<EntityAI>() != null)
        {
            GetComponent<EntityAI>().SetSpawnID(spawnID);
        }
        
    }

    public ushort GetSpawnID()
    {
        return spawnID;
    }

    public bool GetAlive()
    {
        return isAlive;
    }
    public bool takeDamage(int _damage)
    {

        Health -= _damage;

        if(Health <= 0)
        {
            gameManager.instance.spawns.Remove(spawnID);
            isAlive = false;

            faction = Entity_Faction.Corpse;
            gameObject.name = "(Corpse)" + gameObject.name;
            if(GetComponent<EntityAI>() != null)
            {
                GetComponent<EntityAI>().SetAlive(false);
            }

            StartCoroutine(Corpse());
            //Simple Death Animation          
            //gameObject.transform.Rotate(Vector3.right * 90);
            gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
        }

        return isAlive;
    }

    IEnumerator Corpse()
    {
        
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
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
    Corpse = 0,
    Dwarves = 100,
    Orglings = 200
}

public enum Entity_Type
{
    Environment = 0,
    Person = 1,
    Ability = 2
}