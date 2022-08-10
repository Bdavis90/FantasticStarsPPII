using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager// : MonoBehaviour
{

    public GameObject characterSpawn;
    public Entity spawnType;
    

    public EntityManager(GameObject _gameObject, Entity _entity)
    {
        characterSpawn = _gameObject;
        spawnType = _entity;

    }
    
    public GameObject GetGameObject()
    {
        return characterSpawn;
    }

    public Entity GetEntity()
    {
        return spawnType;
    }
}
