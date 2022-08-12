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
    }

    void Update()
    {
        transform.position += transform.forward * weapon.bulletSpeed * Time.deltaTime;
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet Collided with a gameObject");
        if(collision.gameObject.GetComponent<IDamageable>() != null)
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
