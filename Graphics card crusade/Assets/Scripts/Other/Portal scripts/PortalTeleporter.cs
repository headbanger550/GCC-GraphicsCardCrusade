using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform receiver;

    private bool playerIsOverlaping = false;

    // Update is called once per frame
    void Update()
    {
        if(playerIsOverlaping)
        {
            Vector3 portalToPlayer = player.position - transform.position;
            float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

            if(dotProduct < 0)
            {
                float rotationDifference = Quaternion.Angle(transform.rotation, receiver.rotation);
                rotationDifference += 180f;
                player.Rotate(Vector3.up, rotationDifference);

                Vector3 positionOffset = Quaternion.Euler(0f, rotationDifference, 0f) * portalToPlayer;
                player.position = receiver.position + positionOffset;

                playerIsOverlaping = false;
            }
        }
    }

    void OnTriggerEnter(Collider coll) 
    {
        if(coll.CompareTag("Player"))
        {
            playerIsOverlaping = true;
        }
    }

    void OnTriggerExit(Collider coll) 
    {
        if(coll.CompareTag("Player"))
        {
            playerIsOverlaping = false;
        }
    }
}
