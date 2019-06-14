using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Garage : Building
{
    [SerializeField]
    Transform _spawnPoint;
    Vector3 _vehiclePosition;
    [SerializeField]
    Transform _lift;
    [SerializeField]
    GameObject _defaultTile;
    [SerializeField]
    GameObject _holeTile;

    [SerializeField]
    List<Vehicle> _vehicles = new List<Vehicle>();
    List<int> _vehicleCost = new List<int> { 10, 10 };
    GameObject _spawnedVehicle;
    public bool spawning = false;
    bool _spawned = false;
    [SerializeField]
    string _layerName;
    int _layer;
    float _timeToSpawn = 5f;
    float _spawnProgress = 0;

    protected override void OnTileStart ()
    {
        base.OnTileStart();
        _layer = LayerMask.NameToLayer(_layerName);
    }

    protected override void TileUpdate ()
    {
        base.TileUpdate();

        //Testing
        if (Input.GetKeyDown(KeyCode.V))
            BuildVehicle(0);

        if (_spawned)
        {
            RaiseVehicle();
        }
    }

    protected override void OnBuilt ()
    {
        base.OnBuilt();
        _holeTile.SetActive(true);
        _defaultTile.SetActive(false);
    }

    void RaiseVehicle ()
    {
        _spawnProgress += Time.deltaTime / _timeToSpawn;
        //_spawnedVehicle.transform.position = new Vector3(_spawnPoint.position.x,
        //                                                  Mathf.Min(_spawnPoint.position.y - _timeToSpawn + _spawnProgress, _spawnPoint.position.y),
        //                                                  _spawnPoint.position.z);
        //_spawnedVehicle.transform.position = Vector3.Lerp(new Vector3(_spawnPoint.position.x, _spawnPoint.position.y - 5, _spawnPoint.position.z),
        //                                                        _spawnPoint.position, _spawnProgress);
        _spawnedVehicle.transform.position = _lift.position + new Vector3(0, 2.1f, 0);

        if (_spawnProgress >= 1)
        {
            //smokeVFX.Stop();
            SetLayerRecursive(_spawnedVehicle.transform.GetChild(0).gameObject, _layer);
            float rand = Random.Range(-3f, 3f);
            Vector3 pos = _spawnPoint.position;
            pos.x += rand;
            _spawnedVehicle.GetComponent<NavMeshAgent>().enabled = true;
            _spawnedVehicle.GetComponent<NavMeshAgent>().SetDestination(pos);
            StartCoroutine(Close());
            _spawnProgress = 0;
        }
    }

    IEnumerator Close ()
    {
        _spawned = false;
        yield return new WaitForSeconds(3f);
        _spawnedVehicle.tag = TagLibrary.VEHICLE_TAG;
        _spawnedVehicle = null;
        spawning = false;
        if (GetComponentInChildren<Animator>())
        {
            GetComponentInChildren<Animator>().Play("Close");
        }
    }

    public void BuildVehicle (int v)
    {
        if (_Energy <= 0)
            return;
        if (!spawning)
        {
            if (_worldController._oreCount >= _vehicleCost[v])
            {
                //smokeVFX.Play();
                ReduceEnergy(10);
                if (GetComponentInChildren<Animator>())
                {
                    GetComponentInChildren<Animator>().Play("Open");
                }
                StartCoroutine(WaitToSpawn(v));
            }
            else
            {
                _worldController.UIScript.ShowNotification("Not enough Ore!");
            }
        }
    }

    IEnumerator WaitToSpawn (int v)
    {
        spawning = true;
        yield return new WaitForSeconds(2f);
        _spawnedVehicle = Instantiate(_vehicles[v].gameObject, _lift.position, _lift.rotation);
        _spawnedVehicle.GetComponent<Unit>()._worldController = _worldController;
        _spawnedVehicle.GetComponent<Unit>().SetTool();
        _worldController._oreCount -= _vehicleCost[v];
        _worldController._levelStatsController.OreUsed(_vehicleCost[v]);
        _spawnedVehicle.GetComponent<NavMeshAgent>().enabled = false;
        SetLayerRecursive(_spawnedVehicle.transform.GetChild(0).gameObject, 0);
        _spawnedVehicle.tag = "Untagged";
        _spawned = true;
    }

    public static void SetLayerRecursive (GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform t in gameObject.transform)
        {
            SetLayerRecursive(t.gameObject, layer);
        }
    }

    public override int GetID ()
    {
        return (int)TileTypeID.Garage;
    }

    public int GetVehicleCost(int vehicleId)
    {
        return _vehicleCost[vehicleId];
    }
}
