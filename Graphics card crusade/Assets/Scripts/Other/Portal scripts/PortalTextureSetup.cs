using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [SerializeField] Camera camA;
    [SerializeField] Material camMatA;

    [SerializeField] Camera camB;
    [SerializeField] Material camMatB;

    // Start is called before the first frame update
    void Start()
    {
        if(camA.targetTexture != null)
        {
            camA.targetTexture.Release();
        }
        camA.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        //camA.aspect = ((float)Screen.width) / Screen.height;
        camMatA.mainTexture = camA.targetTexture;

        if(camB.targetTexture != null)
        {
            camB.targetTexture.Release();
        }
        camB.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        //camB.aspect = ((float)Screen.width) / Screen.height;
        camMatB.mainTexture = camB.targetTexture;
    }
}
