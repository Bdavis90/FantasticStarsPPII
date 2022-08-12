using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    IEnumerator DestroyCorpse()
    {
        yield return new WaitForSeconds(corpseTimer);
        Destroy(gameObject);
    }
}
