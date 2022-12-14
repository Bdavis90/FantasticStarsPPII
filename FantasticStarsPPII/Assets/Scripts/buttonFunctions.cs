using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void resume()
    {
        if (gameManager.instance.isPaused)
        {
            gameManager.instance.isPaused = !gameManager.instance.isPaused;
            gameManager.instance.cursorUnlock();
        }
    }

    public void restart()
    {
        gameManager.instance.cursorUnlock();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void respawn()
    {
        gameManager.instance.playerScript.resetHP();
        gameManager.instance.playerScript.respawn();
        gameManager.instance.cursorUnlock();
    }

    public void quit()
    {
        Application.Quit();
    }
}
