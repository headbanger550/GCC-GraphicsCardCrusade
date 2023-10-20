using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBob : MonoBehaviour
{
    [SerializeField, Range(0, 0.1f)] float amplitude = 0.015f;
    [SerializeField, Range(0, 30)] float frequency = 10f;
    [SerializeField] CharacterController cc;

    private Vector3 startPos;

    private PlayerMovement pm;
    
    // Start is called before the first frame update
    void Start()
    {
        pm = cc.GetComponent<PlayerMovement>();

        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;

        if(speed <= 3f || !pm.isGrounded) 
        { 
            ResetPosition(); 
            return; 
        }
        else
        {
            PlayMotion();
        }
    }

    void PlayMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * frequency) * amplitude;
        pos.z = Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;

        transform.localPosition += pos; 
    }

    void ResetPosition()
    {
        if(transform.localPosition == startPos) return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, 1 * Time.deltaTime);
    }
}
