using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoTypes
{
    bullets,
    twentyVoltGauge,
    fourGauge,
    rockets,
    APRounds,
    energy
}

public class Ammo : MonoBehaviour
{
    [SerializeField] AmmoSlot[] ammoSlots;

    [System.Serializable]
    private class AmmoSlot
    {
        public AmmoTypes ammoTypes;
        public int ammoAmount;
    }

    private int startingAmmo;

    void Start() 
    {
        //startingAmmo = ammoAmount;    
    }

    public int GetCurrentAmmo(AmmoTypes ammoTypes)
    {
        return GetAmmoSlot(ammoTypes).ammoAmount;
    }

    public void ReduceCurrentAmmo(AmmoTypes ammoTypes)
    {
        GetAmmoSlot(ammoTypes).ammoAmount--;
    }

    public void ReturnAmmo(AmmoTypes ammoTypes, int ammoAmount)
    {
        GetAmmoSlot(ammoTypes).ammoAmount += ammoAmount;
    }

    private AmmoSlot GetAmmoSlot(AmmoTypes ammoTypes)
    {
        foreach(AmmoSlot slot in ammoSlots)
        {
            if(slot.ammoTypes == ammoTypes)
            {
                return slot;
            }
        }
        return null;
    }
}
