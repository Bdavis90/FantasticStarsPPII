using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponStats : ScriptableObject
{
    [Header("----- Adjustable Fields -----")]
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public GameObject effectsPrefab;
    [Range(0, 10000)][SerializeField] public int damage;
    [Range(0.1f, 60)][SerializeField] public float rateOfFire;
    [Range(2, 15)][SerializeField] public float range;
    [Range(1, 1000)] [SerializeField] public int bulletSpeed;
    [Range(1, 60)][SerializeField] public int shotQuantity;
    [Range(0.01f, 1)] [SerializeField] public float accuracy;
    [Header("Static Fields, don't touch")]
    [SerializeField] public int accuracyScale = 30;

}
