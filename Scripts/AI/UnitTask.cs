using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTask
{
    // Start is called before the first frame update

    public enum TaskType
    {
        none,
        Mine,
        Pickup,
        Walk,
        Attack,
        Reinforce,
        Build,
        GetTool,
        RefillOxygen,
        RefillEnergy,
        ClearRubble,
        GetInVehicle,
        Idle,
        RechargeVehicle,
        flameTarget
    }
    public enum ItemType
    {
        EnergyCrystal,
        Ore
    }

    public Vector3 _location;
    public TaskType _taskType;
    public RockScript _targetRock;
    public Unit.UnitTool _requiredTool;
    public GameObject _itemToPickUp;
    public ItemType _itemType;
    public string _taskDescription;
    public Monster _targetMonster;
    public Building _targetBuilding;
    public RubbleScript _targetRubble;
    public Vehicle _targetVehicle;
    public MushroomCluster _targetMushroom;

    public bool IsValid()
    {
        switch (_taskType)
        {
            case TaskType.Mine:
                return (_location != null && _targetRock != null);

            case TaskType.Attack:
                return (_targetMonster != null);

            case TaskType.RefillOxygen:
            case TaskType.Walk:
            case TaskType.GetTool:
                return (_location != null);

            case TaskType.Build:
            case TaskType.RefillEnergy:
                return (_targetBuilding != null && _location != null);

            case TaskType.Pickup:
                return (_itemToPickUp != null);

            case TaskType.ClearRubble:
                return (_location != null && _targetRubble != null);
            case TaskType.GetInVehicle:
                return (_targetVehicle != null && !_targetVehicle.GetOccupied());
            
            case TaskType.flameTarget:
                return (_location != null && _targetMushroom != null);
            case TaskType.none:
                return false;
            default:
                return true;
        }
    }

    public void Invalidate()
    {
        _taskType = TaskType.none;
    }

    public GameObject GetTargetObject()
    {
        if (_targetRock != null)
        {
            return _targetRock.gameObject;
        }
        if (_targetRubble != null)
        {
            return _targetRubble.gameObject;
        }
        if (_targetBuilding != null)
        {
            return _targetBuilding.gameObject;
        }
        return null;
    }

    public void UpdateTask()
    {
        if (GetTargetObject())
        {
            TaskLibrary.Get().UpdateOrderMarker(GetTargetObject(), this);
            Tile t = GetTargetObject().GetComponent<Tile>();
            if (t != null)//tell tiles about task that target them for easy canceling!
            {
                t.NotifyAboutTask(this);
            }
        }
    }

    public void DestroyTask()
    {
        if (GetTargetObject() != null)
        {
            TaskLibrary.Get().UpdateOrderMarker(GetTargetObject(), null);
        }
    }

    ~UnitTask()
    {}
}
