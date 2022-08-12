using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCharacter : MonoBehaviour, ICharacterDirector
{
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
        GetComponent<CharacterSheet>().ResetHealth();
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 1);
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.z);
        GetComponent<CharacterController>().enabled = false;
        transform.position = new Vector3(0, 0.8f, 0); //TODO::Update to GameManager respawn point
        GetComponent<CharacterController>().enabled = true;
        GetComponent<CharacterSheet>().isAlive = true;
        gameManager.instance.AddCharacter_to_GameManager(GetComponent<CharacterSheet>().GetSpawnID(), new CharacterManager(gameObject, GetComponent<CharacterSheet>()));
    }

    private IEnumerator HUDFlash()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }
}
