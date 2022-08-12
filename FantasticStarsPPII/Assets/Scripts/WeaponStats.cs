using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponStats : ScriptableObject
{
    [SerializeField] public GameObject projectilePrefab;
    [Range(0, 10000)][SerializeField] public int damage;
    [Range(1, 60)][SerializeField] public float rateOfFire;
    [Range(2, 3000)][SerializeField] public float range;
    [Range(1, 1000)] [SerializeField] public int bulletSpeed;

    private void Awake()
    {
        rateOfFire = 1 / rateOfFire;
    }
}
