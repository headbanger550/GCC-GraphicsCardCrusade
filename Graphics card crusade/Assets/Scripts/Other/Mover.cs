using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] bool isSprite;
    private Transform player;

    [Header("1-rotate, 2-move")]
    [SerializeField] int type;

    [Space]

    [SerializeField] bool shouldBob;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobOffset;

    [Space]

    [SerializeField] float rotationSpeed;
    [SerializeField] Vector3 rotateAxis;

    private Vector3 startingPos;
    private Vector3 currentRotation;

    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;

        if(isSprite)
        {
            player = FindObjectOfType<PlayerMovement>().transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isSprite)
        {
            transform.LookAt(player);
        }

        if(shouldBob)
        {
            transform.position = new Vector3(transform.position.x, startingPos.y + Mathf.Sin(Time.time * bobSpeed) / bobOffset, transform.position.z);
        }
        if(type == 1)
        {
            transform.Rotate(rotateAxis * rotationSpeed * Time.deltaTime);
        }
    }
}
