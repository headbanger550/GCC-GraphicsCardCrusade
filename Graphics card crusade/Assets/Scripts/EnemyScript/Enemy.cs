using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] bool hasAnimations;
    [SerializeField] bool isDrone;
    [SerializeField] bool isStill;
    [SerializeField] bool hasIdle;

    [SerializeField] bool isCoruptable;
    [SerializeField] GameObject coruptedVariant;

    [Space]

    [SerializeField] bool doesSpot;
    [SerializeField] LayerMask obstructionMask;
    [SerializeField] LayerMask enemyMask;

    [Space]

    [Tooltip("1-melle, 2-ranged, 3-Corruptor")]
    [SerializeField] int enemyTypeID;

    [Header("For ranged enemey")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bullet;
    [SerializeField] float bulletSpeed;
    [SerializeField] float fireRate;
    [SerializeField] GameObject onAttackEffect;

    [Space]

    [SerializeField] float chaseRange = 5f;
    [SerializeField] float turnSpeed = 5f;

    [Space]

    [SerializeField] float health;
    [SerializeField] float damage;

    [Space]

    public GameObject[] enemyHitParticles;
    [SerializeField] GameObject gibs;
    [SerializeField] bool isParticle;
    [SerializeField] bool isExplosive;

    [Space]

    [SerializeField] float shockTime;

    [SerializeField] Transform target;
    private Transform player;
    private float distanceToTarget = Mathf.Infinity;
    public bool isProvoked;

    private bool canDamage = true;

    private Vector3 destination;
    private float nextTimeToFire;

    private NavMeshAgent nevMesh;
    private Animator animator;

    void OnEnable() 
    {
        if(!hasIdle)
        {
            isProvoked = true;
        }
    }

    void Start()
    {
        nevMesh = GetComponent<NavMeshAgent>();
        if(hasAnimations)
        {
            animator = GetComponent<Animator>();
        }
        else if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        player = FindObjectOfType<PlayerMovement>().transform;

        if(enemyTypeID != 3)
            target = player;
    }

    void Update()
    {   
        if(enemyTypeID == 3 && (target == null || target == player))
        {
            GoToEnemy();
        }

        if(doesSpot)
            SpotTarget();
        
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        destination = target.position;

        if(isProvoked && target != null)
        {   
            if(hasAnimations && hasIdle)
                StartCoroutine(GetShocked());

            EngageTarget();
        }

        else if(distanceToTarget <= chaseRange)
        {
            isProvoked = true;
        }
    }

    public void OnDamageTaken()
    {
        isProvoked = true;
    }

    void SpotTarget()
    {
        if(!Physics.Raycast(transform.position, transform.position - target.position, obstructionMask))
        {
            isProvoked = false;
        }
        else
            isProvoked = true;
    }

    void EngageTarget()
    {
        FaceTarget();

        if(enemyTypeID == 2 && isStill)
        {
            AttackTarget();
        }
        
        if(distanceToTarget >= nevMesh.stoppingDistance && !isStill)
        {
            ChaseTarget();
        }
        else if(distanceToTarget <= nevMesh.stoppingDistance && !isStill)
        {
            AttackTarget();
        }

    }

    void ChaseTarget()
    {   
        if(hasAnimations)
        {
            animator.SetTrigger("Move");
            animator.SetBool("Attack", false);
        }
        nevMesh.SetDestination(target.position);
    }

    IEnumerator GetShocked()
    {
        animator.SetTrigger("Shocked");
        yield return new WaitForSeconds(shockTime);
    }  

    void GoToEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, chaseRange, enemyMask);
        if(enemies.Length > 0)
        {
            for(int i = 0; i < enemies.Length; i++)
            {
                int randEnemie = Random.Range(0, enemies.Length);
                target = enemies[randEnemie].transform;
            }
        }
        else
        {
            target = player;
        }
    } 

    void AttackTarget()
    {
        if(hasAnimations)
            animator.SetBool("Attack", true);

        if(isDrone)
        {
            AttackEvent();
            DamageEnemy(health + 10);
        }
    }

    public void AttackEvent()
    {   
        if(onAttackEffect != null)
        {
            GameObject instOBJ = Instantiate(onAttackEffect, firePoint.position, onAttackEffect.transform.rotation);
            Destroy(instOBJ, 1f);
        }

        PlayerMovement player = target.GetComponent<PlayerMovement>();
        if(player != null && enemyTypeID == 1)
        {
            player.DamagePlayer(damage);
            Rigidbody pRB = player.GetComponent<Rigidbody>();
            if(pRB != null)
            {
                pRB.AddForce(transform.forward * 10000f * Time.deltaTime, ForceMode.Impulse);
            }
            Debug.Log("Player damaged");
        }

        if(enemyTypeID == 2)
        {   
            RangedAttack();
        }
    }

    void RangedAttack()
    {
        if(Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            InstantiateProjectile(firePoint, bullet, firePoint.rotation);
        }
    }
    void InstantiateProjectile(Transform shotPoint, GameObject bullet, Quaternion rotation)
    {
        GameObject projectileObj = Instantiate(bullet, shotPoint.position, rotation);
        projectileObj.GetComponent<Rigidbody>().velocity = (destination - shotPoint.position).normalized * bulletSpeed;
        
        if(projectileObj.GetComponent<Rigidbody>() == null)
        {
            projectileObj.GetComponentInChildren<Rigidbody>().velocity = (destination - shotPoint.position).normalized * bulletSpeed;
        }
    }

    public void DamageEnemy(float playerDamage)
    {
        if(canDamage)
        {
            OnDamageTaken();
            health -= playerDamage;
            if(health <= 0)
            {
                Die();
            }
            if(health <= -15)
            {
                Gib();
                Debug.Log("ded");
                Destroy(gameObject);
            }
        }
    }

    void Die()
    {
        canDamage = false;
        if(hasAnimations)
        {
            animator.SetTrigger("Ded");
        }
        nevMesh.enabled = false;
        this.enabled = false;
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized; // we get the dirrection to the player
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); // we make a look rotation so we now were to rotate if i'm not mistaken
        transform.rotation = Quaternion.Slerp(transform.localRotation, lookRotation, Time.deltaTime * turnSpeed); // we smoothly(and spherecly rotate) towards the player
    }

    void Gib()
    {
        GameObject instGibs = Instantiate(gibs, transform.position, Quaternion.identity);
        if(!isParticle)
        {
            Rigidbody[] rbs = instGibs.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody gib in rbs)
            {
                gib.AddExplosionForce(5f, transform.position, 15f);
            }
        }
        else
        {
            Destroy(instGibs, 0.8f);
        }
    }

    private void OnTriggerEnter(Collider coll) 
    {
        if(enemyTypeID == 3)
        {
            Enemy colidedEnemie = coll.GetComponent<Enemy>();
            if(colidedEnemie.isCoruptable == true && colidedEnemie != null)
            {
                GameObject corrEnemie = Instantiate(colidedEnemie.coruptedVariant, coll.transform.position, Quaternion.identity);
                //corrEnemie.GetComponent<Enemy>().isProvoked = true;

                Destroy(coll.gameObject);
                Destroy(gameObject);
            }
        }
    }

    

    private void OnDrawGizmosSelected() 
    {
        //To be provoked radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
