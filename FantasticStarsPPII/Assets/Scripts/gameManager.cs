using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public ushort spawnID = 0;
    public Dictionary<ushort, EntityManager> entitySpawns = new Dictionary<ushort, EntityManager>(100);

    private void Awake()
    {
        instance = this;
        
    }

    /*******************************************/
    /*        spawn Dictionary Methods         */
    /*******************************************/
    #region Dictionary Methods
    public ushort GenerateCharacterID()
    {
        bool checkID = true;
        ushort IDresult = 0;


        while (checkID)
        {
            //0 is Reserved for Null Reference
            if (spawnID == 0)
            {
                spawnID++;
            }
            //Loop Until a used ID is found. Maximum Existing Game = 65535
            if (entitySpawns.ContainsKey(spawnID))
            {
                spawnID++;
            }
            else
            {
                IDresult = spawnID;
                checkID = false;
            }
        }        
        return IDresult;
    }
    public void AddCharacter_to_GameManager(ushort _key, EntityManager _objectPair)
    {
        entitySpawns.Add(_key, _objectPair);
    }

    public bool ContainsSpawn(ushort _ID)
    {
        return entitySpawns.ContainsKey(_ID);
    }

    public Vector3 GetIDPosition(ushort _ID)
    {
        return entitySpawns.GetValueOrDefault(_ID).GetGameObject().transform.position;
    }
    #endregion
}
