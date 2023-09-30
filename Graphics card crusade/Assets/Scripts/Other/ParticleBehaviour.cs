using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class ParticleBehaviour : MonoBehaviour
{
    [SerializeField] bool isExplosive;
    [SerializeField] float range;

    private Transform target;

    private float distanceToTarget;

    void OnEnable() 
    {
        target = FindObjectOfType<PlayerMovement>().transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        if(distanceToTarget <= range && isExplosive)
        {
            ExplodeOnPlayer();
        }
    }

    void ExplodeOnPlayer()
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.AddExplosionForce(800f, transform.position,50f);
    }
}
