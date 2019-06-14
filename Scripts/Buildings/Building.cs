using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Building : Tile
{
    protected WorldController _worldController;
    public float _Energy = 100;
    protected float _EnergyReduction = 1;
    float _EnergyTimer = 3f;
    bool _EnergyRequest = false;
    [SerializeField]
    protected int _Level = 0;
    protected int _MaxLevel = 2;

    [TileAttrib]
    public bool rotated = false;
    [SerializeField]
    Vector3 _OrigPos;
    [TileAttrib]
    public bool _Built = true;
    public VisualEffect smokeVFX;
    float _BuildProgress = 0;
    float _TimeToBuild = 5;
    public Worker _Builder;
    public int _BuildCost = 1;
    public UnitTask _BuildTask;
    public float range = 0;

    public Vector2[] _BuildingSize;
    [SerializeField]
    GameObject[] _BuildTiles;
    public GameObject _BuildingObject;
    public GameObject _PreviewObject;

    [SerializeField] GameObject _fogRange;
    [SerializeField] protected bool _loseEnergy = false;
    public GameObject FogRange { get { return _fogRange; } }
    public bool LoseEnergy { set { _loseEnergy = value; } }
    public bool IsPowered { get { return _Energy > 0; } }
    public int BuildingLevel { get { return _Level; } set { _Level = value; } }

    public enum BuildingType { HQ, Skip, OxyGen, LavaBlockade, Garage, Outpost, H2OConverter, Turret, PowerGen };
    public BuildingType buildingType = BuildingType.HQ;

    public BuildingBarsUpdaterScript _barsUpdater;
    [SerializeField]
    GameObject _VisibleRangeObject;
    protected bool _Destroy = false;

    bool _building = false;
 
    [SerializeField] protected AudioSource _audioSource;

    void Start()
    {
        _barsUpdater = GetComponentInChildren<BuildingBarsUpdaterScript>();
        if (smokeVFX)
            smokeVFX.Stop();
    }

    protected override void OnTileDestroy()
    {
        base.OnTileDestroy();
        WorldController.GetWorldController._buildings.Remove(this);
        if (smokeVFX != null)
        {
            Destroy(smokeVFX.gameObject, 5f);
        }
    }

    public virtual void DestroyBuilding(bool cancelled = false)
    {
        if (smokeVFX)
        {
            smokeVFX.Play();
            smokeVFX.transform.parent = null;
        }
        _Destroy = true;
        _BuildProgress = 0;
        _OrigPos = _BuildingObject.transform.position;

        if (TaskList.DoesTaskExist(_BuildTask))
        {
            TaskList.RemoveTask(_BuildTask);
        }
        if (_Builder)
        {
            _Builder.CancelCurrentTask();
        }

        _worldController._oreCount += Mathf.RoundToInt(cancelled ? _BuildCost : _BuildCost * .8f);
        if (_worldController.CheckStorage())
            _worldController._oreCount = _worldController._maxStorage - _worldController._energyCrystalsCount;
        if (_worldController._oreCount < 0)
        {
            _worldController._oreCount = 0;
        }

    }

    protected override void TileUpdate()
    {
        if (_VisibleRangeObject)
        {
            if (GetComponent<SelectableObject>()._selected)
            {
                _VisibleRangeObject.SetActive(true);
            }
            else
            {
                _VisibleRangeObject.SetActive(false);
            }
        }

        if (_Destroy)
        {
            //Just reussing build progress cause why not
            _BuildProgress += Time.deltaTime;
            _BuildingObject.transform.position = Vector3.Lerp(_OrigPos, new Vector3(_OrigPos.x, _OrigPos.y - 5, _OrigPos.z),
                                                                    _BuildProgress);
            //_BuildingObject.transform.position = new Vector3(_OrigPos.x,
            //                                                  Mathf.Min(_OrigPos.y - 5f + _BuildProgress, _OrigPos.y),
            //                                                  _OrigPos.z);

            if (_BuildProgress >= 1)
            {
                if (_Builder)
                {
                    _Builder.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
                }
                smokeVFX.Stop();
                TileMap3D mpa = WorldController._worldController.GetComponent<TileMap3D>();
                mpa.ClearTile(x, y);
            }
        }

        //if (_Builder && !_Built)
        //{
        //    if (smokeVFX)
        //        smokeVFX.Play();

        //    _BuildProgress += Time.deltaTime / _TimeToBuild;
        //    _BuildingObject.transform.position = Vector3.Lerp(new Vector3(_OrigPos.x, _OrigPos.y - 5, _OrigPos.z),
        //                                                            _OrigPos, _BuildProgress);
        //    //_BuildingObject.transform.position = new Vector3(_OrigPos.x,
        //    //                                                  Mathf.Min(_OrigPos.y - 5f + _BuildProgress, _OrigPos.y),
        //    //                                                  _OrigPos.z);

        //    if (_BuildProgress >= 1)
        //    {
        //        _Built = true;
        //        _Builder.CancelCurrentTask();
        //        _Builder = null;
        //        OnBuilt();
        //    }
        //}

        if (_Built)
        {
            EnergyTimer();
        }


        if (_audioSource != null)
        {
            if (!_Built && _building)
            {
             if(!_audioSource.isPlaying)
                    _audioSource.Play();
                
            }
            else
            {
                _audioSource.Stop();
            }
        }
        //_building = false;


    }
    public void BuildMe (bool b = true)
    {
        _building = b;
    }

    private void FixedUpdate ()
    {
        if (_building)
        {
            if (smokeVFX)
            {
                smokeVFX.Play();
            }

            _BuildProgress += Time.deltaTime / _TimeToBuild;
            _BuildingObject.transform.position = Vector3.Lerp(new Vector3(_OrigPos.x, _OrigPos.y - 7, _OrigPos.z),
                                                                    _OrigPos, _BuildProgress);


            if (_BuildProgress >= 1)
            {
                _Built = true;
                _building = false;
                OnBuilt();
            }
        }
        _building = false;
    }

    /// <summary>
    /// "Affect the health of the building"
    /// </summary>
    public float Health
    {
        get { return _TileHealth; }
        set
        {
            _TileHealth = value;
            _worldController._AlertMode = true;
            if (_TileHealth <= 0)
            {
                TileMap3D mpa = WorldController._worldController.GetComponent<TileMap3D>();
                mpa.ClearTile(x, y);
            }
            _barsUpdater.UpdateHealthFill(value);
        }
    }

    /// <summary>
    /// "Reduce the amount of energy in the building"
    /// </summary>
    protected virtual void EnergyTimer()
    {
        if (_loseEnergy)
        {
            _EnergyTimer -= Time.deltaTime;
            if (_EnergyTimer <= 0)
            {
                ReduceEnergy(_EnergyReduction);
                _EnergyTimer = 3f;
            }
        }
    }

    public void ReduceEnergy(float amt)
    {
        _Energy -= amt;
        _barsUpdater.UpdateEnergyFill(_Energy);

        if (_Energy < 30 && !_EnergyRequest && _worldController._energyCrystalsCount > 0)
        {
            _EnergyRequest = true;
            TaskList.InsertTaskToBeginning(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RefillEnergy, transform.position, gameObject));
            //TaskList.AddTaskToGlobalTaskList(tempTask);
        }

        if (_Energy < 0)
            _Energy = 0;
        if (_Energy > 100)
            _Energy = 100;
    }

    /// <summary>
    /// "When an energy crystal is added, set to max energy"
    /// </summary>
    /// <param name="crystal">"The crystal put into the building"</param>
    public virtual void AddEnergyCrystal(EnergyCrystal crystal)
    {
        _Energy = 100;
        _EnergyRequest = false;
        if (_barsUpdater != null)
        {
            _barsUpdater.UpdateEnergyFill(_Energy);
        }
        _worldController._levelStatsController.EnergyCrystalUsed();
        Destroy(crystal.gameObject);
    }

    /// <summary>
    /// "Called when the player chooses to upgrade a building"
    /// </summary>
    public virtual bool UpgradeBuilding(bool requireCrystal = true)
    {
        if (_worldController._energyCrystalsCount > 0 || requireCrystal == false)
        {
            if (_Level < _MaxLevel)
            {
                _Level++;
                if (requireCrystal)
                    _worldController._energyCrystalsCount--;
                _worldController._levelStatsController.EnergyCrystalUsed();
                return true;
            }
            _worldController.UIScript.ShowNotification("Already Max Level");
        }
        return false;
    }

    /// <summary>
    /// "Called once the building is built"
    /// </summary>
    protected virtual void OnBuilt()
    {
        if (TaskList.DoesTaskExist(_BuildTask))
        {
            TaskList.RemoveTask(_BuildTask);
        }
        GetComponent<OrderVisualizer>().UpdateVisuals(new UnitTask());
        //if (_BuildTiles.Length > 0)
        //{
        //    foreach (GameObject buildTile in _BuildTiles)
        //        buildTile.SetActive(false);
        //}
        _fogRange.SetActive(true);
        _CanFluidEnter = false;
        if (smokeVFX)
            smokeVFX.Stop();
        StartCoroutine(WaitTillBuilt());
    }

    IEnumerator WaitTillBuilt()
    {
        yield return new WaitForSeconds(2f);
        if (_BuildingObject.GetComponentInChildren<Collider>())
        {
            _BuildingObject.GetComponentInChildren<Collider>().enabled = true;
        }
        if (_BuildingObject.GetComponentInChildren<UnityEngine.AI.NavMeshObstacle>())
        {
            _BuildingObject.GetComponentInChildren<UnityEngine.AI.NavMeshObstacle>().enabled = true;
        }
    }

    protected override void OnTileStart()
    {
        _TileDurablity = 1.0f;
        _worldController = WorldController.GetWorldController;
        WorldController.GetWorldController._buildings.Add(this);

        _CanFluidEnter = true;
        CheckIfBuilt();

        _IsDestructable = true;
        //if (buildingSize.Length > 0)
        //{
        //    for (int i = 0; i < buildingSize.Length; i++)
        //    {
        //        Tile t = _worldController.GetComponent<TileMap3D>().GetTileAtPos(X + (int)buildingSize[i].x, Y + (int)buildingSize[i].y);
        //        t.HasBuilding
        //    }
        //}
    }

    public void CheckIfBuilt()
    {
        transform.rotation = rotated ? Quaternion.Euler(new Vector3(0, 90, 0)) : new Quaternion();
        _OrigPos = _BuildingObject.transform.position;

        if (!_Built)
        {
            if (_BuildingObject.GetComponentInChildren<Collider>())
            {
                _BuildingObject.GetComponentInChildren<Collider>().enabled = false;
            }
            if (_BuildingObject.GetComponentInChildren<UnityEngine.AI.NavMeshObstacle>())
            {
                _BuildingObject.GetComponentInChildren<UnityEngine.AI.NavMeshObstacle>().enabled = false;
            }
            _BuildingObject.transform.position = new Vector3(_OrigPos.x, _OrigPos.y - 8f, _OrigPos.z);
            //if (_BuildTiles.Length > 0)
            //{
            //    foreach (GameObject buildTile in _BuildTiles)
            //        buildTile.SetActive(true);
            //}
        }
        else
        {
            _BuildingObject.transform.position = _OrigPos;

            GetComponent<OrderVisualizer>().UpdateVisuals(new UnitTask());
            //if (_BuildTiles.Length > 0)
            //{
            //    foreach (GameObject buildTile in _BuildTiles)
            //        buildTile.SetActive(false);
            //}
            OnBuilt();
        }
    }

    public bool CanUpgrade()
    {
        return (_Level < _MaxLevel && _Built);
    }
}
