using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGen : Building
{
    [SerializeField]
    List<Building> connectedBuildings = new List<Building>();

    public override void OnGridUpdate ()
    {
        base.OnGridUpdate();
        foreach (var building in _worldController._buildings)
        {
            if (building.GetID() == (int)TileTypeID.PowerGen)
                continue;

            float distance = Vector3.Distance(transform.position, building.transform.position);
            if (distance < range)
            {
                connectedBuildings.Add(building);
            }
        }
    }

    protected override void EnergyTimer ()
    {
        if (_loseEnergy)
            base.EnergyTimer();

        foreach (var building in connectedBuildings)
        {
            if (_Energy > 0)
            {
                if (building._Energy <= 99)
                {
                    building.ReduceEnergy(-1 * (_Level + 1));
                    ReduceEnergy(1 * (_Level + 1));

                    if (building._Energy > 100)
                    {
                        building._Energy = 100;
                    }
                    if (_Energy < 0)
                    {
                        _Energy = 0;
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    public override int GetID ()
    {
        return (int)TileTypeID.PowerGen;
    }
}
