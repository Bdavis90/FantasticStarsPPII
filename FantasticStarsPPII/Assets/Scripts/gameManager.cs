using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public GameObject player;
    public playerController playerScript;

    public GameObject pauseMenu;
    public GameObject currentMenuOpen;

    public GameObject playerDamageFlash;
    public GameObject playerDeadMenu;

    public static gameManager instance;

    public ushort spawnID = 0;
    //public Dictionary<ushort, EntityManager> entitySpawns = new Dictionary<ushort, EntityManager>(100);
    public Dictionary<ushort, CharacterManager> character_Spawns = new Dictionary<ushort, CharacterManager>(100);


    public bool isPaused;
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            isPaused = !isPaused;
            currentMenuOpen = pauseMenu;
            currentMenuOpen.SetActive(isPaused);

            if (isPaused)
                cursorLock();
            else if (currentMenuOpen)
                cursorUnlock();
        }
    }

    public void cursorLock()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0;
    }

    public void cursorUnlock()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        currentMenuOpen.SetActive(isPaused);
        currentMenuOpen = null;
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
            if (character_Spawns.ContainsKey(spawnID))
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
    public void AddCharacter_to_GameManager(ushort _key, CharacterManager _objectPair)
    {
        character_Spawns.Add(_key, _objectPair);
    }
    public bool ContainsSpawn(ushort _ID)
    {
        return character_Spawns.ContainsKey(_ID);
    }
    public Vector3 GetIDPosition(ushort _ID)
    {
        return character_Spawns.GetValueOrDefault(_ID).GetGameObject().transform.position;
    }
    #endregion
}

