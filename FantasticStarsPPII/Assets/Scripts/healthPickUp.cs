using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthPickUp : MonoBehaviour
{
    [SerializeField] healthStats healthStat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<CharacterSheet>().healthPickUp(healthStat.healthPoints);
            Destroy(gameObject);
        }
    }
}
