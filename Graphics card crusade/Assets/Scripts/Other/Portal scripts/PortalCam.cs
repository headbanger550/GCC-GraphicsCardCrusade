using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCam : MonoBehaviour
{
    [SerializeField] Transform playerCamera;
    [SerializeField] Transform camPos;
    [SerializeField] Transform portal;
    [SerializeField] Transform otherPortal;

    // Start is called before the first frame update
    void Start()
    {
        //transform.position = new Vector3(transform.position.x, camPos.position.y, transform.position.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 playerOffsetFromPortal = playerCamera.position - otherPortal.position;
        transform.position = portal.position + playerOffsetFromPortal;

        float angularDifferenceBetweenPortalRotations = Quaternion.Angle(portal.rotation, otherPortal.rotation);

        Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
        Vector3 newCameraDirrection = portalRotationalDifference * playerCamera.forward;
        transform.rotation = Quaternion.LookRotation(newCameraDirrection, Vector3.up);
        
    }
}
