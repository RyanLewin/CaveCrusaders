using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.VFX;

public class Worker : Unit
{
    public string _currentTaskType;
    public AudioClip pickHit;
    int _weaponRange = 15;
    [SerializeField] ParticleSystem _theParticleSystem;
    float _shotTimer = 2;
    float _idleTimer;
    TileMap3D _theTilemap;
    float _jumpFloat = 4;
    float _oxegenLossRate = 2;
    bool _flee;
    float _pickUpDelay = 1;
    CPU_FOW FOW;
    bool _shooting = false;
    Ray _ray;
    RaycastHit _hit;
    bool _workerActive = true;

    [SerializeField] WorkerSound _workerSound;
    Light _Light;
    //////tools 
    [SerializeField] float _flamerRange;
    float _flameTimer;

    //energyCrytsal prefab
    [SerializeField] GameObject _energyCrystal;


    public UnitTask CurrentTask { get { return _currentTask; } }
    // Start is called before the first frame update
    void Start()
    {
        _unitType = UnitType.worker;
        _healthBarUpdater = GetComponentInChildren<HealthBarUpdaterScript>();
        _healthBarUpdater._maxHealth = Health;
        _Light = GetComponentInChildren<Light>();
        if (_name != "Gary")
        {
            _name = NameGenerator.GetWorkerName();
        }

        if (_name == "Gary")
        {
            _speed = 6;
        }
        _theAudioSource = GetComponent<AudioSource>();
        _navAgent = GetComponent<NavMeshAgent>();
        _selectableObject = GetComponent<SelectableObject>();
        //_currentTool = UnitTool.MiningTool;
        _worldController = WorldController.GetWorldController;
        _navAgent.speed = _speed;
        _monsterList = new List<Monster>();
        SetTool();
        _theTilemap = _worldController.GetComponent<TileMap3D>();
        _workerVoices = WorkerVoices.GetWorkerVoices;
        _taskLibary = TaskLibrary.Get();
        _line = GetComponent<LineRenderer>();
        _destinationTolerance = 1;
        _flameTimer = 3;
        FOW = CPU_FOW.Get();
        _idleTimer = Random.Range(7, 10);

    }

    // Update is called once per frame
    void Update()
    {
        if (_workerActive)
        {
            if (!_navAgent.pathPending && _selectableObject._selected && _navAgent.path.corners.Length > 0 && _navAgent.remainingDistance >= 0.1f)
            {
                DrawPath();
                _endPointIndercator.transform.position = new Vector3(_navAgent.path.corners[_navAgent.path.corners.Length - 1].x, _navAgent.path.corners[_navAgent.path.corners.Length - 1].y, _navAgent.path.corners[_navAgent.path.corners.Length - 1].z);
            }
            else
            {
                _line.positionCount = 0;
                _endPointIndercator.transform.position = transform.position + new Vector3(0, -10, 0);
            }
            
            if (!Dead)
            {
                UpdateFluidRipple();
                CheckHealth();

                

                CheckTile();
                _navAgent.speed = _speed;

                if (_useO2)
                {
                    Health -= Time.deltaTime / _oxegenLossRate;
                    if (_currentTask == null)
                    {
                        CheckOxygen();
                    }
                    else if (_currentTask._taskType != UnitTask.TaskType.RefillOxygen)
                    {
                        CheckOxygen();
                    }
                }
                if (_currentTool == UnitTool.Weapon && _worldController._AlertMode && _worldController._monsters.Count > 0)
                {
                    if (_flee)
                    {
                        _flee = false;
                    }
                    if (_currentTask != null)
                    {
                        if (_currentTask._taskType == UnitTask.TaskType.Attack)
                        {
                            FindClosestRockMonster();
                            if (_closestMonster != null)
                            {
                                if (_currentTask._targetMonster != _closestMonster)
                                {
                                    UnitTask tempTask = _taskLibary.CreateTask(UnitTask.TaskType.Attack, _closestMonster.transform.position, _closestMonster.gameObject);
                                    SetTask(tempTask, false);
                                }
                            }
                        }
                        else if (_currentTask._taskType != UnitTask.TaskType.RefillOxygen && _currentTask._taskType != UnitTask.TaskType.Walk)
                        {
                            FindClosestRockMonster();
                            if (_closestMonster != null)
                            {
                                UnitTask tempTask = _taskLibary.CreateTask(UnitTask.TaskType.Attack, _closestMonster.transform.position, _closestMonster.gameObject);
                                SetTask(tempTask, false);
                            }
                        }

                    }
                    else
                    {
                        FindClosestRockMonster();
                        if (_closestMonster != null)
                        {
                            UnitTask tempTask = _taskLibary.CreateTask(UnitTask.TaskType.Attack, _closestMonster.transform.position, _closestMonster.gameObject);
                            SetTask(tempTask, false);
                        }

                    }
                }
                if (!CheckMonster())
                {
                    if (_currentTask != null)
                    {
                        RunTask();
                    }
                    else
                    {
                        GetTask();
                    }
                }

                if (WorldController._worldController.DEBUG_FAST_WORKER_MOVE)
                {
                    _navAgent.speed = 50.0f;
                    _navAgent.acceleration = 200;
                }
                else
                {
                    _navAgent.speed = _speed;
                    _navAgent.acceleration = 8;
                }
                CheckHealth();
            }
            else
            {
                if (_Light != null)
                {
                    _Light.enabled = false;
                }
            }
        }
    }
    /// <summary>
    /// Runs the current task that the worker has. the function will call GetTask() when current task is complete
    /// </summary>
    void RunTask()
    {
        switch (_currentTask._taskType)
        {
            ////////////////////////////////////////////////////////  ATTACK  ///////////////////////////////////////////////////////////////
            case UnitTask.TaskType.Attack:

                if (_currentTask._targetMonster != null)
                {
                    if (!FOW.SampleFOW(_currentTask._targetMonster.transform.position) || _currentTask._targetMonster._hiding || _currentTask._targetMonster.Health <= 0)
                    {
                        GetTask();
                        break;
                    }

                    if (Vector3.Distance(transform.position, _currentTask._targetMonster.transform.position) > _weaponRange && !_shooting)// || _hit.transform != _currentTask._targetMonster.transform)
                    {
                        _navAgent.SetDestination(_currentTask._targetMonster.transform.position);
                        _shotTimer -= Time.deltaTime;
                        _animator.SetBool("Walking", true);
                        _animator.SetBool("Shoot", false);
                    }
                    else
                    {
                        _shooting = true;
                        //int layerMask = 1 << 10;
                        //layerMask = ~layerMask;
                        //if (Physics.Raycast(transform.position, _currentTask._targetMonster.transform.position, out _hit, _weaponRange + 5, layerMask))
                        //{
                        //    if (_hit.transform.gameObject == _currentTask._targetMonster.gameObject)
                        //    {
                        //      //  Debug.DrawRay(transform.position, _currentTask._targetMonster.transform.position - transform.position, Color.green);
                        //        print("Positive hit");
                        //    }
                        //    else
                        //    {
                        //     //   Debug.DrawRay(transform.position, _currentTask._targetMonster.transform.position - transform.position, Color.red);
                        //    }
                        //}

                        _navAgent.ResetPath();
                        _animator.SetBool("Shoot", true);
                        _animator.SetBool("Walking", false);                        
                        transform.LookAt(new Vector3(_currentTask._targetMonster.transform.position.x, transform.position.y, _currentTask._targetMonster.transform.position.z));
                        

                        if (Vector3.Distance(transform.position, _currentTask._targetMonster.transform.position) > _weaponRange + 1)
                        {
                            _shooting = false;
                        }
                    }

                }
                else
                {
                    GetTask();
                }

                break;
            /////////////////////////////////////////////////////// REFILL OXEGEN  ////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.RefillOxygen:
                if (_targetOxegenBuilding == null)
                {
                    GetTask();
                    break;
                }
                if (!_taskSetup)
                {
                    _navAgent.SetDestination(_currentTask._location + new Vector3 (Random.Range(-1,1),0, Random.Range(-1, 1)));
                    _animator.SetBool("Walking", true);
                    _taskSetup = true;
                }
                if (!_navAgent.pathPending)
                {
                    if (!CanIGetToLocation(_currentTask._location))
                    {
                       // Debug.Log("Canceled: " + _currentTask._taskDescription);
                        _currentTask.DestroyTask();
                        _currentTask = null;
                        break;
                    }
                }
                if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                {
                    OxygenGenerator oxyGen = _targetOxegenBuilding.GetComponent<OxygenGenerator>();
                    oxyGen.ChangeInWorkers(true, this);
                    if (_name != "Gary")
                    {
                        _navAgent.ResetPath();
                        _animator.SetBool("Walking", false);
                        if (Health < 100 && _targetOxegenBuilding.IsPowered)
                        {
                            Health += 20 * (oxyGen.BuildingLevel + 1) * Time.deltaTime;
                            _targetOxegenBuilding.ReduceEnergy(Time.deltaTime * 2);
                        }
                        else if (!_targetOxegenBuilding.IsPowered)
                        {
                            oxyGen.ChangeInWorkers(false, this);
                            GetTask();
                            break;
                        }
                        else
                        {
                            oxyGen.ChangeInWorkers(false, this);
                            Health = 100;
                            GetTask();
                            break;
                        }
                    }
                    else
                    {
                        _navAgent.ResetPath();
                        _animator.SetBool("Walking", false);
                        if (Health < 200 && _targetOxegenBuilding.IsPowered)
                        {
                            Health += 20 * (oxyGen.BuildingLevel + 1) * Time.deltaTime;
                            _targetOxegenBuilding.ReduceEnergy(Time.deltaTime * 2);
                        }
                        else if (!_targetOxegenBuilding.IsPowered)
                        {
                            oxyGen.ChangeInWorkers(false, this);
                            GetTask();
                            break;
                        }
                        else
                        {
                            oxyGen.ChangeInWorkers(false, this);
                            Health = 200;
                            GetTask();
                            break;
                        }
                    }
                }
                break;
            /////////////////////////////////////////////////////////   BUILD   //////////////////////////////////////////////////////////////
            case UnitTask.TaskType.Build:

                if (_currentTask._targetBuilding != null)
                {
                    if (_currentTool == UnitTool.Hammer)
                    {
                        if (!_taskSetup)
                        {
                            _navAgent.SetDestination(_currentTask._location);
                            transform.GetChild(0).transform.Translate(new Vector3(0, 0.01f, 0));
                            _animator.SetBool("Walking", true);
                            _taskSetup = true;
                        }
                        if (!_navAgent.pathPending && _currentTask._location != null)
                        {
                            if (!CanIGetToLocation(_currentTask._location))
                            {
                              //  Debug.Log("Canceled: " + _currentTask._taskDescription);
                                _currentTask.DestroyTask();
                                _currentTask = null;
                                break;
                            }
                        }
                        if (Vector3.Distance(_currentTask._targetBuilding.transform.position, transform.position) < 9.0f && Vector3.Distance(_navAgent.destination, transform.position) > 2.0f)
                        {
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Build", true);

                            if (transform.GetChild(0).transform.position.y > 0)
                            {
                                _jumpFloat -= Time.deltaTime * 3;
                                transform.GetChild(0).transform.Translate(new Vector3(0, _jumpFloat, 0) * Time.deltaTime);
                            }

                        }
                        else if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                        {
                            _navAgent.ResetPath();
                            if (_currentTask._targetBuilding._Built)
                            {
                                _navAgent.SetDestination(_currentTask._targetBuilding.transform.position + new Vector3(8, 0, 0));
                                transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
                                GetComponentInChildren<Light>().enabled = true;
                                _currentToolModel.GetComponent<MeshRenderer>().enabled = true;
                                _animator.SetBool("Build", false);

                                GetTask();
                            }
                            else
                            {
                                _currentTask._targetBuilding.BuildMe();
                                GetComponentInChildren<Light>().enabled = false;
                                transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
                                _currentToolModel.GetComponent<MeshRenderer>().enabled = false;
                                if (transform.GetChild(0).transform.position.y > 0)
                                {
                                    transform.GetChild(0).transform.Translate(new Vector3(0, -transform.GetChild(0).transform.position.y, 0));
                                }
                            }
                        }

                    }
                    else
                    {
                        GetTask();
                        transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
                        _currentToolModel.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                else
                {
                    GetTask();
                    transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
                    _currentToolModel.GetComponent<MeshRenderer>().enabled = true;
                }
                break;
            ///////////////////////////////////////////////////// MINE  //////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.Mine:

                if (_currentTask._targetRock != null)
                {

                    if (_currentTool == UnitTool.MiningTool)
                    {
                        if (!_taskSetup)
                        {
                            _navAgent.SetDestination(_currentTask._location);
                            _taskSetup = true;
                        }
                        if (!_navAgent.pathPending)
                        {
                            if (!CanIGetToLocation(_currentTask._location))
                            {
                               // Debug.Log("Canceled: " + _currentTask._taskDescription);
                                _currentTask.DestroyTask();
                                _currentTask = null;
                                break;
                            }

                            if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                            {
                                if (_currentTask._targetRock == null)
                                {
                                    _taskSetup = false;

                                    if (_worldController._miningLevel == WorldController.MiningLevel.two)
                                    {
                                        _currentToolModel.transform.GetChild(2).GetComponent<ParticleSystem>().Stop();
                                    }
                                    GetTask();
                                }
                                _navAgent.ResetPath();


                                if (_worldController._miningLevel == WorldController.MiningLevel.one)
                                {
                                    transform.LookAt(new Vector3(_currentTask._targetRock.transform.position.x, transform.position.y, _currentTask._targetRock.transform.position.z));
                                }
                                else if (_worldController._miningLevel == WorldController.MiningLevel.two)
                                {
                                    Vector3 tempVect = _currentTask._targetRock.transform.position + (transform.right * 4);
                                    transform.LookAt(new Vector3(tempVect.x, transform.position.y, tempVect.z));
                                    _currentTask._targetRock.Mined(1.0f);
                                    _currentToolModel.transform.GetChild(1).transform.Rotate(new Vector3(0, 0, -20));
                                    _currentToolModel.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                                }
                                else
                                {
                                    Vector3 tempVect = _currentTask._targetRock.transform.position + (transform.right * 4);
                                    transform.LookAt(new Vector3(tempVect.x, transform.position.y, tempVect.z));
                                    _currentTask._targetRock.Mined(1.0f);
                                    _currentToolModel.transform.GetChild(0).GetChild(0).GetChild(0).transform.Rotate(new Vector3(0, 0, -20));
                                }

                                //call animation                   
                                _animator.SetBool("Mining", true);
                                _animator.SetBool("Walking", false);
                            }
                            else
                            {
                                _animator.SetBool("Walking", true);
                            }
                        }
                    }
                    else
                    {
                        GetTask();
                    }
                }
                else
                {
                    GetTask();
                    if (_worldController._miningLevel == WorldController.MiningLevel.two)
                    {
                        _currentToolModel.transform.GetChild(2).GetComponent<ParticleSystem>().Stop();
                    }
                }

                break;
            /////////////////////////////////////////////  PICK UP   //////////////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.Pickup:

                if (_currentTask._itemToPickUp != null)
                {
                    if (!_taskSetup)
                    {
                        //using the location of item and location of storage check which is closer. if  storage is full
                        //get new task

                        //check if full
                        if (_worldController.CheckStorage())
                        {
                          //  Debug.Log("Storage full 1");
                            _worldController.UIScript.ShowNotification("Storage Full");
                            TaskList.AddTaskToGlobalTaskList(_currentTask);
                            GetTask();
                            break;
                        }

                        //null out target building
                        _targetBuilding = null;

                        //go to item
                        _navAgent.SetDestination(_currentTask._itemToPickUp.transform.position);
                        _animator.SetBool("Walking", true);

                        if (_targetBuilding == null)
                        {
                            FindclosestStorageBuilding();
                        }
                        _taskSetup = true;
                    }
                    else if (Vector3.Distance(transform.position, _currentTask._itemToPickUp.transform.position) > 2 && !_itemInHands)
                    {
                        //go to item
                        _navAgent.SetDestination(_currentTask._itemToPickUp.transform.position);
                        _animator.SetBool("Walking", true);

                        //check if full
                        if (_worldController.CheckStorage())
                        {
                           // Debug.Log("Storage full 2");
                            _worldController.UIScript.ShowNotification("Storage Full");
                            UnitTask tempTask = _currentTask;
                            _currentTask = null;
                            TaskList.AddTaskToGlobalTaskList(tempTask);
                            GetTask();
                            break;
                        }
                    }
                    else if (Vector3.Distance(transform.position, _currentTask._itemToPickUp.transform.position) < 2 && !_itemInHands)
                    {//pick up item    
                        FindclosestStorageBuilding();
                        //check if full
                        if (_worldController.CheckStorage())
                        {
                           // Debug.Log("Storage full 3");
                            _worldController.UIScript.ShowNotification("Storage Full");
                            UnitTask tempTask = _currentTask;
                            _currentTask = null;
                            TaskList.AddTaskToGlobalTaskList(tempTask);
                            GetTask();
                            break;
                        }

                        if (_pickUpDelay == 1)
                        {
                            _workerSound.PlayHumming();
                            _animator.SetBool("Carrying", true);
                            _animator.SetBool("Walking", false);
                            _theParticleSystem.Play();
                        }

                        if (_pickUpDelay <= 0)
                        {
                            Destroy(_currentTask._itemToPickUp.GetComponent<Rigidbody>());
                            _currentTask._itemToPickUp.transform.parent = _carryToolSocket.transform.GetChild(0).transform;
                            _itemInHands = true;
                            _navAgent.SetDestination(_targetBuilding.transform.position + new Vector3(0,0,1));

                        }
                        else
                        {
                            _navAgent.ResetPath();
                            _pickUpDelay -= Time.deltaTime;
                        }


                    }
                    if (_navAgent.remainingDistance < _destinationTolerance + 3 && !_navAgent.pathPending && _itemInHands)
                    {//drop off item
                        //check if full
                        if (_worldController.CheckStorage())
                        {
                          // Debug.Log("Storage full 4");
                            _worldController.UIScript.ShowNotification("Storage Full");
                            UnitTask tempTask = _currentTask;
                            _currentTask._itemToPickUp.transform.parent = null;
                            _currentTask._itemToPickUp.AddComponent<Rigidbody>();
                            _currentTask = null;
                            TaskList.AddTaskToGlobalTaskList(tempTask);

                            _itemInHands = false;
                            GetTask();

                            break;
                        }

                        if (_currentTask._itemType == UnitTask.ItemType.Ore)
                        {
                            _worldController._oreCount++;
                            _worldController._levelStatsController.OreColleded();
                        }
                        else if (_currentTask._itemType == UnitTask.ItemType.EnergyCrystal)
                        {
                            _worldController._energyCrystalsCount++;
                            _worldController._levelStatsController.EnergyCrystalCollected();
                            _worldController._energyCrystals.Remove(_currentTask._itemToPickUp.GetComponent<EnergyCrystal>());
                        }
                        _itemInHands = false;
                        Destroy(_currentTask._itemToPickUp);
                        _theParticleSystem.Stop();
                        GetTask();
                        break;
                    }

                }
                else
                {
                    GetTask();
                    break;
                }

                if (_itemInHands)
                {
                    float rx = 0;
                    float ry = 0;
                    float rz = 0;

                    if (_currentTask._itemToPickUp.transform.rotation.x > 0)
                    {
                        rx = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.x < 0)
                    {
                        rx = 1;
                    }
                    if (_currentTask._itemToPickUp.transform.rotation.y > 0)
                    {
                        ry = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.y < 0)
                    {
                        ry = 1;
                    }
                    if (_currentTask._itemToPickUp.transform.rotation.z > 0)
                    {
                        rz = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.z < 0)
                    {
                        rz = 1;
                    }
                    _currentTask._itemToPickUp.transform.Rotate(new Vector3(rx, ry, rz));


                    _currentTask._itemToPickUp.transform.position = Vector3.MoveTowards(_currentTask._itemToPickUp.transform.position, _carryToolSocket.transform.GetChild(0).transform.position, Time.deltaTime);
                }

                break;
            /////////////////////////////////////////////////////////////  Reinforce  //////////////////////////////////////////////////////////
            case UnitTask.TaskType.Reinforce:

                GetTask();

                break;
            //////////////////////////////////////////////////////////////  WALK  /////////////////////////////////////////////////////////
            case UnitTask.TaskType.Walk:

                if (!_taskSetup)
                {
                    _navAgent.SetDestination(_currentTask._location);

                    _taskSetup = true;

                }
                if (!_navAgent.pathPending)
                {
                    if (!CanIGetToLocation(_currentTask._location))
                    {
                        GetTask();
                        break;
                    }
                    _animator.SetBool("Walking", true);

                    if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                    {
                        GetTask();
                    }
                }

                break;
            /////////////////////////////////////////////////////////  GET TOOL  //////////////////////////////////////////////////////////////
            case UnitTask.TaskType.GetTool:

                if (!_taskSetup)
                {
                    _navAgent.SetDestination(_currentTask._location + new Vector3(0, 0, 1));
                    _animator.SetBool("Walking", true);
                    _taskSetup = true;
                }
                if (!_navAgent.pathPending)
                {
                    if (!CanIGetToLocation(_currentTask._location))
                    {
                      //  Debug.Log("Canceled: " + _currentTask._taskDescription);
                        _currentTask.DestroyTask();
                        _currentTask = null;
                        break;
                    }
                }
                if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                {
                    _currentTool = _currentTask._requiredTool;
                    SetTool();
                    GetTask();
                }
                break;
            //////////////////////////////////////////////////////   REFILL ENERGY  /////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.RefillEnergy:

                if (_currentTask._targetBuilding == null && !_itemInHands)
                {
                    CancelCurrentTask();
                    GetTask();
                    break;
                }
                else if (_currentTask._targetBuilding == null && _itemInHands)
                {
                    FindclosestStorageBuilding();
                    _currentTask._location = _targetBuilding.transform.position;
                    _currentTask._taskType = UnitTask.TaskType.Pickup;
                    _currentTask._taskDescription = "Transporting an Energy crystal";
                    _currentTask._itemType = UnitTask.ItemType.EnergyCrystal;
                    _currentTask._requiredTool = Unit.UnitTool.none;

                    _navAgent.SetDestination(_targetBuilding.transform.position);
                    break;
                }

                if (!_taskSetup)
                {
                    if (_worldController._energyCrystalsCount <= 0)
                    {
                        GetTask();
                        break;
                    }

                    FindclosestStorageBuilding();
                    _navAgent.SetDestination(_targetBuilding.transform.position);
                    _animator.SetBool("Walking", true);
                    _taskSetup = true;
                }
                else if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending && !_itemInHands)
                {//pick up item                    
                    _currentTask._itemToPickUp = Instantiate(_energyCrystal, _carryToolSocket.transform.GetChild(0).transform.position, _energyCrystal.transform.rotation);
                    Destroy(_currentTask._itemToPickUp.GetComponent<Rigidbody>());
                    _currentTask._itemToPickUp.transform.parent = _carryToolSocket.transform.GetChild(0).transform;
                    _currentTask._itemToPickUp.GetComponent<EnergyCrystal>()._addTask = false;
                    _navAgent.SetDestination(_currentTask._location);
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Carrying", true);
                    _theParticleSystem.Play();
                    _itemInHands = true;
                    _worldController._energyCrystalsCount--;
                    // _worldController._levelStatsController.EnergyCrystalUsed();
                }
                else if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending && _itemInHands)
                {//drop off item
                    _currentTask._targetBuilding.AddEnergyCrystal(_currentTask._itemToPickUp.GetComponent<EnergyCrystal>());
                    Destroy(_currentTask._itemToPickUp);
                    _itemInHands = false;
                    _theParticleSystem.Stop();
                    _animator.SetBool("Carrying", false);
                    GetTask();
                    break;
                }

                if (_itemInHands && _currentTask._itemToPickUp != null)
                {
                    float rx = 0;
                    float ry = 0;
                    float rz = 0;

                    if (_currentTask._itemToPickUp.transform.rotation.x > 0)
                    {
                        rx = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.x < 0)
                    {
                        rx = 1;
                    }
                    if (_currentTask._itemToPickUp.transform.rotation.y > 0)
                    {
                        ry = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.y < 0)
                    {
                        ry = 1;
                    }
                    if (_currentTask._itemToPickUp.transform.rotation.z > 0)
                    {
                        rz = -1;
                    }
                    else if (_currentTask._itemToPickUp.transform.rotation.z < 0)
                    {
                        rz = 1;
                    }
                    _currentTask._itemToPickUp.transform.Rotate(new Vector3(rx, ry, rz));


                    _currentTask._itemToPickUp.transform.position = Vector3.MoveTowards(_currentTask._itemToPickUp.transform.position, _carryToolSocket.transform.GetChild(0).transform.position, Time.deltaTime);
                }

                break;
            //////////////////////////////////////////////////////  CLEAR RUBBLE /////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.ClearRubble:
                if (_currentTask._targetRubble != null)
                {

                    if (_currentTool == UnitTool.Shovel)
                    {
                        if (!_taskSetup)
                        {
                            _navAgent.SetDestination(_currentTask._location);
                            _animator.SetBool("Walking", true);
                            _taskSetup = true;
                        }
                        if (!_navAgent.pathPending)
                        {
                            if (!CanIGetToLocation(_currentTask._location))
                            {
                              //  Debug.Log("Canceled: " + _currentTask._taskDescription);
                                _currentTask.DestroyTask();
                                _currentTask = null;
                                break;
                            }
                        }
                        if (_navAgent.remainingDistance < _destinationTolerance && !_navAgent.pathPending)
                        {
                            if (_currentTask._targetRubble == null)
                            {
                                GetTask();
                            }
                            _navAgent.ResetPath();
                            _currentTask._targetRubble.Dig();
                            //call animation                   
                            _animator.SetBool("Digging", true);
                            _animator.SetBool("Walking", false);

                        }
                    }
                    else
                    {
                        GetTask();
                    }
                }
                else
                {
                    GetTask();
                }

                break;
            /////////////////////////////////////////////////  IDLE //////////////////////////////////////////////////////////////////////
            case UnitTask.TaskType.Idle:

                if (TaskList.Tasks.Count > 0)
                {
                    GetTask();
                }
                else
                {

                    if (_idleTimer <= 0)
                    {
                        _navAgent.SetDestination(_currentTask._location);
                        _animator.SetBool("Walking", true);
                        _idleTimer = Random.Range(7, 10);
                        _currentTask._location = transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
                    }
                    else
                    {
                        _idleTimer -= Time.deltaTime;
                    }

                    if (_navAgent.remainingDistance <= _destinationTolerance && !_navAgent.pathPending)
                    {
                        _animator.SetBool("Walking", false);
                        _navAgent.ResetPath();
                    }
                }

                break;
            ///////////////////////////////////////////////////////// GET IN Vehicle //////////////////////////////////////////////////////////////
            case UnitTask.TaskType.GetInVehicle:
                if (!_taskSetup)
                {
                    if (_currentTask._targetVehicle.GetOccupied())
                    {
                        GetTask();
                        break;
                    }

                    _navAgent.SetDestination(_currentTask._targetVehicle.transform.position);
                    _animator.SetBool("Walking", true);
                    _taskSetup = true;
                }
                if (!_navAgent.pathPending)
                {
                    if (!CanIGetToLocation(_currentTask._location))
                    {
                       // Debug.Log("Canceled: " + _currentTask._taskDescription);
                        _currentTask.DestroyTask();
                        _currentTask = null;
                        break;
                    }
                }
                if (_navAgent.remainingDistance < _destinationTolerance + 2 && !_navAgent.pathPending)
                {
                    //_currentTask._targetVehicle.SetOccupied(true);
                    _currentTask._targetVehicle.Embark(gameObject);
                }
                break;
            ///////////////////////////////////////////////////// FLame Target //////////////////////////////////////////////////////////            
            case UnitTask.TaskType.flameTarget:
                if (_currentTool == UnitTool.FlameThrower)
                {
                    if (_closestMushroom == null)
                    {
                        _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
                        FindClosestMushroom();
                        if (_closestMushroom != null)
                        {
                            UnitTask newTask = TaskLibrary.Get().CreateTask(UnitTask.TaskType.flameTarget, _closestMushroom.parent.position, _closestMushroom.parent.gameObject);
                            SetTask(newTask);
                        }
                    }
                    if (_currentTask._targetMushroom == null)
                    {
                        GetTask();
                        break;
                    }

                    if (!_taskSetup)
                    {
                        _navAgent.SetDestination(_currentTask._location);
                        _taskSetup = true;
                    }
                    if (!_navAgent.pathPending)
                    {
                        if (!CanIGetToLocation(_currentTask._location))
                        {
                            _currentTask.DestroyTask();
                            GetTask();
                            break;
                        }
                    }
                    if (Vector3.Distance(transform.position, _currentTask._location) < _destinationTolerance + 4)
                    {
                        _navAgent.ResetPath();
                        //FLAME THROWER!!
                        if (_flameTimer > 0)
                        {
                            _flameTimer -= Time.deltaTime;
                            transform.LookAt(_currentTask._location);
                            _currentToolModel.transform.LookAt(_currentTask._location);
                            _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(true);
                            _currentTask._targetMushroom.Burn(_currentToolModel);
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Flame", true);
                        }
                        else
                        {
                            _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
                            _workerSound.StopFlamethrower();
                            StartCoroutine(FlamethrowerDelay());
                        }
                        _workerSound.Flamethrower();
                    }
                    else
                    {
                        _animator.SetBool("Walking", true);
                        _animator.SetBool("Flame", false);
                    }
                }
                else
                {
                    GetTask();
                }

                break;
        }
    }

    /// <summary>
    /// Timer before flamethrower needs to recharge
    /// </summary>
    IEnumerator FlamethrowerDelay()
    {
        yield return new WaitForSeconds(2f);
        _flameTimer = 3;
    }

    /// <summary>
    /// Gets a new task for the worker
    /// </summary>
    void GetTask()
    {
        GetComponentInChildren<Light>().enabled = true;
        UnitTask tempTask;
        int tempIterator;
        _pickUpDelay = 1;

        _taskSetup = false;
        if (_navAgent.isOnNavMesh)
        {
            _navAgent.ResetPath();
        }

        _jumpFloat = 4;

        //reset animations
        _animator.SetBool("Walking", false);
        _animator.SetBool("Mining", false);
        _animator.SetBool("Carrying", false);
        _animator.SetBool("Digging", false);
        _animator.SetBool("Build", false);
        _animator.SetBool("Flee", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Flame", false);

        _theParticleSystem.Stop();

        if (_currentTool == UnitTool.FlameThrower)
        {
            _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
        }

        if (_currentTask != null)
        {
            if (_currentTask._taskType == UnitTask.TaskType.Build)
            {
                transform.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                _currentToolModel.GetComponent<MeshRenderer>().enabled = true;
            }
            _currentTask = null;
        }


        //check if there is a task in local list
        if (_localTaskList.Count > 0)
        {
            _currentTask = _localTaskList[0];
            _localTaskList.RemoveAt(0);
        }
        //get task from taskList
        else if (TaskList.Tasks.Count > 0)
        {
            //check it has the correct tool for the job
            for (int i = 0; i < TaskList.Tasks.Count; i++)
            {
                if (TaskList.Tasks[i]._requiredTool == UnitTool.none || TaskList.Tasks[i]._requiredTool == _currentTool)
                {
                    if (TaskList.Tasks[i]._taskType == UnitTask.TaskType.Pickup)
                    {
                        //TODO --- if full or cant pickup ignor task
                        if (!_worldController.CheckStorage() && _canPickup)
                        {
                            tempTask = TaskList.Tasks[i];
                            tempIterator = i;
                            for (int j = i + 1; j < TaskList.Tasks.Count; j++)
                            {
                                if (TaskList.Tasks[j]._taskType == UnitTask.TaskType.Pickup)
                                {
                                    if (Vector3.Distance(transform.position, tempTask._location) > Vector3.Distance(transform.position, TaskList.Tasks[j]._location))
                                    {
                                        tempTask = TaskList.Tasks[j];
                                        tempIterator = j;
                                    }
                                }
                            }

                            _currentTask = tempTask;
                            TaskList.Tasks.RemoveAt(tempIterator);
                            break;
                        }
                    }
                    else
                    {
                        _currentTask = TaskList.Tasks[i];
                        TaskList.Tasks.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        else
        {//idle if nothing to do
            _currentTask = new UnitTask
            {
                _taskType = UnitTask.TaskType.Idle,
                _location = transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)),
                _taskDescription = "Idleing About"
            };
        }

    }
    /// <summary>
    /// Function to overite the current task the worker is performing
    /// </summary>
    /// <param name="theTask">The task that the worker will change too</param>
    /// <param name="makeSound">Set to true if you want the worker to make the ordered voice line</param>
    public void SetTask(UnitTask theTask, bool makeSound = false)
    {
        GetComponentInChildren<Light>().enabled = true;
        UnitTask tempTask;
        _pickUpDelay = 1;

        //reset animations
        _animator.SetBool("Walking", false);
        _animator.SetBool("Mining", false);
        _animator.SetBool("Carrying", false);
        _animator.SetBool("Digging", false);
        _animator.SetBool("Build", false);
        _animator.SetBool("Flee", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Flame", false);
        _theParticleSystem.Stop();
        _jumpFloat = 4;

        _localTaskList.Clear();
        _taskSetup = false;
        if (_navAgent.isOnNavMesh)
        {
            _navAgent.ResetPath();
        }

        if (transform.GetChild(0).transform.position.y > 0)
        {
            transform.GetChild(0).transform.Translate(new Vector3(0, -transform.GetChild(0).transform.position.y, 0));
        }


        if (_currentTask != null)
        {
            tempTask = _currentTask;
            _currentTask = null;
            if (tempTask._taskType != UnitTask.TaskType.Walk
                && tempTask._taskType != UnitTask.TaskType.GetTool
                && tempTask._taskType != UnitTask.TaskType.RefillOxygen
                && tempTask._taskType != UnitTask.TaskType.Idle
                && tempTask._taskType != UnitTask.TaskType.none
                && tempTask._taskType != UnitTask.TaskType.Attack)
            {
                TaskList.AddTaskToGlobalTaskList(tempTask);
            }

            switch (tempTask._taskType)
            {
                case UnitTask.TaskType.Pickup:
                    if (_itemInHands)
                    {
                        tempTask._itemToPickUp.transform.parent = null;
                        tempTask._itemToPickUp.AddComponent<Rigidbody>();
                        _itemInHands = false;
                    }
                    break;
                case UnitTask.TaskType.Build:

                    //_currentTask._targetBuilding._Builder = null;
                    transform.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                    _currentToolModel.GetComponent<MeshRenderer>().enabled = true;

                    break;
                case UnitTask.TaskType.flameTarget:

                    if (_currentTool == UnitTool.FlameThrower)
                    {
                        _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
                    }

                    break;
                case UnitTask.TaskType.RefillEnergy:
                    if (_itemInHands)
                    {
                        tempTask._itemToPickUp.transform.parent = null;
                        tempTask._itemToPickUp.AddComponent<Rigidbody>();
                        _itemInHands = false;

                        UnitTask newTask = new UnitTask
                        {
                            _itemToPickUp = tempTask._itemToPickUp,
                            _location = _targetBuilding.transform.position,
                            _taskType = UnitTask.TaskType.Pickup,
                            _taskDescription = "Transporting an Energy crystal",
                            _itemType = UnitTask.ItemType.EnergyCrystal,
                            _requiredTool = Unit.UnitTool.none
                        };
                        TaskList.AddTaskToGlobalTaskList(newTask);
                    }
                    break;

            }
        }
        if (makeSound)
        {
            playOrderSound();
        }
        if (theTask != null)
        {
            _currentTask = theTask;
            theTask.UpdateTask();
        }
        else
        {
            _currentTask = null;
        }
    }
    /// <summary>
    /// Adds a task to the end of the Workers task list
    /// </summary>
    /// <param name="theTask">Task to be Added</param>
    public void AddTask(UnitTask theTask)
    {
        theTask.UpdateTask();
        playOrderSound();
        _localTaskList.Add(theTask);
    }
    /// <summary>
    /// Finds the closest storage building and sets _targetbuilding to it
    /// </summary>
    void FindclosestStorageBuilding()
    {
        List<Building> buildingList = _worldController._buildings;
        List<Building> sortedBList = new List<Building>();

        foreach (Building aBuilding in buildingList)
        {
            if (aBuilding.tag == "HQ" || aBuilding.tag == "SKIP")
            {
                sortedBList.Add(aBuilding);
            }
        }

        foreach (Building building in sortedBList)
        {
            if (building._Built)
            {
                if (_targetBuilding == null)
                {
                    _targetBuilding = building;
                }
                else
                {
                    if (Vector3.Distance(transform.position, building.transform.position) < Vector3.Distance(transform.position, _targetBuilding.transform.position))
                    {
                        _targetBuilding = building;
                    }
                }
            }
        }
        if (_targetBuilding == null)
        {
            GetTask();
        }
    }
    /// <summary>
    /// Returns the current task the worker is doing
    /// </summary>
    /// <returns>returns a UnitTask type with current tasks varibles set </returns>
    public UnitTask GetCurrentTask()
    {
        return _currentTask;
    }
    bool CheckMonster()
    {
        _monsterList = _worldController._monsters;
        if (_monsterList.Count > 0)
        {
            if (_currentTask != null)
            {
                if (_currentTask._taskType != UnitTask.TaskType.Attack)
                {
                    if (!_worldController._AlertMode)
                    {
                        return RunFromMonster();
                    }
                    else
                    {
                        if (_currentTool != UnitTool.Weapon)
                        {
                            return RunFromMonster();
                        }
                        else
                        {
                            if (_currentTask._taskType != UnitTask.TaskType.Walk)
                            {
                                return RunFromMonster();
                            }
                        }
                    }
                }
            }
            else
            {
                return RunFromMonster();
            }
        }
        return false;
    }
    bool RunFromMonster()
    {
        //Ray _ray;
        RaycastHit _hit;

        FindClosestRockMonster();
        if (_closestMonster == null)
        {
            return false;
        }
        if (Vector3.Distance(_closestMonster.transform.position, transform.position) < 10 || _flee)
        {
            if (Vector3.Distance(_closestMonster.transform.position, transform.position) > 12)
            {
                _flee = false;
                return false;
            }

            //ray cast to closest monster
            // _ray = new Ray(transform.position, _closestMonster.transform.position - transform.position);
           // Debug.DrawRay(transform.position, _closestMonster.transform.position - transform.position);
            int layerMask = 1 << 10;
            layerMask = ~layerMask;
            if (Physics.Raycast(transform.position, _closestMonster.transform.position - transform.position, out _hit, 20, layerMask))
            {
                if (_hit.transform.tag == "Monster")
                {
                    

                    if (_currentTask != null)
                    {
                        if (_currentTask._taskType == UnitTask.TaskType.Pickup)
                        {
                            if (_itemInHands)
                            {
                                _currentTask._itemToPickUp.transform.parent = null;
                                _currentTask._itemToPickUp.AddComponent<Rigidbody>();
                                _itemInHands = false;
                            }
                        }
                        _taskSetup = false;
                        _localTaskList.Add(_currentTask);
                        _currentTask = null;
                    }
                    _navAgent.ResetPath();
                    _navAgent.SetDestination(transform.position - (_closestMonster.transform.position - transform.position));
                    _animator.SetBool("Flee", true);
                    _animator.SetBool("Mining", false);
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Digging", false);
                    _animator.SetBool("Carrying", false);
                    _animator.SetBool("Flame", false);

                    if (_currentTool == UnitTool.FlameThrower)
                    {
                        _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
                    }
                    _flee = true;
                    

                    return true;
                }                
                else
                {
                    _flee = false;
                    return false;

                }
            }
            else
            {
                _flee = false;
                return false;
            }
        }
        else
        {
            _flee = false;
            return false;
        }
    }
    void CheckOxygen()
    {
        _targetOxegenBuilding = null;
        FindclosestOxegenBuilding();
        if (_targetOxegenBuilding != null)
        {
            int currentSetting = (int)SettingScript.instance.RiskLevel;
            float timeToGen = Vector3.Distance(transform.position, _targetOxegenBuilding.transform.position) / _speed / _oxegenLossRate;

            if (timeToGen > (Health - (currentSetting + 10)))
            {
                UnitTask temptask = _taskLibary.CreateTask(UnitTask.TaskType.RefillOxygen, _targetOxegenBuilding.transform.position, gameObject);

                SetTask(temptask, false);
            }
        }


    }
    void CheckHealth()
    {
        if (Health < 0)
        {
            //print("Dead");
            _navAgent.ResetPath();
            Destroy(GetComponent<NavMeshAgent>());
            UnitTask tempTask = new UnitTask();
            SetTask(tempTask, false);
            _animator.SetBool("Walking", false);
            _animator.SetBool("Mining", false);
            _animator.SetBool("Carrying", false);
            _animator.SetBool("Flame", false);
            _animator.SetBool("Dead", true);
            _animator.SetBool("Flee", false);
            _animator.Play("Death");
            

            if (_currentTool == UnitTool.FlameThrower)
            {
                _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
            }
            Dead = true;
            _workerActive = false;

            _healthBarUpdater.gameObject.SetActive(false);
            _worldController._workers.Remove(this);
            _worldController._levelStatsController.UnitKilled();
            Destroy(_fowGameObject);
            Destroy(_healthBars);
            Destroy(MinimapIcon);
            _endPointIndercator.transform.position = transform.position + new Vector3(0, -10, 0);
            _line.positionCount = 0;
            
            Garage.SetLayerRecursive(gameObject, 0);

            UnitToolInfoScript.instance.UpdateDisplay();

        }
    }
    /// <summary>
    /// Finds the closest Oxegen building and sets _targetbuilding to it
    /// </summary>
    void FindclosestOxegenBuilding()
    {
        List<Building> buildingList = _worldController._buildings;
        List<Building> sortedBList = new List<Building>();

        foreach (Building aBuilding in buildingList)
        {
            if (aBuilding.tag == "GEN" && aBuilding._Built && aBuilding.IsPowered)
            {
                sortedBList.Add(aBuilding);
            }
        }

        foreach (Building building in sortedBList)
        {
            if (building._Built)
            {
                if (_targetOxegenBuilding == null)
                {
                    _targetOxegenBuilding = building;
                }
                else
                {
                    if (Vector3.Distance(transform.position, building.transform.position) < Vector3.Distance(transform.position, _targetOxegenBuilding.transform.position))
                    {
                        _targetOxegenBuilding = building;
                    }
                }
            }
        }

    }
    void FindClosestRockMonster()
    {
        _closestMonster = _monsterList[0];
        foreach (Monster theMonster in _monsterList)
        {
            float diffence = Vector3.Distance(theMonster.transform.position, transform.position) - Vector3.Distance(_closestMonster.transform.position, transform.position);
            if (diffence > 1f || diffence < -1f)
            {
                if (Vector3.Distance(theMonster.transform.position, transform.position) < Vector3.Distance(transform.position, _closestMonster.transform.position) && FOW.SampleFOW(theMonster.transform.position))
                {
                    _closestMonster = theMonster;
                }
            }
        }        
        if (!FOW.SampleFOW(_closestMonster.transform.position) || _closestMonster._hiding)
        {           
            _closestMonster = null;
        }
    }

    void FindClosestMushroom()
    {
        if (_worldController._mushroomClusters.Count > 0)
        {
            _closestMushroom = _worldController._mushroomClusters[0].transform.GetChild(0);
            if (_closestMushroom == null)
            {
                return;
            }
            foreach (MushroomCluster mushroom in _worldController._mushroomClusters)
            {
                if (!mushroom)
                    continue;
                if (Vector3.Distance(mushroom.transform.position, transform.position) < Vector3.Distance(transform.position, _closestMushroom.transform.position) && FOW.SampleFOW(mushroom.transform.position))
                {
                    if (Vector3.Distance(mushroom.transform.position, transform.position) < 8f)
                    {
                        continue;
                    }
                    if (mushroom.transform.GetChild(0).localScale.x > .8f)
                        _closestMushroom = mushroom.transform.GetChild(0);
                }
            }

            if (!FOW.SampleFOW(_closestMushroom.transform.position))
            {
                _closestMushroom = null;
            }
        }
    }




    void CheckTaskViability()
    {
        if (_currentTask != null && !_navAgent.pathPending)
        {
            //check if the worker can get to the location
            if (Vector3.Distance(_navAgent.pathEndPosition, _currentTask._location) > 10)
            {
                TaskList.AddTaskToGlobalTaskList(_currentTask);
                GetTask();
            }
        }
    }
    public void CancelCurrentTask()
    {
        if (_currentTask != null)
        {
            if(_itemInHands)
            {
                _currentTask._itemToPickUp.transform.parent = null;
                _currentTask._itemToPickUp.AddComponent<Rigidbody>();
            }
            _currentTask.DestroyTask();

        }
        
        GetTask();
    }

    /// <summary>
    /// Checks if current task already exists elsewhere
    /// </summary>
    /// <returns>Bool if task already exists</returns>
    bool IsTaskDupe()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                if (worker != this && (worker.GetCurrentTask() != null))
                {
                    if (TaskMenuScript.AreTasksSame(_currentTask, worker.GetCurrentTask()))
                    {
                        return true;
                    }
                }
            }
        }

        foreach (UnitTask task in TaskList.Tasks)
        {
            if (TaskMenuScript.AreTasksSame(_currentTask, task))
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// A function to check what the status of the tile the worker is currently walking on
    /// </summary>
    void CheckTile()
    {
        Tile currentTile = _theTilemap.FindCell(transform.position);
        if (currentTile == null)
        {
            return;
        }
        if (currentTile.GetFluidLevel() > FluidEngine.MIN_SIM_VALUE && currentTile.CurrentFluidType == Fluid.FluidType.Lava)
        {
            Health -= 30 * Time.deltaTime;
        }
        else if (currentTile.GetFluidLevel() > FluidEngine.MIN_SIM_VALUE && currentTile.CurrentFluidType == Fluid.FluidType.Water)
        {
            if (_name != "Gary")
            {
                _speed = 1.5f;
                _animator.SetFloat("SpeedVar", 0.5f);
            }
            else
            {
                _speed = 3f;
                _animator.SetFloat("SpeedVar", 1f);
            }
        }
        else if (currentTile.gameObject.tag == "Rubble")
        {
            if (_name != "Gary")
            {
                _speed = 1.5f;
                _animator.SetFloat("SpeedVar", 0.5f);
            }
            else
            {
                _speed = 3f;
                _animator.SetFloat("SpeedVar", 1f);
            }
        }
        else
        {
            if (_name != "Gary")
            {
                _speed = 3;
                _animator.SetFloat("SpeedVar", 1.0f);
            }
            else
            {
                _speed = 6;
                _animator.SetFloat("SpeedVar", 1.0f);
            }
        }
    }
    public void SetStats(string name, float health, UnitTool theTool)
    {
        _name = name;
        Health = health;
        _currentTool = theTool;
        SetTool();
    }
    public void DeathByLandslide()
    {
        UnitTask tempTask = new UnitTask();
        SetTask(tempTask, false);
        _animator.SetBool("Walking", false);
        _animator.SetBool("Mining", false);
        _animator.SetBool("Carrying", false);
        _animator.SetBool("Flame", false);
        _animator.SetBool("Dead", true);
        _animator.Play("Death");

        if (_currentTool == UnitTool.FlameThrower)
        {
            _currentToolModel.GetComponent<Flamethrower>().TurnOnOff(false);
        }
        Dead = true;
        _worldController._workers.Remove(this);
        _worldController._levelStatsController.UnitKilled();
        Destroy(gameObject);
        UnitToolInfoScript.instance.UpdateDisplay();
    }
    void playOrderSound()
    {
        if (_theAudioSource.isPlaying)
        {
            return;
        }
        AudioClip theClip;

        switch (_name)
        {
            case "Grumpy Dave":
                theClip = _workerVoices._grumpyDaveeOrder[Random.Range(0, _workerVoices._grumpyDaveeOrder.Count)];
                break;
            case "Scottish Dave-ette":
                theClip = _workerVoices._scottDaveetteOrder[Random.Range(0, _workerVoices._scottDaveetteOrder.Count)];
                break;
            case "Irish Dave":
                theClip = _workerVoices._irishDaveeOrder[Random.Range(0, _workerVoices._irishDaveeOrder.Count)];
                break;
            case "Southern Dave":
                theClip = _workerVoices._southernDaveeOrder[Random.Range(0, _workerVoices._southernDaveeOrder.Count)];
                break;
            case "Dave-ette":
                theClip = _workerVoices._daveEtteOrder[Random.Range(0, _workerVoices._daveEtteOrder.Count)];
                break;
            case "Irish Dave-ette":
                theClip = _workerVoices._irishDaveetteOrder[Random.Range(0, _workerVoices._irishDaveetteOrder.Count)];
                break;
            case "The Other Dave-ette":
                theClip = _workerVoices._theOtherdaveEtteOrder[Random.Range(0, _workerVoices._theOtherdaveEtteOrder.Count)];
                break;
            case "Dave From Bristol":
                theClip = _workerVoices._bristolDaveeOrder[Random.Range(0, _workerVoices._bristolDaveeOrder.Count)];
                break;
            case "The Other Irish Dave-ette":
                theClip = _workerVoices._theOtherdaveEtteOrder[Random.Range(0, _workerVoices._theOtherdaveEtteOrder.Count)];
                break;
            case "Southern Dave-ette":
                theClip = _workerVoices._southernDaveetteOrder[Random.Range(0, _workerVoices._southernDaveetteOrder.Count)];
                break;
            default:
                theClip = _workerVoices._bristolDaveeOrder[Random.Range(0, _workerVoices._bristolDaveeOrder.Count)];
                break;
        }

        _theAudioSource.PlayOneShot(theClip);
    }
    public void PlaySelectionSound()
    {
        AudioClip theClip;

        switch (_name)
        {
            case "Grumpy Dave":
                theClip = _workerVoices._grumpyDaveSelect[Random.Range(0, _workerVoices._grumpyDaveSelect.Count)];
                break;
            case "Scottish Dave-ette":
                theClip = _workerVoices._scottDaveetteSelect[Random.Range(0, _workerVoices._scottDaveetteSelect.Count)];
                break;
            case "Irish Dave":
                theClip = _workerVoices._irishDaveSelect[Random.Range(0, _workerVoices._irishDaveSelect.Count)];
                break;
            case "Southern Dave":
                theClip = _workerVoices._southernDaveSelect[Random.Range(0, _workerVoices._southernDaveSelect.Count)];
                break;
            case "Dave-ette":
                theClip = _workerVoices._daveEtteSelect[Random.Range(0, _workerVoices._daveEtteSelect.Count)];
                break;
            case "Irish Dave-ette":
                theClip = _workerVoices._irishDaveetteSelect[Random.Range(0, _workerVoices._irishDaveetteSelect.Count)];
                break;
            case "The Other Dave-ette":
                theClip = _workerVoices._theOtherdaveEtteSelect[Random.Range(0, _workerVoices._theOtherdaveEtteSelect.Count)];
                break;
            case "Dave From Bristol":
                theClip = _workerVoices._bristolDaveSelect[Random.Range(0, _workerVoices._bristolDaveSelect.Count)];
                break;
            case "The Other Irish Dave-ette":
                theClip = _workerVoices._theOtherdaveEtteSelect[Random.Range(0, _workerVoices._theOtherdaveEtteSelect.Count)];
                break;
            case "Southern Dave-ette":
                theClip = _workerVoices._southernDaveetteSelect[Random.Range(0, _workerVoices._southernDaveetteSelect.Count)];
                break;
            default:
                theClip = _workerVoices._bristolDaveSelect[Random.Range(0, _workerVoices._bristolDaveSelect.Count)];
                break;
        }

        _theAudioSource.PlayOneShot(theClip);
    }
    [Header("VFX")]
    [SerializeField]
    VisualEffect RippleVFX;
    [SerializeField]
    float SpawnRateFactor = 10.0f;
    [SerializeField]
    float MinSpeed = 0.2f;
    [SerializeField]
    float TransfromScale = 3.0f;
    void UpdateFluidRipple()
    {
        Tile currentTile = _theTilemap.FindCell(transform.position);
        if (currentTile == null)
        {
            return;
        }
        float FluidHeight = currentTile.GetFluidLevel();
        if (RippleVFX != null)
        {
            if (currentTile.CurrentFluidType == Fluid.FluidType.None || FluidHeight < FluidEngine.MIN_SIM_VALUE)
            {
                RippleVFX.Stop();
                return;
            }
            RippleVFX.Play();
            Vector3 VFXpos = RippleVFX.transform.position;
            VFXpos.y = FluidHeight * TransfromScale;
            RippleVFX.transform.position = VFXpos;

            float Speed = _navAgent.velocity.magnitude;
            if (Speed < MinSpeed)
            {
                Speed = 0.0f;
            }
            RippleVFX.SetFloat("SpawnRate", Speed * SpawnRateFactor);
        }
        if (_Light != null)
        {
            _Light.enabled = !(currentTile.CurrentFluidType == Fluid.FluidType.Water && FluidHeight > 0.1f);
        }
    }
    /// <summary>
    /// Turn a worker on or off usin this function
    /// </summary>
    /// <param name="theStatus">enter true for on false for off</param>
    public void SetWorkersStatus(bool theStatus)
    {
        _workerActive = theStatus;
        if (_navAgent == null)
        {
            _navAgent = GetComponent<NavMeshAgent>();
        }
        if (_navAgent != null)
        {
            _navAgent.enabled = theStatus;
        }
    }

}
