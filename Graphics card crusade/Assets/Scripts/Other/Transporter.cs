using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transporter : MonoBehaviour
{
    [Header("1-jump pad, 2-teleporter, 3-latch button, 4-latch")]
    [SerializeField] int tipe;

    [Header("for jump pad")]
    [SerializeField] float jumpForce;

    [Header("For telepad")]
    [SerializeField] Transform destination;

    [Header("For button")]
    [SerializeField] float interactionRange;
    private Transform player;

    [Header("For latch")]
    [SerializeField] GameObject latches;

    private float distance;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(transform.position, player.position);
        if(tipe == 3)
        {
            if(distance <= interactionRange && Input.GetKeyDown(KeyCode.E))
            {
                Destroy(latches);
            }
        }
    }

    void OnTriggerEnter(Collider coll) 
    {
        if(tipe == 1)// basically if it's a jump pad
        {
            Rigidbody player = coll.GetComponentInChildren<Rigidbody>();
            if(player != null)
            {
                
                player.AddForce(player.transform.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
            }
        }
        if(tipe == 2)
        {
            coll.transform.position = destination.position;
        }
    }
}
