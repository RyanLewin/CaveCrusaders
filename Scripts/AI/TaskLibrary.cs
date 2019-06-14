using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLibrary : MonoBehaviour
{
    public static TaskLibrary Get()
    {
        return WorldController.GetWorldController.TaskLib;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool CanMineTarget(RockScript Target)
    {
        if (Target == null)
        {
            return false;
        }
        int Level = (int)WorldController.GetWorldController._miningLevel;
        switch (Target.RockType)
        {
            case RockScript.Type.Dirt:
                return true;
            case RockScript.Type.LooseRock:
                return (Level >= 1);
            case RockScript.Type.HardRock:
                return (Level >= 2);
        }
        return false;
    }
    public bool CanAttack()
    {
        return (int)WorldController.GetWorldController._miningLevel >= 1;
    }

    public UnitTask CreateTask(UnitTask.TaskType t, Vector3 pos, GameObject Data)
    {
        UnitTask newTask = null;
        switch (t)
        {
            case UnitTask.TaskType.Mine:
                RockScript Rock = Data.GetComponentInParent<RockScript>();
                if (CanMineTarget(Rock))
                {
                    newTask = new UnitTask
                    {
                        _location = pos,
                        _taskType = UnitTask.TaskType.Mine,
                        _targetRock = Rock,
                        _requiredTool = Unit.UnitTool.MiningTool,
                        _taskDescription = "Mining " + Rock.RockType
                    };
                }
                break;
            case UnitTask.TaskType.Pickup:
                Ore O = Data.GetComponent<Ore>();
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.Pickup,
                    _itemToPickUp = Data,
                    _itemType = (O != null) ? UnitTask.ItemType.Ore : UnitTask.ItemType.EnergyCrystal,
                    _taskDescription = (O != null) ? "Transporting an Ore" : "Transporting an Energy crystal",
                    _requiredTool = Unit.UnitTool.none
                };
                break;
            case UnitTask.TaskType.Walk:
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.Walk,
                    _taskDescription = "Walking to location",
                    _requiredTool = Unit.UnitTool.none
                };
                break;
            case UnitTask.TaskType.Attack:
                Monster tempMonster = Data.GetComponent<Monster>();
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.Attack,
                    _requiredTool = Unit.UnitTool.Weapon,
                    _taskDescription = "Attacking " + tempMonster._type,
                    _targetMonster = Data.GetComponent<Monster>()
                };
                break;
            case UnitTask.TaskType.flameTarget:
                MushroomCluster mushroom = Data.GetComponentInParent<MushroomCluster>();
                if (mushroom == null)
                {
                    return null;
                }
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.flameTarget,
                    _requiredTool = Unit.UnitTool.FlameThrower,
                    _taskDescription = "Burning things",
                    _targetMushroom = mushroom
                };
                break;
            case UnitTask.TaskType.Reinforce:
                break;
            case UnitTask.TaskType.Build:
                Building tempBuilding = Data.GetComponentInParent<Building>();
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.Build,
                    _targetBuilding = Data.GetComponentInParent<Building>(),
                    _requiredTool = Unit.UnitTool.Hammer,
                    _taskDescription = "Building a " + tempBuilding.name
                };
                break;
            case UnitTask.TaskType.GetTool:
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.GetTool,
                    _taskDescription = "Getting " + RadialMenuScript.instance.NewTool.ToString(),
                    _requiredTool = RadialMenuScript.instance.NewTool
                };
                break;
            case UnitTask.TaskType.RefillOxygen:
                Building closestOxyGen = null;
                foreach (Building building in WorldController.GetWorldController._buildings)
                {
                    if (building.tag == TagLibrary.GEN_TAG && building._Built)
                    {
                        if (closestOxyGen == null || Vector3.Distance(Data.transform.position, building.transform.position) < Vector3.Distance(Data.transform.position, closestOxyGen.transform.position))
                        {
                            closestOxyGen = building;
                        }
                    }
                }
                if (closestOxyGen == null)
                {
                    return null;
                }
                newTask = new UnitTask
                {
                    _location = closestOxyGen.transform.position,
                    _taskType = UnitTask.TaskType.RefillOxygen,
                    _taskDescription = "Refilling Oxygen",
                    _targetBuilding = closestOxyGen
                };
                break;
            case UnitTask.TaskType.RefillEnergy:
                Building tempBuidling = Data.GetComponent<Building>();
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.RefillEnergy,
                    _targetBuilding = tempBuidling,
                    _taskDescription = "Refilling " + tempBuidling.name + " energy"
                };
                break;
            case UnitTask.TaskType.ClearRubble:
                RubbleScript tempRubble = Data.GetComponentInParent<RubbleScript>();
                if (tempRubble == null)
                {
                    return null;
                }
                newTask = new UnitTask
                {
                    _location = tempRubble.transform.position,
                    _taskType = UnitTask.TaskType.ClearRubble,
                    _targetRubble = tempRubble,
                    _requiredTool = Unit.UnitTool.Shovel,
                    _taskDescription = "Clearing rubble"
                };
                break;
            case UnitTask.TaskType.GetInVehicle:
                Vehicle tempVehicle = Data.GetComponent<Vehicle>();
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.GetInVehicle,
                    _targetVehicle = tempVehicle,
                    _taskDescription = "Entering vehicle"
                };
                break;
            case UnitTask.TaskType.RechargeVehicle:
                newTask = new UnitTask
                {
                    _location = pos,
                    _taskType = UnitTask.TaskType.RechargeVehicle,
                    _taskDescription = "Recharging vehicle"
                };
                break;
            case UnitTask.TaskType.Idle:
            case UnitTask.TaskType.none:
            default:
                Debug.LogError("Invalid Task for Create Task: " + t.ToString());
                break;
        }
        if (newTask == null || !newTask.IsValid())
        {
            return null;
        }
        return newTask;
    }
    public static bool CanWorkerExecuteTask(UnitTask.TaskType t, Worker w)
    {
        switch (t)
        {
            case UnitTask.TaskType.ClearRubble:
                return w._currentTool == Unit.UnitTool.Shovel;
            case UnitTask.TaskType.Mine:
                return w._currentTool == Unit.UnitTool.MiningTool;
            case UnitTask.TaskType.Attack:
                return w._currentTool == Unit.UnitTool.Weapon;
            case UnitTask.TaskType.Reinforce:
            case UnitTask.TaskType.Build:
                return w._currentTool == Unit.UnitTool.Hammer;
            case UnitTask.TaskType.flameTarget:
                return w._currentTool == Unit.UnitTool.FlameThrower;
        }
        return true;
    }
    public static bool CanVehicleExecuteTask(UnitTask.TaskType t, Vehicle w)
    {
        switch (t)
        {
            case UnitTask.TaskType.Walk:
            case UnitTask.TaskType.Mine:
                return true;
        }
        return false;
    }
    public void UpdateOrderMarker(GameObject G, UnitTask task)
    {
        if (task != null)
        {
            if (OrderVisualizer.ConvertTaskToOrderType(task._taskType) == OrderVisualizer.OrderVisualType.None)
            {
                //this task has no icon.
                return;
            }
        }
        OrderVisualizer OV = G.GetComponentInParent<OrderVisualizer>();
        if (OV != null)
        {
            OV.UpdateVisuals(task);
        }
    }
}
