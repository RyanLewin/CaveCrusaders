using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBlockade : Building
{
    TileMap3D _tilemap;

    public LavaBlockade _mainBlockade;
    public List<GameObject> _sideWalls = new List<GameObject>();
    public int connected = 1;

    protected override void OnFluidContact (FluidContactInfo f)
    {
        if (f.ContactedFluid.GetFluidType() == Fluid.FluidType.Lava)
        {
            if (_mainBlockade != null)
            {
                _mainBlockade.LoseEnergy = true;
            }
        }

        if (_Energy <= 0 || !_Built)
        {
            base.OnFluidContact(f);
        }
    }

    protected override void TileUpdate ()
    {
        base.TileUpdate();
        if (_mainBlockade != this && _mainBlockade)
        {
            _Energy = _mainBlockade._Energy;
            _barsUpdater.UpdateEnergyFill(_Energy);
        }
    }

    public override bool UpgradeBuilding (bool requireCrystal = true)
    {
        if (_mainBlockade != this)
        {
            if (_mainBlockade.UpgradeBuilding(requireCrystal))
            {
                _Level = _mainBlockade._Level;
                return true;
            }
            else
            {
                return false;
            }
        }
        if (base.UpgradeBuilding(requireCrystal))
        {
            switch (_Level)
            {
                case (1):
                    _EnergyReduction = .7f;
                    break;
                case (2):
                    _EnergyReduction = .4f;
                    break;
            }
            return true;
        }
        return false;
    }

    protected override void EnergyTimer ()
    {
        if (_mainBlockade == this)
        {
            base.EnergyTimer();
        }
    }

    public override int GetID ()
    {
        return (int)TileTypeID.LavaBlockade;
    }

    public override void OnGridUpdate ()
    {
        base.OnGridUpdate();
        Tile tile = _tilemap.GetTileAtPos(rotated ? X : X + 1,
                                          rotated ? Y - 1 : Y);
        if (tile != null)
        {
            if (tile.GetComponent<RockScript>())
            {
                _sideWalls[0].SetActive(true);
            }
        }

        tile = _tilemap.GetTileAtPos(rotated ? X : X - 1,
                                          rotated ? Y + 1 : Y);
        if (tile != null)
        {

            if (tile.GetComponent<RockScript>())
            {
                _sideWalls[1].SetActive(true);
            }
        }

        GetInitialBlockade();
    }

    protected override void OnTileStart ()
    {
        base.OnTileStart();
        _sideWalls = new List<GameObject> {
            _BuildingObject.transform.GetChild(0).GetChild(0).gameObject,
            _BuildingObject.transform.GetChild(0).GetChild(1).gameObject,
            _BuildingObject.transform.GetChild(0).GetChild(2).gameObject,
            _BuildingObject.transform.GetChild(0).GetChild(3).gameObject
        };
        _tilemap = GameObject.FindGameObjectWithTag("Logic").GetComponent<TileMap3D>();
        HadFluidBlock = true;
    }

    void GetInitialBlockade ()
    {
        Tile tile = _tilemap.GetTileAtPos(rotated ? X : X - 1,
                                          rotated ? Y - 1 : Y);
        if (tile)
        {
            if (tile.GetComponent<LavaBlockade>())
            {
                LavaBlockade blockade = tile.GetComponent<LavaBlockade>();
                if (blockade._mainBlockade)
                    _mainBlockade = blockade._mainBlockade;
                _mainBlockade.connected += 1;
            }
            else
            {
                connected = 1;
                _mainBlockade = this;
            }
        }
        AdjacentBlockades(true);
    }

    protected override void OnTileDestroy ()
    {
        base.OnTileDestroy();
        AdjacentBlockades(false);
    }

    void AdjacentBlockades (bool activate)
    {
        if(_tilemap == null)
        {
            return;
        }
        Tile tile = _tilemap.GetTileAtPos(X + 1, Y);
        if (CheckIfAdjacentRotated(tile))
        {
            if (rotated)
            {
                _sideWalls[3].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[1].SetActive(activate);
            }
            else
            {
                _sideWalls[0].SetActive(true);
                tile.GetComponent<LavaBlockade>()._sideWalls[2].SetActive(activate);
            }
        }
        tile = _tilemap.GetTileAtPos(X - 1, Y);
        if (CheckIfAdjacentRotated(tile))
        {
            if (rotated)
            {
                _sideWalls[2].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[0].SetActive(activate);
            }
            else
            {
                _sideWalls[1].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[3].SetActive(activate);
            }
        }
        tile = _tilemap.GetTileAtPos(X, Y - 1);
        if (CheckIfAdjacentRotated(tile))
        {
            if (rotated)
            {
                _sideWalls[0].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[3].SetActive(activate);
            }
            else
            {
                _sideWalls[2].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[1].SetActive(activate);
            }
        }
        tile = _tilemap.GetTileAtPos(X, Y + 1);
        if (CheckIfAdjacentRotated(tile))
        {
            if (rotated)
            {
                _sideWalls[1].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[2].SetActive(activate);
            }
            else
            {
                _sideWalls[3].SetActive(activate);
                tile.GetComponent<LavaBlockade>()._sideWalls[0].SetActive(activate);
            }
        }
    }

    bool CheckIfAdjacentRotated (Tile tile)
    {
        if (tile)
        {
            if (tile.GetComponent<LavaBlockade>())
            {
                if (tile.GetComponent<Building>().rotated != rotated)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
