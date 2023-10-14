using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] GameObject doorPng;
    [SerializeField] float launchForce;

    [SerializeField] float velocityThreshold;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision coll) 
    {
        if(coll.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = coll.gameObject.GetComponent<PlayerMovement>();
            if(player != null)
            {
                if(player.movementSpeed >= velocityThreshold)
                {
                    GameObject obj = Instantiate(doorPng, transform.position, player.transform.localRotation);
                    obj.GetComponent<Rigidbody>().AddForce(obj.transform.forward * launchForce * Time.deltaTime, ForceMode.Impulse);

                    Destroy(obj, 1f);
                    Destroy(gameObject);
                }
            }
        }
    }
}
