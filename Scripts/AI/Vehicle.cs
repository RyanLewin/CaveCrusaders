using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Vehicle : Unit
{


    /*bool _occupied;*/ //serialised for testing
    string _driversName;
    float _driversHealth;
    UnitTool _driversTool;
    float _energyLevel;
    bool _gotItem = false;
    public BuildingBarsUpdaterScript _barsUpdater;
    //GameObject _theDriver;

    //storage
    GameObject _storageItemOne;
    GameObject _storageItemTwo;
    GameObject _storageItemThree;
    UnitTask.ItemType _storageOneType;
    UnitTask.ItemType _storageTwoType;
    UnitTask.ItemType _storageThreeType;
    [SerializeField] GameObject _storageSlotOne;
    [SerializeField] GameObject _storageSlotTwo;
    [SerializeField] GameObject _storageSlotThree;


    [SerializeField] SkinnedMeshRenderer _driverMesh;
    [SerializeField] GameObject _workerPrefab;

    [SerializeField] GameObject _frontLeftWheel;
    [SerializeField] GameObject _frontRightWheel;
    [SerializeField] GameObject _backRightWheel;
    [SerializeField] GameObject _backLeftWheel;

    [SerializeField] VehicleSound _vehicleSound;

    // Start is called before the first frame update
    void Start()
    {
        _unitType = UnitType.vehicle;
        _barsUpdater = GetComponentInChildren<BuildingBarsUpdaterScript>();
        _barsUpdater._maxHealth = Health;

        _navAgent = GetComponent<NavMeshAgent>();
        _worldController = WorldController.GetWorldController;
        _worldController._workers.Add(this);
        _localTaskList = new List<UnitTask>();
        _navAgent.speed = _speed;
        Energy = 100;
        _taskLibary = TaskLibrary.Get();
        //SetTool();

    }

    // Update is called once per frame
    void Update()
    {
        CheckEnergy();
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

        if (_occupied)
        {
            Energy -= Time.deltaTime / 2;

            if (_navAgent.remainingDistance > 0 && !_navAgent.pathPending)
            {
                _frontLeftWheel.transform.Rotate(new Vector3(0, 0, 10));
                _frontRightWheel.transform.Rotate(new Vector3(0, 0, 10));
                _backLeftWheel.transform.Rotate(new Vector3(0, 0, 10));
                _backRightWheel.transform.Rotate(new Vector3(0, 0, 10));
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


        }
    }
    void RunTask()
    {
        switch (_currentTask._taskType)
        {
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
                }
                if (_navAgent.remainingDistance < 0.5f && !_navAgent.pathPending)
                {
                    GetTask();
                }
                break;
            case UnitTask.TaskType.Mine:
                if (_currentTask._targetRock != null)
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
                            GetTask();
                            break;
                        }

                        //if (Vector3.Distance(_currentTask._targetRock.transform.position, transform.position) < 7f)
                        if (_navAgent.remainingDistance < 3f && !_navAgent.pathPending)
                        {
                            if (_currentTask._targetRock == null)
                            {
                                _taskSetup = false;
                                GetTask();
                            }
                            _navAgent.ResetPath();
                            transform.LookAt(_currentTask._targetRock.transform.position + new Vector3(0, 1.5f, 0));

                            if (_worldController._miningLevel == WorldController.MiningLevel.three)
                            {
                                _laserDrillBit.transform.Rotate(0, 0, -20);
                                _currentTask._targetRock.Mined(5);
                            }
                            else
                            {
                                _drillBit.transform.Rotate(-20, 0, 0);
                                _currentTask._targetRock.Mined(3);
                            }


                            _vehicleSound.Mining();
                        }
                    }
                }
                else
                {
                    GetTask();
                }

                break;
            ////////////////////////////////////////// Recharge Vehicle  /////////////////////////////////////////////////
            case UnitTask.TaskType.RechargeVehicle:
                if (!_taskSetup)
                {                   
                    //FindClosestBuildingWithTag("GARAGE", "");
                    if (_targetBuilding == null)
                    {
                        GetTask();
                        break;
                    }

                    _navAgent.SetDestination(_currentTask._location);

                    _taskSetup = true;
                }

                //if (Vector3.Distance(_currentTask._location, transform.position) < 3f)
                if (_navAgent.remainingDistance < 0.5f && !_navAgent.pathPending)
                {
                    _navAgent.ResetPath();
                    if (Energy < 100 && _targetBuilding.IsPowered)
                    {
                        Energy += 20 * (_targetBuilding.BuildingLevel + 1) * Time.deltaTime;
                        _targetBuilding.ReduceEnergy(Time.deltaTime * 2);
                    }
                    else if (!_targetBuilding.IsPowered)
                    {
                        GetTask();
                        break;
                    }
                    else
                    {
                        Energy = 100;
                        GetTask();
                    }

                }

                

                break;
            //////////////////////////////////////////////   PickUp   //////////////////////////////////////////////////////////////////
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
                            Debug.Log("Storage full");
                            _worldController.UIScript.ShowNotification("Storage Full");
                            GetTask();
                            break;
                        }

                        //go to item
                        _navAgent.SetDestination(_currentTask._itemToPickUp.transform.position);

                        _targetBuilding = FindClosestBuildingWithTag("SKIP", "HQ");

                        _taskSetup = true;
                    }
                    else if (_navAgent.remainingDistance > 0.5f && !_navAgent.pathPending && !_gotItem)
                    {
                        //go to item
                        _navAgent.SetDestination(_currentTask._itemToPickUp.transform.position);
                    }
                    if (_navAgent.remainingDistance < 0.5f && !_navAgent.pathPending && !_gotItem)
                    {//pick up item                    

                        Destroy(_currentTask._itemToPickUp.GetComponent<Rigidbody>());
                        _targetBuilding = FindClosestBuildingWithTag("SKIP", "HQ");
                        if (_storageItemOne == null)
                        {
                            _currentTask._itemToPickUp.transform.position = _storageSlotOne.transform.position;
                            _currentTask._itemToPickUp.transform.parent = _storageSlotOne.transform;
                            _storageItemOne = _currentTask._itemToPickUp;
                            _storageOneType = _currentTask._itemType;
                            _gotItem = true;
                            GetTask();
                            break;
                        }
                        else if (_storageItemTwo == null)
                        {
                            _currentTask._itemToPickUp.transform.position = _storageSlotTwo.transform.position;
                            _currentTask._itemToPickUp.transform.parent = _storageSlotTwo.transform;
                            _storageItemTwo = _currentTask._itemToPickUp;
                            _storageTwoType = _currentTask._itemType;
                            _gotItem = true;
                            GetTask();
                            break;
                        }
                        else if (_storageItemThree == null)
                        {
                            _currentTask._itemToPickUp.transform.position = _storageSlotThree.transform.position;
                            _currentTask._itemToPickUp.transform.parent = _storageSlotThree.transform;
                            _storageItemThree = _currentTask._itemToPickUp;
                            _storageThreeType = _currentTask._itemType;
                            _gotItem = true;
                        }

                        _gotItem = true;

                        _navAgent.SetDestination(_targetBuilding.transform.position);
                    }
                    else if (_navAgent.remainingDistance < 0.5f && !_navAgent.pathPending && _gotItem)
                    {//drop off item

                        if (_storageItemOne != null)
                        {
                            if (_storageOneType == UnitTask.ItemType.Ore)
                            {
                                _worldController._oreCount++;
                                _worldController._levelStatsController.OreColleded();
                            }
                            else if (_storageOneType == UnitTask.ItemType.EnergyCrystal)
                            {
                                _worldController._energyCrystalsCount++;
                                _worldController._levelStatsController.EnergyCrystalCollected();
                            }
                            Destroy(_storageItemOne);
                        }
                        if (_storageItemTwo != null)
                        {
                            if (_storageTwoType == UnitTask.ItemType.Ore)
                            {
                                _worldController._oreCount++;
                                _worldController._levelStatsController.OreColleded();
                            }
                            else if (_storageTwoType == UnitTask.ItemType.EnergyCrystal)
                            {
                                _worldController._energyCrystalsCount++;
                                _worldController._levelStatsController.EnergyCrystalCollected();
                            }
                            Destroy(_storageItemTwo);
                        }
                        if (_storageItemThree != null)
                        {
                            if (_storageThreeType == UnitTask.ItemType.Ore)
                            {
                                _worldController._oreCount++;
                                _worldController._levelStatsController.OreColleded();
                            }
                            else if (_storageThreeType == UnitTask.ItemType.EnergyCrystal)
                            {
                                _worldController._energyCrystalsCount++;
                                _worldController._levelStatsController.EnergyCrystalCollected();
                            }
                            Destroy(_storageItemThree);
                        }

                        GetTask();
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
    /// Gets the next task off the task list and resets the unit
    /// </summary>
    void GetTask()
    {//drill or move tasks only
        _taskSetup = false;
        _currentTask = null;
        _gotItem = false;

        //check if there is a task in local list
        if (_localTaskList.Count > 0)
        {
            _currentTask = _localTaskList[0];
            _localTaskList.RemoveAt(0);
            Debug.Log("Got Task from local list");
        }

        switch (_vType)
        {
            case VehicleType.smallDrill:
                //get task from taskList. small drill will only get drill tasks
                if (TaskList.Tasks.Count > 0)
                {
                    for (int i = 0; i < TaskList.Tasks.Count; i++)
                    {
                        if (TaskList.Tasks[i]._taskType == UnitTask.TaskType.Mine)
                        {
                            _currentTask = TaskList.Tasks[i];
                            TaskList.Tasks.RemoveAt(i);
                            break;
                        }
                    }
                }
                break;
            case VehicleType.smallTransport:
                //get task from taskList. small drill will only get drill tasks
                if (TaskList.Tasks.Count > 0)
                {
                    for (int i = 0; i < TaskList.Tasks.Count; i++)
                    {
                        if (TaskList.Tasks[i]._taskType == UnitTask.TaskType.Pickup && !_worldController.CheckStorage())
                        {
                            _currentTask = TaskList.Tasks[i];
                            TaskList.Tasks.RemoveAt(i);
                            break;
                        }
                    }
                }
                break;
        }
    }
    public void SetTask(UnitTask theTask)
    {
        _taskSetup = false;
        _gotItem = false;
        PlayOrderSound();

        switch (_vType)
        {
            case VehicleType.smallDrill:
                if (theTask != null)
                {
                    if (theTask._taskType == UnitTask.TaskType.Mine || theTask._taskType == UnitTask.TaskType.RechargeVehicle || theTask._taskType == UnitTask.TaskType.Walk)
                    {
                        if (_currentTask != null)
                        {
                            if (_currentTask._taskType != UnitTask.TaskType.RechargeVehicle)
                            {
                                if (_currentTask._taskType == UnitTask.TaskType.Mine)
                                {
                                    TaskList.AddTaskToGlobalTaskList(_currentTask);
                                }
                                _currentTask = theTask;
                            }
                        }
                        else
                        {
                            _currentTask = theTask;
                        }
                       

                    }
                }
                break;
            case VehicleType.smallTransport:
                if (theTask._taskType == UnitTask.TaskType.Pickup || theTask._taskType == UnitTask.TaskType.RechargeVehicle || theTask._taskType == UnitTask.TaskType.Walk)
                {
                    if (_currentTask != null)
                    {
                        if (_currentTask._taskType != UnitTask.TaskType.RechargeVehicle)
                        {
                            if (_currentTask._taskType == UnitTask.TaskType.Pickup)
                            {
                                TaskList.AddTaskToGlobalTaskList(_currentTask);
                            }
                            _currentTask = theTask;
                        }
                    }
                    else
                    {
                        _currentTask = theTask;
                    }                   
                    
                }
                break;
        }


    }
    public void SetOccupied(bool trueFalse)
    {
        _occupied = trueFalse;

        if (_occupied)
        {
            _driverMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
        else
        {
            _driverMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
    }
    public bool GetOccupied()
    {
        return _occupied;
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
                    return RunFromMonster();
                }
            }
            else
            {
                return RunFromMonster();
            }
        }
        return false;
    }
    /// <summary>
    /// a function to check if a monster is near, if there is change nav mesh agents destination to make the vehicle flee
    /// </summary>
    /// <returns></returns>
    bool RunFromMonster()
    {
        Ray _ray;
        RaycastHit _hit;

        _closestMonster = _monsterList[0];
        foreach (Monster theMonster in _monsterList)
        {
            if (Vector3.Distance(theMonster.transform.position, transform.position) < Vector3.Distance(transform.position, _closestMonster.transform.position))
            {
                _closestMonster = theMonster;
            }
        }

        //ray cast to closest monster
        _ray = new Ray(transform.position, _closestMonster.transform.position - transform.position);
        Debug.DrawRay(transform.position, _ray.direction);

        if (Physics.Raycast(_ray, out _hit))
        {
            if (_hit.transform.tag == "Monster")
            {
                if (_currentTask != null)
                {
                    _taskSetup = false;
                }
                if (Vector3.Distance(_closestMonster.transform.position, transform.position) < 10)
                {
                    _navAgent.SetDestination(transform.position - (_closestMonster.transform.position - transform.position));

                    return true;
                }
                else
                {

                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Saves the drivers information for disembark
    /// </summary>
    /// <param name="name">workers name</param>
    /// <param name="health">workers current health</param>
    /// <param name="tool">workers current tool</param>
    public void Embark(GameObject theDriver)
    {
        _theDriver = theDriver;
        _theDriver.GetComponent<Worker>().CancelCurrentTask();
        _theDriver.GetComponent<Worker>()._inVehicle = true;
        _theDriver.gameObject.SetActive(false);
        SetOccupied(true);

        //_theDriver.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        //_theDriver.GetComponent<Worker>()._healthBars.GetComponent<Canvas>().enabled = false;

        //if (_theDriver.GetComponent<Worker>()._currentToolModel != null)
        //{
        //    MeshRenderer[] tempList = _theDriver.GetComponent<Worker>()._currentToolModel.GetComponentsInChildren<MeshRenderer>();
        //    for (int i = 0; i < tempList.Length; i++)
        //    {
        //        tempList[i].enabled = false;
        //    }

        //    if (_theDriver.GetComponent<Worker>()._currentToolModel.GetComponent<MeshRenderer>() != null)
        //    {
        //        _theDriver.GetComponent<Worker>()._currentToolModel.GetComponent<MeshRenderer>().enabled = false;
        //    }
        //}
        //_theDriver.GetComponent<Worker>().enabled = false;
        //_theDriver.GetComponent<NavMeshAgent>().enabled = false;
        //_theDriver.GetComponent<SelectableObject>().enabled = false;
        //_theDriver.GetComponent<AudioSource>().enabled = false;
        //_theDriver.GetComponent<LineRenderer>().enabled = false;
        //_theDriver.GetComponentInChildren<Light>().enabled = false;



        UnitToolInfoScript.instance.UpdateDisplay();
    }
    /// <summary>
    /// disembarks driver spawning new worke prefab
    /// </summary>
    public void Disembark()
    {
        _navAgent.ResetPath();
        _theDriver.gameObject.SetActive(true);
        _theDriver.transform.position = transform.position;
        _theDriver.GetComponent<Worker>()._inVehicle = false;
        SetOccupied(false);
        //_theDriver.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        //_theDriver.GetComponent<NavMeshAgent>().enabled = true;
        //_theDriver.GetComponent<SelectableObject>().enabled = true;
        //_theDriver.GetComponent<Worker>().enabled = true;
        //if (_theDriver.GetComponent<Worker>()._currentToolModel != null)
        //{
        //    MeshRenderer[] tempList = _theDriver.GetComponent<Worker>()._currentToolModel.GetComponentsInChildren<MeshRenderer>();
        //    for (int i = 0; i < tempList.Length; i++)
        //    {
        //        tempList[i].enabled = true;
        //    }

        //    if (_theDriver.GetComponent<Worker>()._currentToolModel.GetComponent<MeshRenderer>() != null)
        //    {
        //        _theDriver.GetComponent<Worker>()._currentToolModel.GetComponent<MeshRenderer>().enabled = true;
        //    }
        //}
        //_theDriver.GetComponent<Worker>()._healthBars.GetComponent<Canvas>().enabled = true;
        //_theDriver.GetComponent<Worker>()._inVehicle = false;
        //_theDriver.GetComponent<Worker>().SetTool();
        //_theDriver.GetComponent<SelectableObject>().enabled = true;
        //_theDriver.GetComponent<AudioSource>().enabled = true;
        //_theDriver.GetComponent<LineRenderer>().enabled = true;
        //_theDriver.GetComponentInChildren<Light>().enabled = true;


        UnitToolInfoScript.instance.UpdateDisplay();
    }
    public void CheckEnergy()
    {
        

        _targetBuilding = FindClosestBuildingWithTag("GARAGE", "");
        if (_targetBuilding != null)
        {
            int currentSetting = (int)SettingScript.instance.RiskLevel;
            float timeToGen = Vector3.Distance(transform.position, _targetBuilding.transform.position) / _speed;

            if (timeToGen > (Energy - (currentSetting + 10)))
            {
                if (_currentTask != null)
                {
                    if(_currentTask._taskType != UnitTask.TaskType.RechargeVehicle)
                    {
                        UnitTask temptask = new UnitTask
                        {
                            _location = _targetBuilding.transform.position,
                            _taskType = UnitTask.TaskType.RechargeVehicle,
                            _taskDescription = "Recharging Vehicle"
                        };

                        SetTask(temptask);
                    }
                }
                else
                {
                    UnitTask temptask = new UnitTask
                    {
                        _location = _targetBuilding.transform.position,
                        _taskType = UnitTask.TaskType.RechargeVehicle,
                        _taskDescription = "Recharging Vehicle"
                    };

                    SetTask(temptask);
                }
            }
        }

        if (Energy <= 0 && _occupied)
        {
            Disembark();
        }

    }
    /// <summary>
    /// returns closest garage if there is one
    /// </summary>
    /// <returns></returns>
    Building FindClosestBuildingWithTag(string theTag, string theOtherTag)
    {
        List<Building> buildingList = _worldController._buildings;
        List<Building> sortedBList = new List<Building>();

        foreach (Building aBuilding in buildingList)
        {
            if (theOtherTag != "")
            {
                if (aBuilding.tag == theTag || aBuilding.tag == theOtherTag)
                {
                    if (aBuilding.IsPowered)
                    {
                        sortedBList.Add(aBuilding);
                    }
                }
            }
            else
            {
                if (aBuilding.tag == theTag)
                {
                    sortedBList.Add(aBuilding);
                }
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
            return null;
        }
        else
        {
            return _targetBuilding;
        }
    }
    /// <summary>
    /// Returns current energy level
    /// </summary>
    /// <returns></returns>
    public float GetEnergy()
    {
        return _energyLevel;
    }
    /// <summary>
    /// Returns current Vehicle Type
    /// </summary>
    /// <returns></returns>
    public VehicleType GetVehicleType()
    {
        return _vType;
    }
    /// <summary>
    /// Adds a task to the end of the Workers task list
    /// </summary>
    /// <param name="theTask">Task to be Added</param>
    public void AddTask(UnitTask theTask)
    {
        theTask.UpdateTask();
        _localTaskList.Add(theTask);
    }


    new public float Health
    {
        get { return _health; }
        set
        {
            _health = value;
            _barsUpdater.UpdateHealthFill(value);
        }
    }


    public float Energy
    {
        get { return _energyLevel; }
        set
        {
            _energyLevel = value;
            _barsUpdater.UpdateEnergyFill(value);
        }
    }
    void PlayOrderSound()
    {
        _vehicleSound.orderSounds();
    }
    public void PlaySelectionSound()
    {
        _vehicleSound.selectionSound();
    }
}