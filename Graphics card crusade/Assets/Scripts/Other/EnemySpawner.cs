using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider coll) 
    {
        if(coll.CompareTag("Player"))
        {
            for(int i = 0; i < enemies.Length; i++)
            {
                enemies[i].SetActive(true);
            }
        }
    }
}
