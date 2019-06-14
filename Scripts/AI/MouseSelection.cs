using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseSelection : MonoBehaviour
{
    [SerializeField] GameObject _selectionRing;
    Vector3 _mouseWorldLocation;
    SelectableObject _currentSelectedObject;
    RaycastHit _hit;
    Ray _ray;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_currentSelectedObject != null)
        {
            _selectionRing.transform.position = _currentSelectedObject.transform.position;
        }
        else
        {
            _selectionRing.transform.position = new Vector3(0, -100, 0);
        }

        if (Physics.Raycast(_ray, out _hit))
        {
            Debug.DrawRay(_ray.origin, _ray.direction * _hit.distance, Color.yellow);
            _mouseWorldLocation = _hit.point;

            if (ControlScript.instance.GetControl("Select").InputDown)
            {

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    RadialMenuScript.instance.CloseMenu();
                }

                switch (_hit.transform.tag)
                {
                    case TagLibrary.WORKER_TAG:
                        if (_currentSelectedObject != null)
                        {
                            _currentSelectedObject._selected = false;
                            _currentSelectedObject = null;
                        }

                        _currentSelectedObject = _hit.transform.gameObject.GetComponent<SelectableObject>();
                        _currentSelectedObject._selected = true;

                        WorkerInfoPanelScript.instance.Open(_hit.transform.GetComponent<Worker>());
                        break;

                    default:

                        if (_currentSelectedObject != null)
                        {
                            _currentSelectedObject._selected = false;
                            _currentSelectedObject = null;
                        }
                        WorkerInfoPanelScript.instance.Close();
                        break;
                }

            }
            else if (ControlScript.instance.GetControl("Order Modifier").AnyInput && ControlScript.instance.GetControl("Order").InputDown)
            {
                if (_currentSelectedObject != null)
                {
                    switch (_hit.transform.tag)
                    {
                        case "Floor":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _mouseWorldLocation,
                                    _taskType = UnitTask.TaskType.Walk,
                                    _taskDescription = "Walking to location",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.AddTask(tempTask);
                            }

                            break;
                        case "RockTile":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.gameObject.GetComponent<Worker>();
                                RockScript tempRock = _hit.transform.gameObject.GetComponentInParent<RockScript>();

                                if (tempWorker._currentTool == Unit.UnitTool.Hammer)
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = _hit.transform.position,
                                        _taskType = UnitTask.TaskType.Reinforce,
                                        _targetRock = tempRock,
                                        _requiredTool = Unit.UnitTool.Hammer,
                                        _taskDescription = "Reinforcing a wall"
                                    };
                                    tempWorker.AddTask(tempTask);
                                }
                                else
                                {
                                    if (tempRock.RockType == RockScript.Type.Dirt || (tempRock.RockType == RockScript.Type.LooseRock && (int)WorldController.GetWorldController._miningLevel >= 1) || (tempRock.RockType == RockScript.Type.HardRock && (int)WorldController.GetWorldController._miningLevel == 2))
                                    {
                                        UnitTask tempTask = new UnitTask
                                        {
                                            _location = _hit.transform.position,
                                            _taskType = UnitTask.TaskType.Mine,
                                            _targetRock = tempRock,
                                            _requiredTool = Unit.UnitTool.MiningTool,
                                            _taskDescription = "Mining a wall"
                                        };
                                        tempWorker.SetTask(tempTask);
                                    }
                                }
                            }
                            break;
                        case "Rubble":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.gameObject.GetComponent<Worker>();
                                RubbleScript tempRubble = _hit.transform.gameObject.GetComponentInParent<RubbleScript>();

                                UnitTask tempTask = new UnitTask
                                {
                                    _location = tempRubble.transform.position,
                                    _taskType = UnitTask.TaskType.ClearRubble,
                                    _targetRubble = tempRubble,
                                    _requiredTool = Unit.UnitTool.Shovel,
                                    _taskDescription = "Clearing rubble"
                                };
                                tempWorker.SetTask(tempTask);
                            }
                            break;
                        case "Ore":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Pickup,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _itemType = UnitTask.ItemType.Ore,
                                    _taskDescription = "Transporting Ore",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;
                        case "EnergyCrystal":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Pickup,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _itemType = UnitTask.ItemType.EnergyCrystal,
                                    _taskDescription = "Transporting an Energy crystal",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;
                        case "HQ":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                //RadialMenuScript.instance.ShowToolOptions(_currentSelectedObject.transform.gameObject.GetComponent<Worker>(), _hit.transform.position);
                            }
                            break;
                        case "SKIP":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                //check if building needs building

                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Build,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _requiredTool = Unit.UnitTool.Hammer,
                                    _taskDescription = "Building A Skip"
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;
                        case "GEN":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                //check if building needs building

                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Build,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _requiredTool = Unit.UnitTool.Hammer,
                                    _taskDescription = "Building An Oxegen Generator"
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;
                        case "Monster":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                if ((int)WorldController.GetWorldController._miningLevel >= 1)
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = _hit.transform.position,
                                        _taskType = UnitTask.TaskType.Attack,
                                        _requiredTool = Unit.UnitTool.Weapon,
                                        _taskDescription = "Attacking monster"
                                    };
                                    tempWorker.SetTask(tempTask);
                                }
                            }
                            break;

                    }
                }
            }
            else if (ControlScript.instance.GetControl("Order").InputDown)
            {
                if (_currentSelectedObject == null || (_currentSelectedObject != null && _currentSelectedObject == _hit.transform.GetComponentInParent<SelectableObject>()))
                {
                    switch (_hit.transform.tag)
                    {
                        case "Worker":
                            Worker worker = _hit.transform.GetComponent<Worker>();
                            //RadialMenuScript.instance.ShowWorkerOptions(worker);
                            break;

                        case "RockTile":
                            RockScript rock = _hit.transform.gameObject.GetComponentInParent<RockScript>();
                            RadialMenuScript.instance.ShowRockOptions(rock);
                            break;

                        case "Rubble":
                            RubbleScript rubble = _hit.transform.gameObject.GetComponentInParent<RubbleScript>();
                            RadialMenuScript.instance.ShowRubbleOptions(rubble);
                            break;

                        case "Monster":
                            Monster monster = _hit.transform.GetComponentInParent<Monster>();
                            RadialMenuScript.instance.ShowEnemyOptions(monster);
                            break;

                        case "HQ":
                            HQ hq = _hit.transform.GetComponentInParent<HQ>();
                            RadialMenuScript.instance.ShowHQOptions(hq);
                            break;

                        case "GEN":
                        case "BLOCK":
                            Building building = _hit.transform.GetComponentInParent<Building>();
                            RadialMenuScript.instance.ShowBuildingOptions(building);
                            break;

                        case "Ore":
                            Ore ore = _hit.transform.GetComponentInParent<Ore>();
                            RadialMenuScript.instance.ShowResourceOptionsOre(ore);
                            break;

                        case "EnergyCrystal":
                            EnergyCrystal energyCrystal = _hit.transform.GetComponentInParent<EnergyCrystal>();
                            RadialMenuScript.instance.ShowResourceOptionsEnergyCrystal(energyCrystal);
                            break;
                    }
                }
                else if (_currentSelectedObject != null)
                {
                    switch (_hit.transform.tag)
                    {
                        case "Floor":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _mouseWorldLocation,
                                    _taskType = UnitTask.TaskType.Walk,
                                    _taskDescription = "Walking to location",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.SetTask(tempTask);
                            }

                            break;
                        case "RockTile":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.gameObject.GetComponent<Worker>();
                                RockScript tempRock = _hit.transform.gameObject.GetComponentInParent<RockScript>();

                                if (tempWorker._currentTool == Unit.UnitTool.Hammer)
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = _hit.transform.position,
                                        _taskType = UnitTask.TaskType.Reinforce,
                                        _targetRock = tempRock,
                                        _requiredTool = Unit.UnitTool.Hammer,
                                        _taskDescription = "Reinforcing a wall"
                                    };
                                    tempWorker.SetTask(tempTask);
                                }
                                else
                                {
                                    if (tempRock.RockType == RockScript.Type.Dirt || (tempRock.RockType == RockScript.Type.LooseRock && (int)WorldController.GetWorldController._miningLevel >= 1) || (tempRock.RockType == RockScript.Type.HardRock && (int)WorldController.GetWorldController._miningLevel == 2))
                                    {
                                        UnitTask tempTask = new UnitTask
                                        {
                                            _location = _hit.transform.position,
                                            _taskType = UnitTask.TaskType.Mine,
                                            _targetRock = tempRock,
                                            _requiredTool = Unit.UnitTool.MiningTool,
                                            _taskDescription = "Mining a wall"
                                        };
                                        tempWorker.SetTask(tempTask);
                                    }
                                }
                            }
                            break;
                        case "Rubble":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.gameObject.GetComponent<Worker>();
                                RubbleScript tempRubble = _hit.transform.gameObject.GetComponentInParent<RubbleScript>();

                                if (tempWorker._currentTool == Unit.UnitTool.Shovel)
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = tempRubble.transform.position,
                                        _taskType = UnitTask.TaskType.ClearRubble,
                                        _targetRubble = tempRubble,
                                        _requiredTool = Unit.UnitTool.Shovel,
                                        _taskDescription = "Clearing rubble"
                                    };
                                    tempWorker.SetTask(tempTask);
                                }
                                else
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = _mouseWorldLocation,
                                        _taskType = UnitTask.TaskType.Walk,
                                        _taskDescription = "Walking to location",
                                        _requiredTool = Unit.UnitTool.none
                                    };
                                    tempWorker.SetTask(tempTask);
                                }
                            }
                            break;
                        case "Ore":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Pickup,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _itemType = UnitTask.ItemType.Ore,
                                    _taskDescription = "Transporting Ore",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.SetTask(tempTask);
                            }
                            break;
                        case "EnergyCrystal":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();
                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Pickup,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _itemType = UnitTask.ItemType.EnergyCrystal,
                                    _taskDescription = "Transporting an Energy crystal",
                                    _requiredTool = Unit.UnitTool.none
                                };
                                tempWorker.SetTask(tempTask);
                            }
                            break;
                        case "HQ":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                //RadialMenuScript.instance.ShowToolOptions(_currentSelectedObject.transform.gameObject.GetComponent<Worker>(), _hit.transform.position);
                            }
                            break;
                        case "SKIP":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                //check if building needs building

                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Build,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _requiredTool = Unit.UnitTool.Hammer,
                                    _taskDescription = "Building A Skip"
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;
                        case "GEN":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                //check if building needs building

                                UnitTask tempTask = new UnitTask
                                {
                                    _location = _hit.transform.position,
                                    _taskType = UnitTask.TaskType.Build,
                                    _itemToPickUp = _hit.transform.gameObject,
                                    _requiredTool = Unit.UnitTool.Hammer,
                                    _taskDescription = "Building An Oxegen Generator"
                                };
                                tempWorker.AddTask(tempTask);
                            }
                            break;

                        case "Monster":
                            if (_currentSelectedObject.transform.tag == "Worker")
                            {
                                Worker tempWorker = _currentSelectedObject.transform.gameObject.GetComponent<Worker>();

                                if ((int)WorldController.GetWorldController._miningLevel >= 1)
                                {
                                    UnitTask tempTask = new UnitTask
                                    {
                                        _location = _hit.transform.position,
                                        _taskType = UnitTask.TaskType.Attack,
                                        _requiredTool = Unit.UnitTool.Weapon,
                                        _taskDescription = "Attacking monster"
                                    };
                                    tempWorker.SetTask(tempTask);
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
    public Vector3 GetMousePosition()
    {
        return _mouseWorldLocation;
    }
}
