using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Outpost : Building
{
    [SerializeField]
    Unit _worker;
    int workerCost;
    [SerializeField]
    Transform spawnPoint;
    [SerializeField]
    Transform _satelliteModel;
    [SerializeField]
    Transform _satellitePivot;
    float _rotateProgress;
    float _timeToRotate = 3f;
    Vector3 _originalRotation;
    [SerializeField]
    VisualEffect _visualEffect;

    protected override void OnTileStart ()
    {
        base.OnTileStart();
        _visualEffect.Stop();
        workerCost = _worldController._HQ.workerCost;
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

    /// <summary>
    /// "Used to Instantiate an extra worker"
    /// </summary>
    public void AddUnit (bool requireOre = true)
    {
        if (_Energy > 0)
        {
            if (_worldController._oreCount >= workerCost || requireOre == false)
            {
                StartCoroutine(PlayEffect());
                if (_worldController._workers.Count >= _worldController._workerLimit)
                {
                    _worldController.UIScript.ShowNotification("Max amount of Workers!");
                    return;
                }
                Unit worker = Instantiate(_worker, spawnPoint.position, _worker.transform.rotation);
                _worldController._workers.Add(worker);
                _worldController._levelStatsController.UnitSpawned();
                //Physics.IgnoreCollision(worker.GetComponent<Collider>(), transform.GetComponent<Collider>(), true);
                worker._worldController = _worldController;
                worker._currentTool = Unit.UnitTool.MiningTool;
                worker.SetTool();
                UnitTask newTask = TaskLibrary.Get().CreateTask(UnitTask.TaskType.Walk, worker.transform.position + transform.forward * 4f, null);
                worker.GetComponent<Worker>().AddTask(newTask);
                if (requireOre)
                {
                    ReduceEnergy(5);
                    _worldController._oreCount -= workerCost;
                    _worldController._levelStatsController.OreUsed(workerCost);
                }
                
            }
            else
            {
                _worldController.UIScript.ShowNotification("Not enough Ore!");
            }
        }
    }

    IEnumerator PlayEffect ()
    {
        _visualEffect.Play();
        yield return new WaitForSeconds(1f);
        _visualEffect.Stop();
    }

    private void OnTriggerExit (Collider other)
    {
    }

    public override int GetID ()
    {
        return (int)TileTypeID.Outpost;
    }
}
