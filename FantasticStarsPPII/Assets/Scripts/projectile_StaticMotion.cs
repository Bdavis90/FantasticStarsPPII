using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile_StaticMotion : MonoBehaviour
{
    
    [SerializeField] public WeaponStats weapon;
    [SerializeField] float rangeDestruction;

    public void SetWeaponStats(WeaponStats _weapon)
    {
        weapon = _weapon;
        rangeDestruction = weapon.range;
    }


    void Update()
    {
        transform.position += transform.forward * weapon.bulletSpeed * Time.deltaTime;
        rangeDestruction -= Time.deltaTime;
        if(rangeDestruction < 0)
        {
            Destroy(gameObject);
        }
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
}
