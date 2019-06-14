using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H2OtoO2 : OxygenGenerator
{
    FluidEngine _FluidEngine;
    [SerializeField]
    float _fluidToRemove = .02f;
    float _powerGained;

    protected override void OnTileStart()
    {
        base.OnTileStart();
        _FluidEngine = _worldController.GetComponentInChildren<FluidEngine>();
    }

    protected override void OnBuilt ()
    {
        base.OnBuilt();
        _CanFluidEnter = true;
    }

    protected override void TileUpdate ()
    {
        base.TileUpdate();
    }

    protected override void OnFluidContact(FluidContactInfo f)
    {
        base.OnFluidContact(f);
        if (!_Built)
            return;
        if (f.ContactedFluid.GetFluidType() == Fluid.FluidType.Water)
        {
            if (_FluidLevel <= 0)
            {
                return;
            }
            float removed = _FluidEngine.RemoveFluidFromCell(x, y, _fluidToRemove, Fluid.FluidType.Water);
            _powerGained += removed;
            if (_powerGained >= 1)
            {
                _Energy += 1;
                if (_Energy > 100)
                    _Energy = 100;
                _barsUpdater.UpdateEnergyFill(_Energy);
                _powerGained = 0;
            }
        }
    }

    public override int GetID()
    {
        return (int)TileTypeID.H2OConverter;
    }
}
