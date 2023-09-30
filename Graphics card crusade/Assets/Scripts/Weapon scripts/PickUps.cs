using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickUps : MonoBehaviour
{
    [SerializeField] int ammoReturnAmmount;
    [SerializeField] AmmoTypes ammoType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider coll) 
    {
        if(coll.gameObject.CompareTag("Player"))
        {
            Ammo ammo = FindObjectOfType<Ammo>();
            ammo.ReturnAmmo(ammoType, ammoReturnAmmount);
            Destroy(gameObject);
        }
    }
}
