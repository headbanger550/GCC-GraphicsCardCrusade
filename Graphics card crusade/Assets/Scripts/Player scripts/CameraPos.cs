using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    [SerializeField] Transform cameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = cameraPosition.position;
    }
}
