using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerEvents : MonoBehaviour, IManagable
{
    //TODO:: Repsawn Timer can be here or in Game Manager
    public void onDeath()
    {
        Debug.Log("Player Died");
        StartCoroutine(DelayedPlayerSpawn());
    }

    public void onHit()
    {
        Debug.Log("Player Took Damage");
        StartCoroutine(HUDFlash());
    }

    private IEnumerator DelayedPlayerSpawn()
    {
        
        yield return new WaitForSeconds(3);
        GetComponent<Entity>().ResetHealth();
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 1);
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.z);
        GetComponent<CharacterController>().enabled = false;
        transform.position = new Vector3(0, 0.8f, 0); //TODO::Update to GameManager respawn point
        GetComponent<CharacterController>().enabled = true;
        GetComponent<Entity>().isAlive = true;
        gameManager.instance.AddCharacter_to_GameManager(GetComponent<Entity>().GetSpawnID(), new EntityManager(gameObject, GetComponent<Entity>()));
    }

    private IEnumerator HUDFlash()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }
}
