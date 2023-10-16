using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //
    // HEY DIPSHIT! ORGANISE THE METHODS BETTER FO FUCK SAKE!!!
    //

    [Header("The gun base")]
    public Gun baseGun;

    [Header("If the raycast should be explosive")]
    [SerializeField] bool isExplosive;
    [SerializeField] float explosionRadius;

    [Header("Huh so that's were the bullet comes out of")]
    [SerializeField] Transform firePoint;

    [Header("How many bullets do we want and what kind of spread should we have?")]
    [SerializeField] int bulletCount;

    [Header("spread")]
    [SerializeField] float spreadX;
    [SerializeField] float spreadY;

    [Header("Ammo and reloading")]
    [SerializeField] int ammoCount;
    [SerializeField] AmmoTypes ammoTypes;
    [SerializeField] bool hasReload;
    [SerializeField] float reloadTime;
    [SerializeField] float reloadDelay;

    [Header("Move Sway")]
    [SerializeField] float swayAmount = 1f;
    [SerializeField] float smoothAmount = 6f;
    [SerializeField] float maxSwayAmount = 2f;

    [Header("Rotation Sway")]
    [SerializeField] float rotationAmmount = 20f;
    [SerializeField] float maxRotationAmount = 30f;
    [SerializeField] float smoothRotation = 10f;

    [Header("VFX")]
    [SerializeField] LineRenderer shotLine;
    [SerializeField] ParticleSystem[] muzzleFlashes;
    [SerializeField] bool hasShellCasings;
    [SerializeField] bool isOnReload;
    [SerializeField] ParticleSystem shellCasing;

    [Header("Particle bullet(if needed")]
    [SerializeField] bool isLazer;
    [SerializeField] ParticleSystem[] particleBullets;

    [Header("Screen shake stuff")]
    public float shakeDuration;
    public float shakeIntesity;
    public float shakeFadeIn;
    public float shakeFadeOut;

    [HideInInspector] public float gunFireRate;

    [HideInInspector] public bool isParticleWeapon;

    private Vector3 randomSpread;
    private Vector3 destination;

    private Vector3 startingPos;

    private float startingSpreadX;
    private float startingSpreadY;

    private int startingAmmo;
    [HideInInspector] public bool isReloading;
    private float nextTimeToFire;

    private float mouseX;
    private float mouseY;

    private Quaternion startingRotation;

    [HideInInspector] public float gunDamage;

    private RaycastHit _hit;
    private Ray _ray;

    private Camera cam;
    private Animator gunAnim;
    private ShakeDaCamera camShake;

    private Ammo ammoSlot;

    void Start() 
    {
        startingPos = transform.localPosition;
        startingRotation = transform.localRotation;
    }

    void OnEnable() 
    {   
        gunAnim = GetComponent<Animator>();

        if(gunAnim == null)
        {
            gunAnim = GetComponentInChildren<Animator>();
        }    

        gunAnim.SetBool("Shooting", false);
        gunAnim.SetBool("Idle", true);

        if(hasReload)
            gunAnim.SetBool("Reloading", false);
        
        this.name = baseGun.name;
        gunDamage = baseGun.damage;
        gunFireRate = baseGun.fireRate;
        baseGun.isParticleBased = isParticleWeapon;

        startingSpreadX = spreadX;
        startingSpreadY = spreadY;

        cam = GetComponentInParent<Camera>();
        camShake = cam.GetComponent<ShakeDaCamera>();
        ammoSlot = GetComponentInParent<Ammo>();

        if(baseGun.hasShotLine) 
        {
            shotLine.enabled = false;
        }
        
        isReloading = false;

        startingAmmo = ammoCount;
    }
    void OnDisable() 
    {       
        gunDamage = 0f;

        gunAnim.SetBool("Shooting", false);
        gunAnim.SetBool("Idle", true);

        if (hasReload)
            gunAnim.SetBool("Reloading", false);
    }

    void Update() 
    {   
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        //MoveSway();
        TiltSway();

        if(hasReload)
        {
            if(isReloading) return;

            if(ammoCount <= 0)
            {
                StartCoroutine(Reload());
            }
        }

        if(Input.GetMouseButton(0) && Time.time >= nextTimeToFire && !isReloading && ammoSlot.GetCurrentAmmo(ammoTypes) > 0)
        {
            nextTimeToFire = Time.time + 1f / gunFireRate;
            gunFireRate = baseGun.fireRate;
            spreadX = startingSpreadX;
            spreadY = startingSpreadY;
            Shoot();
        }

        else if(Input.GetMouseButton(0) && ammoCount <= 0)
        {
            ammoCount = startingAmmo;
        }

        if(!Input.GetMouseButton(0) && !hasReload)
        {
            //MoveSway();
            gunAnim.SetBool("Shooting", false);
            gunAnim.SetBool("Idle", true);
        }

        else if(!Input.GetMouseButton(0) && isParticleWeapon)
        {
            foreach(var l in particleBullets)
            {
                l.Stop(true);
            }
             
        }

        else if(Input.GetMouseButton(1) && ammoCount <= 0)
        {
            ammoCount = startingAmmo;
        }
        
    }

    IEnumerator Reload()
    {  
        yield return new WaitForSeconds(reloadDelay);

        isReloading = true;

        gunAnim.SetBool("Shooting", false);
        gunAnim.SetBool("Reloading", true);

        yield return new WaitForSeconds(reloadTime);


        if(!Input.GetMouseButtonDown(0))
        {
            gunAnim.SetBool("Reloading", false);
            gunAnim.SetBool("Idle", true);
        }
        else
        {
            gunAnim.SetBool("Reloading", false);
            gunAnim.SetBool("Shooting", true);
        }

        isReloading = false;
        ammoCount = startingAmmo;
    }

    public void Shoot()
    {
        gunAnim.SetBool("Idle", false);
        gunAnim.SetBool("Shooting", true);

        if(!isOnReload)
            CreateVFX();

        camShake.ShakeCamera();

        for(int i = 0; i < muzzleFlashes.Length; i++) { muzzleFlashes[i].Play(); }

        ammoSlot.ReduceCurrentAmmo(ammoTypes);
        ammoCount--;

        if(baseGun.hasShotLine) shotLine.SetPosition(0, transform.position);

        if(baseGun.isRyacast)
        {
            if(baseGun.hasSpread)
            {
                for(int i = 0; i < bulletCount; i++)
                {
                    randomSpread = new Vector3(Random.Range(-spreadX, spreadX), Random.Range(-spreadY, spreadY), 0);

                    if(Physics.Raycast(cam.transform.position, cam.transform.forward + randomSpread, out _hit, baseGun.layerToDamage))
                    {
                        if(isExplosive)
                        {
                            ExplosiveRaycast(baseGun.damage / 2, explosionRadius, _hit.point);
                        }

                        CreateImpactVFX(baseGun.impactEffects);
                        MoveRigidBodies(_hit);
                        if(baseGun.hasShotLine) CreateShotline(_hit.point);

                        StartCoroutine(DamageObj());
                    }
                    else
                    {
                        if(baseGun.hasShotLine) CreateShotline(cam.transform.forward * 100f);
                    }
                    //StartCoroutine(ShootLineVFX());
                }
            }
            else
            {
                if(Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, baseGun.layerToDamage))
                {
                    if(isExplosive)
                    {
                        ExplosiveRaycast(baseGun.damage / 2, explosionRadius, _hit.point);
                    }

                    CreateImpactVFX(baseGun.impactEffects);
                    MoveRigidBodies(_hit);
                    if(baseGun.hasShotLine) CreateShotline(_hit.point);

                    StartCoroutine(DamageObj());
                }
                else
                {
                    if(baseGun.hasShotLine) CreateShotline(cam.transform.forward * 100f);
                }
               //StartCoroutine(ShootLineVFX());
            }
        }

        if(baseGun.isProjectile)
        {
            if(baseGun.hasSpread)
            {
                for(int i = 0; i < bulletCount; i++)
                {
                    GetPoint(randomSpread);
                    InstantiateProjectile(firePoint, baseGun.bullet, firePoint.rotation);
                }
            }
            else
            {
                GetPoint();
                InstantiateProjectile(firePoint, baseGun.bullet, firePoint.rotation);
            }
        }

        if(baseGun.isParticleBased)
        {
            foreach(var l in particleBullets)
            {
               l.Stop(false);
               l.Play();
            }

            if(isLazer)
                FireLazer();
        }
    }

    void InstantiateProjectile(Transform firepoint, GameObject bullet, Quaternion rotation)
    {
        GameObject projectileObj = Instantiate(bullet, firepoint.position, rotation);
        projectileObj.GetComponent<Rigidbody>().velocity = (destination - firepoint.position).normalized * baseGun.bulletSpeed;
        
        if(projectileObj.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Boy there's no god dam rigidbody on the bullet");
        }
    }

    void GetPoint(Vector3 optionalSpread = default(Vector3))
    {
        randomSpread = new Vector3(Random.Range(-spreadX, spreadX), Random.Range(-spreadY, spreadY), 0);
                
        _ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0) + optionalSpread);

        if (Physics.Raycast(_ray, out _hit, baseGun.layerToDamage))
        {
            destination = _hit.point;
        }   
        else
        {
            destination = _ray.GetPoint(500);
        }
    }

    void MoveRigidBodies(RaycastHit hit)
    {
        Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.AddForce(-hit.normal * baseGun.impactForce);
        }
    }

    void ExplosiveRaycast(float explosiveDamage, float range, Vector3 explosionPosition)
    {
        Collider[] gotColliders = Physics.OverlapSphere(explosionPosition, range);
        foreach(Collider damageObject in gotColliders)
        {
            Enemy enemies = damageObject.GetComponent<Enemy>();
            if(enemies != null)
            {
                enemies.DamageEnemy(explosiveDamage);
            }

            Rigidbody rb = damageObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.AddExplosionForce(5000, explosionPosition, range);
            }
        }
    }

    void FireLazer()
    {
        Vector3 defaultScale = particleBullets[0].shape.scale;

        float lazerLength = 100f;

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, lazerLength, baseGun.layerToDamage))
        {
            lazerLength = hit.distance;
        }

        Vector3 newScale = new Vector3(defaultScale.x, defaultScale.y, lazerLength);
        Vector3 newPos = new Vector3(0, 0, lazerLength / 2);

        foreach(var l in particleBullets)
        {
            ParticleSystem.ShapeModule s = l.shape;
            s.scale = newScale;
            s.position = newPos;
        }
    }

    IEnumerator DamageObj()
    {
        Enemy enemy = _hit.transform.GetComponent<Enemy>();
        Shield shield = _hit.transform.GetComponent<Shield>();

        if(enemy != null)
        {
            enemy.DamageEnemy(gunDamage);

            if(shield != null)
            {
                shield.DamageShield(gunDamage);
                Debug.Log("damaged");
            }

            CreateImpactVFX(enemy.enemyHitParticles);
            camShake.ShakeCamera(3);

            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1;
        }
    }

    void CreateShotline(Vector3 endPoint)
    {
        shotLine.enabled = true;
        LineRenderer lr = Instantiate(shotLine).GetComponent<LineRenderer>();
        lr.SetPositions(new Vector3[2] {firePoint.position, endPoint});
        Destroy(lr.gameObject, 0.2f);
    }

    void CreateImpactVFX(GameObject[] impcts) // <-- make sure this works with the SO (sciptable object) array
    {
        foreach(GameObject impct in impcts)
        {
            GameObject impactObj = Instantiate(impct, _hit.point, Quaternion.LookRotation(_hit.normal));
            Destroy(impactObj, 1f);
        }
    }

    public void CreateVFX()
    {
        if(hasShellCasings)
        {
            shellCasing.Emit(1);
        }
    }

    void MoveSway()
    {
        mouseX = Mathf.Clamp(mouseX * swayAmount, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY * swayAmount, -maxSwayAmount, maxSwayAmount);

        Vector3 finalPos = new Vector3(mouseX, mouseY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + startingPos, Time.deltaTime * smoothAmount);
    }

    void TiltSway()
    {
        float tiltY = Mathf.Clamp(mouseX * rotationAmmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = Mathf.Clamp(mouseY * rotationAmmount, -maxRotationAmount, maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(-tiltX, tiltY, tiltY));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * startingRotation, Time.deltaTime * smoothRotation);
    }

}
