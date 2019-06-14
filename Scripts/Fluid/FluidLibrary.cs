using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidLibrary : MonoBehaviour
{
    [SerializeField]
    Fluid[] _FluidDefinitions = {null};

    public Fluid GetFluid(Fluid.FluidType type)
    {
        return _FluidDefinitions[(int)type];
    }
}
