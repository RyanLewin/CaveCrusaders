using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public enum MonsterState
    {
        wondering,
        attackingBuilding,
        attackingUnit,
        jump,
        eating,
        BreakWall,
        Flee,
        Hiding
    }
    //navigation
    public NavMeshAgent _navAgent;
    public NavMeshPath _navPath;
    public Vector3 _navigationTarget;
    public float _wonderTimer = 5;

    public float _health;
    public float _speed;
    public bool dead;

    //basic stats
    public int _eatenCrystals;
    [SerializeField] public GameObject _crystalPrefab;
    public float _attackTimer;
    public bool _hiding;

    //annimations
    public Animator _animator;

    public MonsterState _currentMonsterState;

    public enum MonsterType { Hopper, Brute, Slug, Ambush};
    public MonsterType _type;

    //eco
    public GameObject _eggPrefab;
    public float _evolutionTimer;

    //healthbar
    public WorldController _theWorldControllor;
    public HealthBarUpdaterScript _healthBarUpdater;

    //targeting
    public List<Unit> _UnitList;
    public List<Building> _buildingList;
    public List<EnergyCrystal> _crystalList;
    public List<Ore> _oreList;
    public Unit _closestUnit;
    public Building _closestBuilding;
    public EnergyCrystal _closestEnergyCrystal;
    public Ore _closestOre;
    public GameObject _currentTarget;
    public RaycastHit _hit;
    public Ray _ray;
    [SerializeField] public float _attackDamage;

    [SerializeField] protected AISound _aiSound;
    protected bool _monsterActive = true;
    public float Health
    {
        get { return _health; }
        set
        {
            _health = value;
            _healthBarUpdater.UpdateFill(value);
            if(_health <= 0)
            {
                _healthBarUpdater.gameObject.SetActive(false);
            }
        }
    }
    public void TakeDamage(int ammount)
    {
        Health -= ammount;
    }

    private void Awake()
    {
        Renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        Renderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
    }

    List<Renderer> Renderers = new List<Renderer>();

    public void SetVisiblity(bool state)
    {
        
        foreach (Renderer r in Renderers)
        {
            r.enabled = state;
        }
        if (_healthBarUpdater != null)
        {
            if (_health <= 0)
            {
                _healthBarUpdater.gameObject.SetActive(false);
            }
            else
            {
                _healthBarUpdater.gameObject.SetActive(state);
            }
        }
    }

    public void CheckWhatsClosest(bool eatCrystals,bool requiredSight, float DetectionRange)
    {
        bool foundCrystal = false;
        bool foundBuilding = false;

        if (eatCrystals)
        {
            if (_crystalList.Count > 0)
            {
                _closestEnergyCrystal = _crystalList[0];
                foreach (EnergyCrystal possibleLunch in _crystalList)
                {
                    if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance(transform.position, _closestEnergyCrystal.transform.position))
                    {
                        _closestEnergyCrystal = possibleLunch;
                    }
                }
                if (requiredSight)
                {
                    //ray cast to closest crystal
                    _ray = new Ray(transform.position, _closestEnergyCrystal.transform.position - transform.position);
                    Debug.DrawRay(transform.position, _closestEnergyCrystal.transform.position - transform.position);


                    if (Physics.Raycast(_ray, out _hit))
                    {
                        if (_hit.transform.tag == "EnergyCrystal")
                        {
                            _currentMonsterState = MonsterState.eating;
                            foundCrystal = true;
                        }
                    }
                }
                else
                {
                    if (Vector3.Distance(_closestEnergyCrystal.transform.position, transform.position) < DetectionRange)
                    {
                        _currentMonsterState = MonsterState.eating;
                        foundCrystal = true;
                    }
                }
            }
        }        
        if (_buildingList.Count > 0 && !foundCrystal)
        {
            _closestBuilding = _buildingList[0];
            foreach (Building possibleLunch in _buildingList)
            {
                if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance(transform.position, _closestBuilding.transform.position))
                {
                    _closestBuilding = possibleLunch;
                }
            }
            if (requiredSight)
            {
                //ray cast to closest building
                _ray = new Ray(transform.position, _closestBuilding.transform.position - transform.position);
                Debug.DrawRay(transform.position, _closestBuilding.transform.position - transform.position);
                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "HQ" || _hit.transform.tag == "GEN" || _hit.transform.tag == "SKIP")
                    {
                        _currentMonsterState = MonsterState.attackingBuilding;
                        _currentTarget = _closestBuilding.transform.gameObject;
                        foundBuilding = true;
                    }
                }
            }
            else
            {
                if (Vector3.Distance(_closestBuilding.transform.position, transform.position) < DetectionRange)
                {
                    _currentMonsterState = MonsterState.attackingBuilding;
                    _currentTarget = _closestBuilding.transform.gameObject;
                    foundBuilding = true;
                }
            }
        }
        if (_UnitList.Count > 0 && !foundCrystal && !foundBuilding)
        {
            _closestUnit = _UnitList[0];
            foreach (Unit possibleLunch in _UnitList)
            {
                if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance(transform.position, _closestUnit.transform.position))
                {
                    _closestUnit = possibleLunch;
                    
                }
            }
            if (requiredSight)
            {
                //ray cast to closest worker
                _ray = new Ray(transform.position, _closestUnit.transform.position - transform.position);
                Debug.DrawRay(transform.position, _closestUnit.transform.position - transform.position);

                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "Worker" || _hit.transform.tag == "Unit")
                    {
                        _currentMonsterState = MonsterState.attackingUnit;
                        _currentTarget = _closestUnit.transform.gameObject;
                    }
                }
            }
            else
            {
                if (Vector3.Distance(_closestUnit.transform.position, transform.position) < DetectionRange)
                {
                    _currentMonsterState = MonsterState.attackingUnit;
                    _currentTarget = _closestUnit.transform.gameObject;
                }
            }
        }
    }
    public void CheckWhatsClosestMk2(bool energyCrystalPriority, bool unitPriority, bool BuildingPriority, bool requiredSight, float detectionRange, bool eatCrystals)
    {
        if(energyCrystalPriority)
        {
            if(EnergyCrystalSearch(requiredSight,detectionRange) && eatCrystals)
            {
                _currentMonsterState = MonsterState.eating;
            }
            else if(BuildingSearch(requiredSight,detectionRange))
            {
                _currentMonsterState = MonsterState.attackingBuilding;
                _currentTarget = _closestBuilding.transform.gameObject;
            }
            else if(UnitSearch(requiredSight,detectionRange))
            {
                _currentMonsterState = MonsterState.attackingUnit;
                _currentTarget = _closestUnit.transform.gameObject;
            }
        }
        else if(unitPriority)
        {
            if (UnitSearch(requiredSight, detectionRange))
            {
                _currentMonsterState = MonsterState.attackingUnit;
                _currentTarget = _closestUnit.transform.gameObject;
            }
            else if (BuildingSearch(requiredSight, detectionRange))
            {
                _currentMonsterState = MonsterState.attackingBuilding;
                _currentTarget = _closestBuilding.transform.gameObject;
            }
            else if (EnergyCrystalSearch(requiredSight, detectionRange) && eatCrystals)
            {
                _currentMonsterState = MonsterState.eating;
            }
        }
        else if (BuildingPriority)
        {
            if (BuildingSearch(requiredSight, detectionRange))
            {
                _currentMonsterState = MonsterState.attackingBuilding;
                _currentTarget = _closestBuilding.transform.gameObject;
            }
            else if (EnergyCrystalSearch(requiredSight, detectionRange) && eatCrystals)
            {
                _currentMonsterState = MonsterState.eating;
            }
            else if (UnitSearch(requiredSight, detectionRange))
            {
                _currentMonsterState = MonsterState.attackingUnit;
                _currentTarget = _closestUnit.transform.gameObject;
            }
        }
    }
    public bool EnergyCrystalSearch(bool requiredSight, float detectionRange)
    {
        if (_crystalList.Count > 0)
        {
            _closestEnergyCrystal = _crystalList[0];
            foreach (EnergyCrystal possibleLunch in _crystalList)
            {
                float difference = Vector3.Distance(possibleLunch.transform.position, transform.position) - Vector3.Distance(transform.position, _closestEnergyCrystal.transform.position);

                if (difference > 1 || difference < -1)
                {
                    if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance(transform.position, _closestEnergyCrystal.transform.position))
                    {
                        _closestEnergyCrystal = possibleLunch;
                    }
                }
            }
            if (requiredSight)
            {
                //ray cast to closest crystal
                _ray = new Ray(transform.position, _closestEnergyCrystal.transform.position - transform.position);
                Debug.DrawRay(transform.position, _closestEnergyCrystal.transform.position - transform.position);


                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "EnergyCrystal")
                    {                        
                        return true;
                    }
                }                
                                 
            }
            else
            {
                if (Vector3.Distance(_closestEnergyCrystal.transform.position, transform.position) < detectionRange)
                {                    
                    return true;
                }

            }
        }        
            return false;
        
    }
    public bool BuildingSearch(bool requiredSight, float detectionRange)
    {
        if (_buildingList.Count > 0)
        {
            _closestBuilding = _buildingList[0];
            foreach (Building possibleLunch in _buildingList)
            {
                float difference = Vector3.Distance(possibleLunch.transform.position, transform.position) - Vector3.Distance( _closestBuilding.transform.position, transform.position);
                if (difference > 1 || difference < -1)
                {
                    if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance( _closestBuilding.transform.position, transform.position))
                    {
                        _closestBuilding = possibleLunch;
                    }
                }
            }
            if (requiredSight)
            {
                //ray cast to closest building
                _ray = new Ray(transform.position, _closestBuilding.transform.position - transform.position);
                Debug.DrawRay(transform.position, _closestBuilding.transform.position - transform.position);
                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "HQ" || _hit.transform.tag == "GEN" || _hit.transform.tag == "SKIP" || _hit.transform.tag == "OUTPOST" || _hit.transform.tag == "TURRET" || _hit.transform.tag == "POWERGEN" || _hit.transform.tag == "GARAGE")
                    {                        
                        return true;
                    }                    
                }
            }
            else
            {
                if (Vector3.Distance(_closestBuilding.transform.position, transform.position) <= detectionRange)
                {                    
                    return true;
                } 
            }
        }

        return false;
    }
    public bool UnitSearch(bool requiredSight, float detectionRange)
    {
        if (_UnitList.Count > 0)
        {
            _closestUnit = _UnitList[0];
            foreach (Unit possibleLunch in _UnitList)
            {
                float difference = Vector3.Distance(possibleLunch.transform.position, transform.position) - Vector3.Distance(_closestUnit.transform.position, transform.position);
                if (difference > 1 || difference < -1)
                {
                    if (Vector3.Distance(possibleLunch.transform.position, transform.position) < Vector3.Distance(transform.position, _closestUnit.transform.position))
                    {
                        _closestUnit = possibleLunch;
                    }
                }
            }
            if (requiredSight)
            {
                //ray cast to closest worker
                _ray = new Ray(transform.position, _closestUnit.transform.position - transform.position);
                Debug.DrawRay(transform.position, _closestUnit.transform.position - transform.position);

                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "Worker" || _hit.transform.tag == "Unit" || _hit.transform.tag == "Vehicle")
                    {
                        if (_hit.transform.tag == "Worker")
                        {
                            if (!_hit.transform.GetComponent<Worker>()._inVehicle)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Distance(_closestUnit.transform.position, transform.position) < detectionRange)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CanIGetToLocation(Vector3 location)
    {
        Vector3[] cornors;

        cornors = _navAgent.path.corners;

        if (cornors.Length > 0)
        {
            if (Vector3.Distance(cornors[cornors.Length - 1], location) < 9)
            {
                return true;
            }
            else
            {
                Debug.Log("Nope.. Cant get there boss");
                return false;
            }
        }
        else
        {
            return true;
        }

    }
    public void SetMonsterStatus(bool theStatus)
    {
        _monsterActive = theStatus;
    }
}
