using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [SerializeField] bool enable = true;

    [SerializeField, Range(0, 0.1f)] float amplitude = 0.015f;
    [SerializeField, Range(0, 30)] float frequency = 10f;

    [SerializeField] Transform _camera;
    [SerializeField] Transform cameraHolder;

    private float toggleSpeed = 3f;
    private Vector3 startPos;

    private CharacterController cc;
    private PlayerMovement pm;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponentInChildren<CharacterController>();
        pm = cc.GetComponent<PlayerMovement>();

        startPos = _camera.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(!enable) return;

        CheckMotion();
        //ResetPosition();
        //_camera.LookAt(FocusTarget());
    }

    void CheckMotion()
    {
        float speed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;

        if(speed <= toggleSpeed || !pm.isGrounded) 
        { 
            ResetPosition(); 
            return; 
        }
        else
        {
            PlayMotion(FootStepMotion());
        }
    }

    void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x = Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
        return pos;
    }

    void ResetPosition()
    {
        if(_camera.localPosition == startPos) return;

        _camera.localPosition = Vector3.Lerp(_camera.localPosition, startPos, 1 * Time.deltaTime);
    }
}
