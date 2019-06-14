using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skip : Building
{
    [SerializeField]
    int _oreCapacity = 500;
    public List<GameObject> _fillTiles = new List<GameObject>();

    public override void DestroyBuilding (bool cancelled = false)
    {
        if (_Built)
            _worldController._maxStorage -= _oreCapacity * (_Level + 1);
        _worldController.AnimateSkips(_worldController.CheckStorage());
        base.DestroyBuilding(cancelled);
    }

    protected override void OnBuilt ()
    {
        base.OnBuilt();
        AddOreCapacity();
    }

    public override bool UpgradeBuilding (bool requireCrystal = true)
    {
        if (base.UpgradeBuilding(requireCrystal))
        {
            AddOreCapacity();
            return true;
        }
        return false;
    }

    /// <summary>
    /// "Used to add to the maximum ore the player can hold"
    /// </summary>
    void AddOreCapacity ()
    {
        _worldController._maxStorage += _oreCapacity;
        _worldController.AnimateSkips(_worldController.CheckStorage());
        _worldController.FillSkips();
    }

    public override int GetID()
    {
        return (int)TileTypeID.Skip;
    }
}
