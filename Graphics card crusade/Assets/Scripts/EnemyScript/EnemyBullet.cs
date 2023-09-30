using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float detectionRange;
    [SerializeField] float detectionSize;
    [SerializeField] LayerMask noHitMask;

    [Space]

    [SerializeField] bool isFromAbove;
    [SerializeField] GameObject otherBullet;
    [SerializeField] Vector3 abovePos;
    [SerializeField] GameObject warningRing;

    [Space]

    [SerializeField] bool isParticle;
    [SerializeField] GameObject groundHitVFX;
    [SerializeField] GameObject hitVFX;

    private Transform player;

    //I wanna fucking kill myself
    private GameObject ringObj;
    private GameObject bulletObj;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable() 
    {
        player = FindObjectOfType<PlayerMovement>().transform;
        if(isFromAbove && !isParticle)
        {
            StartCoroutine(AirBullet());
        }
    }

    // Update is called once per frame
    void Update()
    {
        noHitMask = ~noHitMask;

        if(!isFromAbove && !isParticle)
        {
            RaycastHit _hit;
            if(Physics.SphereCast(transform.position, detectionSize, transform.forward, out _hit, detectionRange, noHitMask))
            {
                InstantiateVFX(hitVFX, _hit.point);
                PlayerMovement player = _hit.transform.GetComponent<PlayerMovement>();
                if(player != null)
                {
                    player.DamagePlayer(damage);
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject, 3f);
            }
        }
    }

    //I couldn't find a better way to do this sooooooooooooooooo yea fucking kill me :)
    IEnumerator AirBullet()
    {   
        ringObj = Instantiate(warningRing, player.position, warningRing.transform.localRotation);  

        yield return new WaitForSeconds(2f);
        Destroy(ringObj);

        bulletObj = Instantiate(otherBullet, ringObj.transform.position + abovePos, otherBullet.transform.localRotation);
            
        Destroy(bulletObj, 1.5f);
        
    }

    void OnParticleCollision(GameObject other) 
    {
        if(isFromAbove)
        {
            RaycastHit _hit;
            if(Physics.SphereCast(transform.position, detectionSize, Vector3.down, out _hit, detectionRange, noHitMask))
            {
                CameraShaker.Instance.ShakeOnce(5f, 2f, 0.1f, 2f);

                InstantiateVFX(groundHitVFX, _hit.point);
                InstantiateVFX(hitVFX, _hit.point);

                PlayerMovement player = _hit.transform.GetComponent<PlayerMovement>();
                if(player != null)
                {
                    player.DamagePlayer(damage);
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            PlayerMovement player1 = other.GetComponentInChildren<PlayerMovement>();
            if(player1 != null)
            {
                player1.DamagePlayer(damage);
                Debug.Log("Got da player");

                InstantiateVFX(hitVFX, other.transform.position);
                Destroy(gameObject);
            }
        }
    }

    void InstantiateVFX(GameObject vfx, Vector3 position)
    {
        GameObject instOBJ = Instantiate(vfx, position, Quaternion.identity);
        Destroy(instOBJ, 1.5f);
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionSize);    
    }
}
