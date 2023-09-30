using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider coll) 
    {
        PlayerMovement player = coll.GetComponentInChildren<PlayerMovement>();
        if(player != null)
        {
            player.DamagePlayer(player.playerHealth);
        }    
    }
}
