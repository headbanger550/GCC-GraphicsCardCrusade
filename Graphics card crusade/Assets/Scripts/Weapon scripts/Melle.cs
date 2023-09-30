using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Melle : MonoBehaviour
{
    [SerializeField] KeyCode punchKey;
    [SerializeField] KeyCode chargeKey;

    [Space]

    [SerializeField] float hitRange;
    [SerializeField] float punchDamage;
    [SerializeField] float shootDamage;

    [Space]

    [SerializeField] float shakeMagnitude;
    [SerializeField] float shakeRoughness;

    [Space]

    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impactEffect;

    private int randomNum;

    private RaycastHit _hit;
    private bool isPunching;

    private Animator animator;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        cam = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        if(Input.GetKeyDown(punchKey))
        {
            isPunching = true;
            randomNum = Random.Range(0, 5);

            animator.SetBool("Normal punch", true);
            animator.SetFloat("Blend", randomNum);

            yield return new WaitForSeconds(0.6f);

            animator.SetBool("Normal punch", false);
            isPunching = false;
        }

        if(Input.GetKey(chargeKey) && !isPunching)
        {
            animator.SetBool("Shoot charge", true);
        }
        else if(Input.GetKeyUp(chargeKey))
        {
            animator.SetBool("Shoot discharge", true);

            yield return new WaitForSeconds(0.3f);

            animator.SetBool("Shoot charge", false);
            animator.SetBool("Shoot discharge", false);
        }
    }

    public void PunchHit()
    {
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, 0.3f, 0.1f);

        if(Physics.SphereCast(cam.transform.position, hitRange, cam.transform.forward, out _hit, 2f))
        {
           StartCoroutine(Effects(impactEffect));

            Enemy enemy = _hit.transform.GetComponent<Enemy>();
            if(enemy == null)
            {
                enemy = _hit.transform.GetComponentInChildren<Enemy>();
                if(enemy != null)
                {
                    enemy.DamageEnemy(punchDamage);
                }
            }
            else
            {
                enemy.DamageEnemy(punchDamage);
            }
        }
    }

    
    public void PunchShoot()
    {
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, 0.3f, 0.1f);
        MuzzleFlash();

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, 10f))
        {
            StartCoroutine(Effects(impactEffect));

            Enemy enemy = _hit.transform.GetComponent<Enemy>();
            if(enemy == null)
            {
                enemy = _hit.transform.GetComponentInChildren<Enemy>();
                if(enemy != null)
                {
                    enemy.DamageEnemy(shootDamage);
                }
            }
            else
            {
                enemy.DamageEnemy(shootDamage); 
            }
        }
    }

    IEnumerator Effects(GameObject impctVFX)
    {
        Debug.Log("BOY"); 
        GameObject impct = Instantiate(impctVFX, _hit.point, Quaternion.LookRotation(_hit.normal));
        Destroy(impct, 1f);
            
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;  
    }

    void MuzzleFlash()
    {
        muzzleFlash.Play();
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}
