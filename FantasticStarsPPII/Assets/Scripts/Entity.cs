using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [Header("----- Adjustable Fields -----")]
    public Entity_Class classType;
    public Entity_Faction faction;
    [SerializeField] bool isNPC;
    [SerializeField] bool isAlive;

    [Header("----- Object Manager -----")]
    [SerializeField] ushort spawnID;
    [SerializeField] string objectName = null;
    [SerializeField] float corpseTimer = 5f;

    [Header("----- Attributes -----")]
    [SerializeField] int healthMax;
    [SerializeField] int health;

    void Start()
    {
        //Generate ID and Add to GameManager
        spawnID = gameManager.instance.GenerateCharacterID();
        objectName = gameObject.name;
 
        gameManager.instance.AddCharacter_to_GameManager(spawnID, new EntityManager(gameObject, this));
        
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public ushort GetSpawnID()
    {
        return spawnID;
    }

    public void takeDamage(int _damage)
    {

        health -= _damage;

        if(health <= 0)
        {
            //If Dead, Remove from Game Manager spawns
            //This is the only Code that should delete from Game Manager
            OnCharacterDeath();
                
            //Simple Death Animation
            //gameObject.transform.Rotate(Vector3.right * 90);
            gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
      
        }
    }

    IEnumerator DestroyCorpse()
    {      
        yield return new WaitForSeconds(corpseTimer);
        Destroy(gameObject);
    }

    private void OnCharacterDeath()
    {
        gameManager.instance.entitySpawns.Remove(spawnID);
        isAlive = false;
        if (isNPC)
        {
            gameObject.name = "(Corpse)" + gameObject.name;
            StartCoroutine(DestroyCorpse());
        }
        else
        {
            StartCoroutine(DelayedPlayerSpawn());
        }
    }

    private IEnumerator DelayedPlayerSpawn()
    {
        health = healthMax;
        yield return new WaitForSeconds(3);
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 1);
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.z);
        GetComponent<CharacterController>().enabled = false;
        transform.position = new Vector3(0, 0.8f, 0); //TODO::Better Vector3 and Method
        GetComponent<CharacterController>().enabled = true;
        isAlive = true;
        gameManager.instance.AddCharacter_to_GameManager(spawnID, new EntityManager(gameObject, this));
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