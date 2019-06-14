using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceOptionsMenu : MonoBehaviour
{
    EnergyCrystal _energyCrystal;
    Ore _ore;

    [SerializeField]
    Button _pickUpButton;
    [SerializeField]
    Text _resourceText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open resource radial dial for an energy crystal, store resource
    /// </summary>
    /// <param name="crystal">Energy crystal selected</param>
    public void OpenEnergyCrystalMenu(EnergyCrystal energyCrystal)
    {
        _energyCrystal = energyCrystal;
        _ore = null;
        _resourceText.text = "Energy Crystal";
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Open resource radial dial for an ore, store resource
    /// </summary>
    /// <param name="ore">Ore selected</param>
    public void OpenOreMenu(Ore ore)
    {
        _energyCrystal = null;
        _ore = ore;
        _resourceText.text = "Ore";
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_ore == null && _energyCrystal == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = (_ore == null)? Camera.main.WorldToScreenPoint(_energyCrystal.transform.position) : Camera.main.WorldToScreenPoint(_ore.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_pickUpButton, !PickUpResourceTaskExists());
        //_pickUpButton.interactable = !PickUpResourceTaskExists();
    }

    /// <summary>
    /// When selecting to pick up a resource, create pick up task depending on resource type and add to task list
    /// </summary>
    public void PickUpResource()
    {
        if (_ore != null)
        {
            TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.Pickup, _ore.transform.position, _ore.gameObject));

            //UnitTask tempTask = new UnitTask
            //{
            //    _location = _ore.transform.position,
            //    _taskType = UnitTask.TaskType.Pickup,
            //    _taskDescription = "Transporting an Ore",
            //    _itemToPickUp = _ore.transform.gameObject,
            //    _itemType = UnitTask.ItemType.Ore,
            //    _requiredTool = Unit.UnitTool.none
            //};
            //AddTaskToGlobalTaskList(tempTask);
        }
        else if (_energyCrystal != null)
        {
            TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.Pickup, _energyCrystal.transform.position, _energyCrystal.gameObject));

            //UnitTask tempTask = new UnitTask
            //{
            //    _location = _energyCrystal.transform.position,
            //    _taskType = UnitTask.TaskType.Pickup,
            //    _taskDescription = "Transporting an Ore",
            //    _itemToPickUp = _energyCrystal.transform.gameObject,
            //    _itemType = UnitTask.ItemType.EnergyCrystal,
            //    _requiredTool = Unit.UnitTool.none
            //};
            //AddTaskToGlobalTaskList(tempTask);
        }
    }

    /// <summary>
    /// Check if an exisitng pick up resource task exists for this resource
    /// </summary>
    /// <returns>Bool if task exists</returns>
    bool PickUpResourceTaskExists()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                UnitTask workerTask = worker.GetCurrentTask();
                if (workerTask != null && (workerTask._taskType == UnitTask.TaskType.Pickup && workerTask._itemToPickUp == (_ore != null ? _ore.gameObject : _energyCrystal.gameObject)))
                {
                    return true;
                }
            }
        }

        return TaskList.Tasks.Find(task => task._taskType == UnitTask.TaskType.Pickup && task._itemToPickUp == (_ore != null ? _ore.gameObject : _energyCrystal.gameObject)) != null;
    }
}