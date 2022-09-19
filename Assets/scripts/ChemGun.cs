using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemGun : Pickup
{
    public override void OnUsed()
    {
        print("Using chemical gun!");
    }
}
