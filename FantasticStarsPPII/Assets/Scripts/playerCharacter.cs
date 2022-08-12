using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**************************************************************/
/*         Player Specific Character Sheet Behaviors          */
/**************************************************************/
public class playerCharacter : MonoBehaviour, ICharacterDirector
{


    public void onDeath()
    {
        //Debug.Log("Player Died");
        //StartCoroutine(DelayedPlayerSpawn());

        gameManager.instance.cursorLock();
        gameManager.instance.currentMenuOpen = gameManager.instance.playerDeadMenu; ;
        gameManager.instance.currentMenuOpen.SetActive(true);
    }

    public void onHit()
    {
        Debug.Log("Player Took Damage");
        StartCoroutine(HUDFlash());
    }

    public void onShoot(WeaponStats _equippedWeapon)
    {
        GameObject projectile = Instantiate(_equippedWeapon.projectilePrefab, Camera.main.transform.position + (transform.forward * 2), Camera.main.transform.rotation);
        projectile.GetComponent<projectile_StaticMotion>().SetWeaponStats(_equippedWeapon);
    }
    private IEnumerator DelayedPlayerSpawn()
    {

        yield return new WaitForSeconds(3);
        GetComponent<CharacterSheet>().ResetHealth();
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 1);
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.z);
        GetComponent<CharacterController>().enabled = false;
        //transform.position = new Vector3(0, 0.8f, 0); //TODO::Update to GameManager respawn point
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
