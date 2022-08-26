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
        gameManager.instance.cursorLock();
        gameManager.instance.currentMenuOpen = gameManager.instance.playerDeadMenu; ;
        gameManager.instance.currentMenuOpen.SetActive(true);

    }

    public void onHit()
    {
        
        StartCoroutine(HUDFlash());
    }

    public void onShoot(WeaponStats _equippedWeapon)
    {
        GameObject projectile = Instantiate(_equippedWeapon.projectilePrefab, Camera.main.transform.position + (transform.forward * 2), Camera.main.transform.rotation);
        projectile.GetComponent<projectile_StaticMotion>().SetWeaponStats(_equippedWeapon);
    }
   

    private IEnumerator HUDFlash()
    {
        gameManager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageFlash.SetActive(false);
    }
}
