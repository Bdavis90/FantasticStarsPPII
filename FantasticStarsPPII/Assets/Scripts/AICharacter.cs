using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/**************************************************************/
/*        Enemy AI Specific Character Sheet Behaviors         */
/**************************************************************/


public class AICharacter : MonoBehaviour, ICharacterDirector
{
    [SerializeField] int corpseTimer = 5;
    public void onDeath()
    {
        gameObject.name = "(corpse)" + gameObject.name;
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
        StartCoroutine(DestroyCorpse());

    }

    public void onHit()
    {
        //Debug.Log("AI Took Damage");

    }

    public void onShoot(WeaponStats _equippedWeapon)
    {
        //if (GetComponent<NavMeshAgent>() != null)
        //{       
        //NavMeshAgent agent = GetComponent<NavMeshAgent>();
        //Vector3 test = agent.nextPosition - transform.position;
        //GameObject projectile = Instantiate(_equippedWeapon.projectilePrefab, transform.position + (transform.forward * 2), Quaternion.LookRotation(test));
        //projectile.GetComponent<projectile_StaticMotion>().SetWeaponStats(_equippedWeapon);
        //}

        //TODO:: Get Weapon Container Gameobject forplayer. Instantiate bullet at that location
        if (GetComponent<AINavMeshController>() != null)
        {

            Vector3 direction;
            if (GetComponent<AINavMeshController>().TryGetTarget(out direction))
            {
                GameObject projectile = Instantiate(_equippedWeapon.projectilePrefab, transform.position + (transform.forward * 2), Quaternion.LookRotation(direction));
                projectile.GetComponent<projectile_StaticMotion>().SetWeaponStats(_equippedWeapon);
            }



        }


    }

    IEnumerator DestroyCorpse()
    {
        yield return new WaitForSeconds(corpseTimer);
        Destroy(gameObject);
    }
}
