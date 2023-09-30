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

    [SerializeField] float movementSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxSpeedCap;

    [SerializeField] float airAccel;

    [SerializeField] Transform orientation;

    [Header("Counter movement shit")]
    [SerializeField] float moveThreshold;
    [SerializeField] float counterMovement = 0.175f;

    [Header("Mouse sensitivity and the camera transform")]
    [SerializeField] float mouseSensitivityX;
    [SerializeField] float mouseSensitivityY;
    [SerializeField] float mouseSensitivityMultiplier;
    [SerializeField] Transform cam;
    [SerializeField] Transform camPos;

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

    private bool isGrounded;

    private float xMovement;
    private float yMovement;
    private Vector3 movement;
    private Vector3 velocity;
    private Vector2 mag;

    private float startingSlidingSpeed;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;

    RaycastHit _groundHit;

    private bool jumping;
    public static bool crouching;

    private CharacterController _Player;
    private Rigidbody rb;
    private Camera _cam;

    // Start is called before the first frame update
    void Start()
    {
        _Player = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        _cam = cam.GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

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
        rb.AddForce(velocity * Time.deltaTime);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        mag = FindVelRelativeToLook();

        Friction();

        movement = orientation.right * xMovement + orientation.forward * yMovement;
        rb.AddForce(movement * (movementSpeed * 10) * Time.deltaTime);

        RaycastHit _hit;
        if(Physics.Raycast(transform.position, -transform.up, out _hit, groundCheckRange))
        {
            float dotProd = Vector3.Dot(transform.up, _hit.normal);
            if(dotProd < 1f)
            {
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 750f);
                maxSpeedCap += 1f;
            }
            if(dotProd == 1f)
            {
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 750f);
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
            rb.AddForce(transform.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
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
                maxSpeedCap = Mathf.Clamp(maxSpeedCap, startingSpeedCap, 850f);
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
        if(xMovement == 0f && yMovement == 0f && isGrounded)
        {
            movementSpeed = 0;
            gravity = -1100;
            if(jumping)
            {
                jumpForce = Mathf.Clamp(jumpForce, 100f, 150f);
                jumpForce *= 2;
            }
        }
        else
        {
            movementSpeed = maxSpeed;
            jumpForce = 200f;
            gravity = -1800;
        }

        if(!isGrounded && (groundTime != 1f || groundTime !> 1f))
        {
            maxSpeed = Mathf.Clamp(maxSpeed, startingSpeed, maxSpeedCap);
            maxSpeed += 1f;
            movementSpeed += airAccel;
        }
        else if(isGrounded && groundTime >= 0.8f)
        {
            maxSpeed = startingSpeed;
        }

        if(isGrounded) groundTime += 0.01f;
        else groundTime = 0;

    }

    void MovementEffects()
    {
        if(movementSpeed > 0f)
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

    void Friction()
    {
        //dir = movement - rb.velocity;
        if(xMovement == 0 && yMovement == 0 && isGrounded)
        {
            rb.velocity -= rb.velocity;
        }

        if (Mathf.Abs(mag.x) > moveThreshold && Mathf.Abs(xMovement) < 0.05f || (mag.x < -moveThreshold && xMovement > 0) || (mag.x > moveThreshold && xMovement < 0)) {
            rb.AddForce(movementSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > moveThreshold && Mathf.Abs(yMovement) < 0.05f || (mag.y < -moveThreshold && yMovement > 0) || (mag.y > moveThreshold && yMovement < 0)) {
            rb.AddForce(movementSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
    }

    public Vector2 FindVelRelativeToLook() 
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }
}
