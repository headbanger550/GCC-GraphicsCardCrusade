using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public enum bulletTipe
{
    playerB,
    enemyB
}

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] bulletTipe bTipe;

    [Space]

    [SerializeField] LayerMask layerToDamage;

    [Space]

    [SerializeField] float detectionRange;
    [SerializeField] float spherecastRange;

    [Space]

    [SerializeField] GameObject impactEffect;

    [Space]

    [SerializeField] float damage;
    [SerializeField] float explosiveDamage;
    [SerializeField] float impactForce;

    [Space]

    [SerializeField] bool shouldCollide;
    [SerializeField] bool shouldRaycast;
    [SerializeField] bool shouldSphereCast;

    [Space]

    [SerializeField] bool shouldStopInPlace;

    [Space]

    [SerializeField] bool isBouncy;
    [SerializeField] float bounceDuration;

    [SerializeField] bool IsExplosive;
    [SerializeField] float explosionRadius;

    [Space]

    [SerializeField] bool isPearcing;
    [SerializeField] int pierceCount;
    private int currentPierceCount = 0;

    [Space]

    [SerializeField] bool isParticle;

    // Start is called before the first frame update
    void Awake()
    {
        //this.GetComponent<MeshFilter>().mesh = bulletGraphics;

        if(shouldRaycast)
        {
            Collider gotCollider = this.GetComponent<Collider>();
            if(gotCollider != null)
                gotCollider.enabled = false;
        }
        else
        {
            Collider gotCollider = this.GetComponent<Collider>();
            if(gotCollider != null)
                gotCollider.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(shouldRaycast)
        {
            RaycastHit _hit;
            if(Physics.Raycast(transform.position, transform.forward, out _hit, detectionRange))
            {
                MoveRigidBodies(_hit);

                if(shouldStopInPlace)
                {
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.velocity = new Vector3(0f, 0f, 0f);
                    }
                }

                if(isBouncy)
                {
                    if(_hit.transform.gameObject.layer == layerToDamage)
                    {
                        DamageEnemy(_hit);

                        if(IsExplosive)
                        {
                            ExplosiveBullets();
                        }
                    }
                }

                if(isPearcing)
                {
                    if(_hit.transform.gameObject.layer == layerToDamage)
                    {
                        if(currentPierceCount < pierceCount)
                        {
                            DamageEnemy(_hit);
                            currentPierceCount++;
                        }
                        else
                        {
                            Destroy(gameObject);
                        }
                    }
                }

                DamageEnemy(_hit);

                if(IsExplosive)
                {
                    ExplosiveBullets();
                }

                GameObject instObj = Instantiate(impactEffect, _hit.point, Quaternion.LookRotation(_hit.normal));
                Destroy(instObj, 2f);

                if(!isPearcing)
                    Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject, 3f);
            }
        }
        
        if(shouldSphereCast)
        {
            RaycastHit _hit;
            if(Physics.SphereCast(transform.position, spherecastRange, transform.forward, out _hit, detectionRange))
            {
                MoveRigidBodies(_hit);

                if(shouldStopInPlace)
                {
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.velocity = new Vector3(0f, 0f, 0f);
                    }
                }

                if(isBouncy)
                {
                    if(_hit.transform.gameObject.layer == layerToDamage)
                    {
                        DamageEnemy(_hit);

                        if(IsExplosive)
                        {
                            ExplosiveBullets();
                        }
                    }
                }

                if(IsExplosive)
                {
                    ExplosiveBullets();
                }

                DamageEnemy(_hit);


                GameObject instObj = Instantiate(impactEffect, _hit.point, Quaternion.LookRotation(_hit.normal));
                Destroy(instObj, 2f);

                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject, 3f);
            }

        }

        if(isBouncy)
        {
            StartCoroutine(BounceAround());  
        }
    }

    void OnTriggerEnter(Collider coll) 
    {
        if(shouldCollide)
        {
            if(coll.gameObject == null)
            {
                Destroy(gameObject, 2f);
            }
            else
            {
                if(shouldStopInPlace)
                {
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.velocity = new Vector3(0f, 0f, 0f);
                    }
                }


                if(IsExplosive)
                {
                    ExplosiveBullets();
                }

                Enemy gotEnemie = coll.transform.GetComponent<Enemy>();
                if(gotEnemie != null)
                {
                    gotEnemie.DamageEnemy(damage);
                }


                GameObject instObj = Instantiate(impactEffect, coll.transform.position, Quaternion.identity);
                Destroy(instObj, 2f);

                Destroy(gameObject);
            }
        }
    }

    IEnumerator DamageEnemy(RaycastHit hit)
    {
        Enemy gotEnemie = hit.transform.GetComponent<Enemy>();
        if(gotEnemie != null)
        {
            gotEnemie.DamageEnemy(damage);
            CameraShaker.Instance.ShakeOnce(2, 8, 0, 0.5f);

            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(0.1f);
            Time.timeScale = 1;
        }
    }

    void ExplosiveBullets()
    {
        Collider[] gotColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach(Collider damageObject in gotColliders)
        {
            Enemy enemies = damageObject.GetComponent<Enemy>();
            if(enemies != null)
            {
                Debug.Log("enemies got");
                enemies.DamageEnemy(explosiveDamage);
            }

            Rigidbody rb = damageObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.AddExplosionForce(5000, transform.position, explosionRadius);
            }
        }
    }

    void MoveRigidBodies(RaycastHit hit)
    {
        Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.AddForce(-hit.normal * impactForce);
        }
    }

    IEnumerator BounceAround()
    {
        yield return new WaitForSeconds(bounceDuration);
        if(IsExplosive)
        {
            ExplosiveBullets();
        }
    }

    
    void OnDrawGizmosSelected()     
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
