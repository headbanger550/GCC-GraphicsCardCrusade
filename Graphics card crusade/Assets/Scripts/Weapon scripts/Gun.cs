using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Gun : ScriptableObject
{
    public new string name;

    [Space]

    public GameObject[] impactEffects;
    public GameObject[] enemyImpactEffects;
    public bool hasShotLine;

    [Space]

    public GameObject bullet;
    public float bulletSpeed = 300f;

    [Space]

    public float damage;  
    public float fireRate;
    public float impactForce;

    [Space]

    public bool isRyacast;
    public bool isProjectile;
    public bool isParticleBased;

    [Space]

    public bool hasSpread;

    [Space]

    public LayerMask layerToDamage;
}
