using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HQ : Skip
{
    [SerializeField]
    Unit _worker;
    public int workerCost = 3;
    [SerializeField]
    Transform spawnPoint;

    [SerializeField]
    GameObject part2;
    [SerializeField]
    GameObject part3;

    [SerializeField]
    Transform _satelliteModel;
    [SerializeField]
    Transform _satellitePivot;
    float _rotateProgress;
    float _timeToRotate = 3f;
    Vector3 _originalRotation;

    protected override void OnTileStart ()
    {
        base.OnTileStart();
        _worldController._HQ = this;
        _satelliteModel.parent = _satellitePivot;
        _satellitePivot.Rotate(transform.forward, 15f);
        StartCoroutine(RotateSatelliteDish());
    }

    IEnumerator RotateSatelliteDish ()
    {
        yield return new WaitForSeconds(3f);
        float rand = Random.Range(-180, 180);
        _originalRotation = _satellitePivot.rotation.eulerAngles;
        StartCoroutine(Rotation(rand));
        //_satellitePivot.Rotate(transform.up, rand);
    }

    IEnumerator Rotation (float value)
    {
        yield return new WaitForFixedUpdate();
        _rotateProgress += Time.deltaTime / _timeToRotate;
        _satellitePivot.rotation = Quaternion.Euler(Vector3.Lerp(_originalRotation, 
                                            new Vector3(_originalRotation.x, _originalRotation.y + value, _originalRotation.z),
                                            _rotateProgress));
        if (_rotateProgress < 1)
        {
            StartCoroutine(Rotation(value));
        }
        else
        {
            _rotateProgress = 0;
            StartCoroutine(RotateSatelliteDish());
        }
    }

    protected override void OnTileDestroy ()
    {
        base.OnTileDestroy();
        if (_worldController)
        {
            _worldController._HQ = null;
            TileMap3D tileMap = _worldController.TileMap;
            Tile temp = tileMap.GetTileAtPos(x + (int)_BuildingSize[0].x, y + (int)_BuildingSize[0].y);
            if (temp)
            {
                if (temp.tag == "HQ")
                    tileMap.ClearTile(temp.X, temp.Y);
            }
        }
    }

    public override bool UpgradeBuilding (bool requireCrystal = true)
    {
        bool success = base.UpgradeBuilding(requireCrystal);

        if (!success)
            return false;

        _worldController._miningLevel = (WorldController.MiningLevel)_Level;

        switch (_Level)
        {
            case (1):
                part2.SetActive(true);
                break;
            case (2):
                part3.SetActive(true);
                break;
        }
        foreach (Unit unit in _worldController._workers)
        {
            Unit worker = unit.GetComponent<Unit>();
            if (worker != null)
            {
                worker.SetTool();
            }
        }
        return success;
    }

    /// <summary>
    /// "Used to Instantiate an extra worker"
    /// </summary>
    public void AddUnit (bool requireOre = true)
    {
        if (_worldController._oreCount >= workerCost || requireOre == false)
        {
            if (_worldController._workers.Count >= _worldController._workerLimit)
            {
                _worldController.UIScript.ShowNotification("Max amount of Workers!");
                return;
            }
            Unit worker = Instantiate(_worker, spawnPoint.position, _worker.transform.rotation);
            _worldController._workers.Add(worker);
            _worldController._levelStatsController.UnitSpawned();
            worker._worldController = _worldController;
            worker._currentTool = Unit.UnitTool.MiningTool;
            worker.SetTool();
            if (!_worldController._UseO2)
                worker._useO2 = false;
            if (requireOre)
            {
                _worldController._oreCount -= workerCost;
                _worldController._levelStatsController.OreUsed(workerCost);
            }
        }
        else
        {
            _worldController.UIScript.ShowNotification("Not enough Ore!");
        }
    }

    public override int GetID()
    {
        return (int)TileTypeID.HQ;
    }
}