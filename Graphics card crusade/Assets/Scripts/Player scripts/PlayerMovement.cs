using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Everything todo with sliding")]
    [SerializeField] float crouchScaleAmount;
    [SerializeField] Vector3 baseScale;
    [SerializeField] float slidingSpeed;

    [Header("Something todo with the movement")]
    private float startingSpeed;
    private float startingSpeedCap;

    public float movementSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxSpeedCap;

    [SerializeField] float airAccel;

    [SerializeField] Transform orientation;

    [Header("Counter movement shit")]
    [SerializeField] float moveThreshold;
    [SerializeField] float counterMovement = 0.175f;

    [Header("Mouse sensitivity and the camera shit")]
    [SerializeField] float mouseSensitivityX;
    [SerializeField] float mouseSensitivityY;
    [SerializeField] float mouseSensitivityMultiplier;
    [SerializeField] Transform cam;
    [SerializeField] Transform camPos;
    [SerializeField] float normalFOV;
    [SerializeField] float movementFOV;
    [SerializeField] float moveTilt = 45;

    [Header("everything with checking the ground")]
    [SerializeField] Transform groundCheckPos;
    [SerializeField] float groundCheckRange;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask ground;

    [SerializeField] float groundTime;

    [Header("jumping force und mass")]
    [SerializeField] float jumpForce;
    [SerializeField] float gravity = -9.81f;

    [Header("Coyote time und Jump buffering")]
    [SerializeField] float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [SerializeField] float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Player health")]
    public float playerHealth;

    [Header("Vfx")]
    [SerializeField] ParticleSystem slididingParticles;
    [SerializeField] GameObject speedLines;
    [SerializeField] GameObject slidingLines;

    [Header("UI")]
    [SerializeField] GameObject deathScreen;
    [SerializeField] TextMeshProUGUI speedUI;
    [SerializeField] TextMeshProUGUI groundTimeUI;
    [SerializeField] TextMeshProUGUI coyoteTimeUI;

    [HideInInspector] public bool isGrounded;

    private float xMovement;
    private float yMovement;
    [HideInInspector] public Vector3 movement;
    private Vector3 velocity;

    private float startingSlidingSpeed;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;

    private Quaternion camRot;

    RaycastHit _groundHit;

    private bool jumping;
    public static bool crouching;

    private CharacterController _Player;
    [HideInInspector] public CharacterController cc;
    private Camera _cam;
    private ShakeDaCamera camShake;

    // Start is called before the first frame update
    void Start()
    {
        _Player = GetComponent<CharacterController>();
        cc = GetComponent<CharacterController>();
        _cam = cam.GetComponentInChildren<Camera>();
        camShake = _cam.GetComponent<ShakeDaCamera>();

        camRot = _cam.transform.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        _cam.fieldOfView = normalFOV;

        //Get to chaging this
        velocity.y = Mathf.Clamp(velocity.y, -10f, 10f);
        movementSpeed = Mathf.Clamp(movementSpeed, 0, maxSpeed);   
        
        startingSpeed = movementSpeed;
        startingSlidingSpeed = slidingSpeed;
        startingSpeedCap = maxSpeedCap;
        
        baseScale = transform.localScale;

        deathScreen.SetActive(false);
    }

    void FixedUpdate() 
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {       
        speedUI.text = movementSpeed.ToString();
        groundTimeUI.text = groundTime.ToString();
        coyoteTimeUI.text = coyoteTimeCounter.ToString();

        MovementEffects();

        GettingTheInput();
        AditionalMovement();

        Sliding();
        
        CameraMovement(orientation);

        PlayerGravity();
        Jumping();
    }

    void GettingTheInput()
    {
        //And this 
        if(movementSpeed > maxSpeed)
        {
            movementSpeed = maxSpeed;
        }    

        xMovement = Input.GetAxis("Horizontal");
        yMovement = Input.GetAxis("Vertical");

        jumping = Input.GetKey(KeyCode.Space);
        crouching = Input.GetKey(KeyCode.LeftControl);
    }

    void Movement()
    {   
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        //if(movementSpeed != 0f)
            //camShake.MoreShakeCam((movementSpeed /(movementSpeed/100f)) /100f, (movementSpeed /(movementSpeed /100f)) /100f);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        movement = orientation.right * xMovement + orientation.forward * yMovement;
        cc.Move(movement * movementSpeed * Time.deltaTime);

        RaycastHit _hit;
        if(Physics.Raycast(transform.position, -transform.up, out _hit, groundCheckRange))
        {
            float dotProd = Vector3.Dot(transform.up, _hit.normal);
            if(dotProd < 1f)
            {
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 60f);
                maxSpeedCap += 1f;
            }
            if(dotProd == 1f)
            {
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 60f);
                maxSpeedCap -= 1f;
            }
        }
    }

   
    void Jumping()
    {
        //TODO: make this shit actually work
        /*if(isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTime -= 0.01f;
        }

        if(jumping)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }*/

        if(jumping && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            cc.Move(velocity * Time.deltaTime);
        }
        
        if(Input.GetKeyUp(KeyCode.Space))
        {
            coyoteTimeCounter = 0f;
        }
    }

    void Sliding()
    {
        if(crouching)
        {
            Vector3 scale = transform.localScale;
            scale.y = crouchScaleAmount;
            transform.localScale = scale;

            cam.transform.position = orientation.position;
            if(!isGrounded)
            {
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 60f);
                maxSpeedCap += 1f;
            }
        }
        else
        {
            transform.localScale = baseScale;
            cam.transform.position = camPos.position;
            maxSpeedCap = startingSpeedCap;
        }
    }

    void AditionalMovement()
    {

        if(!isGrounded && (groundTime != 1f || groundTime !> 1f))
        {
            maxSpeed = Mathf.Clamp(maxSpeed, startingSpeed, maxSpeedCap);
            maxSpeed += 1f;
            movementSpeed += airAccel;
        }
        else if(isGrounded && groundTime >= 0.8f)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, startingSpeed, 2f);
        }

        if(isGrounded) groundTime += 0.01f;
        else groundTime = 0;

    }

    void MovementEffects()
    {
        //Move particles
        if(cc.velocity != Vector3.zero)
        {
            speedLines.SetActive(true);
        }
        else
        {
            speedLines.SetActive(false);
        }

        if(crouching && isGrounded && movementSpeed > 0f)
        {
            slidingLines.SetActive(true);
        }
        else
        {
            slidingLines.SetActive(false);
        }

        //Move fov
        float parameter = Mathf.InverseLerp(startingSpeed, maxSpeed, movementSpeed);
        _cam.fieldOfView = Mathf.Lerp(normalFOV, movementFOV, parameter);

        //Camera tilt (who could've guesed :0)
        CameraTilt();
    }

    void CameraMovement(Transform thingToRotateAround)
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * mouseSensitivityMultiplier * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * mouseSensitivityMultiplier * Time.deltaTime;
        
        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        thingToRotateAround.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    void CameraTilt()
    {
        float rotZ = -Input.GetAxis("Horizontal") * moveTilt;

        Quaternion finalRot = Quaternion.Euler(_cam.transform.rotation.x, _cam.transform.rotation.y, rotZ);
        _cam.transform.localRotation = Quaternion.Lerp(camRot, finalRot, 1f);
    }

    void PlayerGravity()
    {
        isGrounded = Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out _groundHit, groundCheckRange);
    }

    public void DamagePlayer(float playerDamage)
    {
        playerHealth -= playerDamage;

        if(playerHealth <= 0f)
        {
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("Player ded");
            
            deathScreen.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
