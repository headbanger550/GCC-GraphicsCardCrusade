using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class ShakeDaCamera : MonoBehaviour
{
    private Weapon currentWeapon;

    public void ShakeCamera(float additionalShake = default(float))
    {
        currentWeapon = GetComponentInChildren<Weapon>();
        CameraShaker.Instance.ShakeOnce(currentWeapon.shakeDuration, currentWeapon.shakeIntesity + additionalShake, currentWeapon.shakeFadeIn, currentWeapon.shakeFadeOut);
    }
}
