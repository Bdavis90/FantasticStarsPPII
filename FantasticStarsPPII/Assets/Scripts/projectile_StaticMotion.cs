using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile_StaticMotion : MonoBehaviour
{
    
    [SerializeField] public WeaponStats weapon;

    public void SetWeaponStats(WeaponStats _weapon)
    {
        weapon = _weapon;
    }

    private void Start()
    {

        StartCoroutine(AutoDestruction());
        Instantiate(weapon.effectsPrefab, gameObject.transform);

        float randomRange = weapon.accuracyScale - (weapon.accuracyScale * weapon.accuracy);
        float ranx = Random.Range(-randomRange, randomRange);
        float rany = Random.Range(-randomRange, randomRange);
        transform.Rotate(Vector3.up * rany);
        transform.Rotate(Vector3.right * ranx);
        Instantiate(weapon.effectsPrefab, gameObject.transform);
    }

    void Update()
    {
        transform.position += transform.forward * weapon.bulletSpeed * Time.deltaTime;
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().takeDamage(weapon.damage);
            
        }
        Destroy(gameObject);

    }

    IEnumerator AutoDestruction()
    {
        yield return new WaitForSeconds(weapon.range);
        Destroy(gameObject);
    }
}
