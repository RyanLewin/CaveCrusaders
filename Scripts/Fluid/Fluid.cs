using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid : MonoBehaviour
{
    [SerializeField]
    //Time taken to Move One unit a tile.
    public float _FlowRate = 1.0f;
    public enum FluidType { Lava, Water, None, Limit };
    [SerializeField]
    FluidType _CurrnetType = FluidType.None;
    public FluidType GetFluidType()
    {
        return _CurrnetType;
    }
    public float DamageToTiles = 1.0f;
}
public struct FluidContactInfo
{
    public Fluid ContactedFluid;
}