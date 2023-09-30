using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] float shieldHealth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DamageShield(float playerdamage)
    {
        shieldHealth -= playerdamage;
        if(shieldHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
