using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public GameObject player;
    public playerController playerScript;

    public GameObject pauseMenu;
    public GameObject currentMenuOpen;

    public GameObject playerDamageFlash;
    public GameObject playerDeadMenu;


    public bool isPaused;
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    // Update is called once per frame
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
}
