using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public enum UnitTool
    {
        none,
        MiningTool,
        Hammer,
        Weapon,
        Shovel,
        FlameThrower
    }
    public enum UnitType
    {
        worker,
        vehicle
    }
    public enum VehicleType
    {
        smallDrill,
        smallTransport,
    }
    //////tools 
    public GameObject _toolSocket;
    public GameObject _carryToolSocket;
    public GameObject _currentToolModel;

    //drillbits
    public GameObject _drillBit;
    public GameObject _laserDrillBit;
    public VehicleType _vType;

    //tool prefabs
    public GameObject _bulletPrefab;
    public GameObject _pickAxePrefab;
    public GameObject _DrillPrefab;
    public GameObject _LasrDrillPrefab;
    public GameObject _laserPistolPrefab;
    public GameObject _laserCannonPrefab;
    public GameObject _hammerPrefab;
    public GameObject _shovelPrefab;
    public GameObject _flamerPrefab;

    //materials
    public Material _workerMinerMat;
    public Material _workerBuilderMat;
    public Material _workerComabatMat;
    public Material _workerDiggerMat;
    public Material _workerDriverMat;
    public Material _workerFlamerMat;
    public Material _GaryMat;

    //basic stats
    public float _health;
    public float _speed;
    public string _name;
    public UnitType _unitType;
    public bool _occupied;
    public GameObject _theDriver;
    public UnitTask _currentTask;
    public HealthBarUpdaterScript _healthBarUpdater;

    //navigation
    public NavMeshAgent _navAgent;
    public NavMeshPath _navPath;
    public Vector3 _navigationTarget;
    public float _destinationTolerance;

    //navigation drawing
    public LineRenderer _line;
    public GameObject _endPointIndercator;

    //annimations
    public Animator _animator;

    //WorldScript reference
    public WorldController _worldController;

    //audio
    public AudioSource _theAudioSource;
    public WorkerVoices _workerVoices;

    //health Bar Stoof
    public GameObject _healthBars;
    [SerializeField]
    protected GameObject MinimapIcon;

    //misc
    [SerializeField] public GameObject _fowGameObject;
    public bool _selected;
    public SelectableObject _selectableObject;
    public UnitTool _currentTool;
    public List<UnitTask> _localTaskList = new List<UnitTask>();
    public bool _itemInHands;
    public bool _taskSetup;
    public Building _targetBuilding;
    public Building _targetOxegenBuilding;
    [SerializeField] public List<Monster> _monsterList;
    public bool _fleeingMonster;
    public Monster _closestMonster;
    public Transform _closestMushroom;
    public TaskLibrary _taskLibary;

    public bool _useO2 = true;
    public bool _canPickup = true;
    public bool Dead = false;
    public bool _inVehicle = false;

    [SerializeField] GameObject _fogRange;
    public GameObject FogRange { get { return _fogRange; } }

    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _worldController = WorldController.GetWorldController;

    }
    public bool CanIGetToLocation(Vector3 location)
    {
        Vector3[] cornors;

        cornors = _navAgent.path.corners;

        if (cornors.Length > 0)
        {
            if (Vector3.Distance(cornors[cornors.Length - 1], location) < 10)
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

    public void DrawPath()
    {
        //set the array of positions to the amount of corners
        _line.positionCount = _navAgent.path.corners.Length;

        for (int i = 0; i < _navAgent.path.corners.Length; i++)
        {
            _line.SetPosition(i, _navAgent.path.corners[i]); //go through each corner and set that to the line renderer's position
        }

    }
    public void SetTool()
    {
        if (_unitType == UnitType.worker)
        {
            UnitToolInfoScript.instance.UpdateDisplay();
            if (_worldController._miningLevel == WorldController.MiningLevel.two)
            {
                _animator.SetInteger("ToolLevel", 2);
            }
            else if (_worldController._miningLevel == WorldController.MiningLevel.three)
            {
                _animator.SetInteger("ToolLevel", 3);
            }

            if (_currentToolModel != null)
            {
                Destroy(_currentToolModel);
            }

            switch (_currentTool)
            {
                case UnitTool.Hammer:
                    _currentToolModel = Instantiate(_hammerPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                    //_currentToolModel.transform.rotation = new Quaternion(0, 0, -180, 0);
                    _animator.SetBool("Two_Handed_Tool", false);
                    if (_name != "Gary")
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerBuilderMat;
                    }
                    else
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                    }
                    break;
                case UnitTool.MiningTool:
                    if (_worldController._miningLevel == WorldController.MiningLevel.one)
                    {
                        _currentToolModel = Instantiate(_pickAxePrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                        _animator.SetBool("Two_Handed_Tool", false);
                    }
                    else if (_worldController._miningLevel == WorldController.MiningLevel.two)
                    {
                        _currentToolModel = Instantiate(_DrillPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                        _animator.SetBool("Two_Handed_Tool", true);
                    }
                    else
                    {
                        _currentToolModel = Instantiate(_LasrDrillPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                        _animator.SetBool("Two_Handed_Tool", true);
                    }
                    if (_name != "Gary")
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerMinerMat;
                    }
                    else
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                    }
                    break;
                case UnitTool.Weapon:
                    if (_worldController._miningLevel == WorldController.MiningLevel.two)
                    {
                        _currentToolModel = Instantiate(_laserPistolPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                        _animator.SetBool("Two_Handed_Tool", false);
                        if (_name != "Gary")
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerComabatMat;
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                        }
                    }
                    else if (_worldController._miningLevel == WorldController.MiningLevel.three)
                    {
                        _currentToolModel = Instantiate(_laserCannonPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                        _animator.SetBool("Two_Handed_Tool", true);
                        if (_name != "Gary")
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerComabatMat;
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                        }
                    }
                    else
                    {
                        Debug.Log("Can not spawn weapon: incorrect tool level");
                    }

                    break;
                case UnitTool.Shovel:
                    _currentToolModel = Instantiate(_shovelPrefab, _toolSocket.transform.position, _toolSocket.transform.rotation, _toolSocket.transform);
                    _currentToolModel.transform.rotation = new Quaternion(0, 0, 0, 0);
                    if (_name != "Gary")
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerDiggerMat;
                    }
                    else
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                    }
                    break;
                case UnitTool.FlameThrower:
                    _currentToolModel = Instantiate(_flamerPrefab, _toolSocket.transform.position, new Quaternion(0, 0, 90, 0), _toolSocket.transform);
                    _currentToolModel.transform.localRotation = Quaternion.Euler(0, 15, 30);
                    _animator.SetBool("Two_Handed_Tool", true);
                    if (_name != "Gary")
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerFlamerMat;
                    }
                    else
                    {
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                    }
                    break;
                default:
                    if (GetComponent<Worker>())
                    {
                        if (_name != "Gary")
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _workerMinerMat;
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material = _GaryMat;
                        }
                    }
                    break;
            }

            if (_currentToolModel != null)
            {
                MeshRenderer[] tempList = _currentToolModel.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < tempList.Length; i++)
                {
                    tempList[i].enabled = !_inVehicle;
                }

                if (_currentToolModel.GetComponent<MeshRenderer>() != null)
                {
                    _currentToolModel.GetComponent<MeshRenderer>().enabled = !_inVehicle;
                }

            }
        }
        else if (_unitType == UnitType.vehicle)
        {
            if (_vType == VehicleType.smallDrill && _worldController._miningLevel == WorldController.MiningLevel.three)
            {
                _drillBit.GetComponent<MeshRenderer>().enabled = false;
                _laserDrillBit.GetComponent<MeshRenderer>().enabled = true;
            }
        }

    }

    public void TakeDamage(float ammount)
    {
        Health -= ammount;
    }
    public float Health
    {

        get { return _health; }
        set
        {
            if (_healthBarUpdater != null)
            {
                _health = value;
                _healthBarUpdater.UpdateFill(value);
            }
        }
    }
}

