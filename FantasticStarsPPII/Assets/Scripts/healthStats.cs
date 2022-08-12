using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class healthStats : ScriptableObject
{
    [Range(1, 20)] public int healthPoints;
}
