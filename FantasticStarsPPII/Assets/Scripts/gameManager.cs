using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public ushort spawnID = 0;
    public Dictionary<ushort, EntityManager> entitySpawns = new Dictionary<ushort, EntityManager>(100);
    public GameObject player;
    public playerController playerScript;

    private void Awake()
    {
        instance = this;
    public GameObject pauseMenu;
    public GameObject currentMenuOpen;
        
    }
    public GameObject playerDamageFlash;
    public GameObject playerDeadMenu;

    /*******************************************/
    /*        spawn Dictionary Methods         */
    /*******************************************/
    #region Dictionary Methods
    public ushort GenerateCharacterID()
    {
        bool checkID = true;
        ushort IDresult = 0;


        while (checkID)
    public bool isPaused;
    void Awake()
        {
            //0 is Reserved for Null Reference
            if (spawnID == 0)
            {
                spawnID++;
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
            }
            //Loop Until a used ID is found. Maximum Existing Game = 65535
            if (entitySpawns.ContainsKey(spawnID))

    // Update is called once per frame
    void Update()
            {
                spawnID++;
            }
            else
        if (Input.GetButtonDown("Cancel"))
            {
                IDresult = spawnID;
                checkID = false;
            }
        }        
        return IDresult;
            isPaused = !isPaused;
            currentMenuOpen = pauseMenu;
            currentMenuOpen.SetActive(isPaused);

            if (isPaused)
                cursorLock();
            else if (currentMenuOpen)
                cursorUnlock();
    }
    public void AddCharacter_to_GameManager(ushort _key, EntityManager _objectPair)
    {
        entitySpawns.Add(_key, _objectPair);
    }

    public bool ContainsSpawn(ushort _ID)
    public void cursorLock()
    {
        return entitySpawns.ContainsKey(_ID);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0;
    }

    public Vector3 GetIDPosition(ushort _ID)
    public void cursorUnlock()
    {
        return entitySpawns.GetValueOrDefault(_ID).GetGameObject().transform.position;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        currentMenuOpen.SetActive(isPaused);
        currentMenuOpen = null;
    }
    #endregion
}
