using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyVehicleOptionsMenu : MonoBehaviour
{
    Worker _worker;
    Vehicle _vehicle;

    [SerializeField]
    Button _enterButton;
    [SerializeField]
    Text _vehicleText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open vehicle radial dial, store vehicle
    /// </summary>
    /// <param name="worker">Worker selected</param>
    /// <param name="vehicle">Vehicle selected</param>
    public void Open(Worker worker, Vehicle vehicle)
    {
        _worker = worker;
        _vehicle = vehicle;
        _vehicleText.text = vehicle.GetVehicleType().ToString();
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_vehicle == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_vehicle.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_enterButton, (!_vehicle.GetOccupied() && !EnterVehicleTaskExists()));
        //_enterButton.interactable = (!_vehicle.GetOccupied() && !EnterVehicleTaskExists());
    }

    /// <summary>
    /// When selecting to enter a vehicle, create and set get in vehicle task
    /// </summary>
    public void EnterVehicle()
    {
        UnitTask tempTask = TaskLibrary.Get().CreateTask(UnitTask.TaskType.GetInVehicle, _vehicle.transform.position, _vehicle.gameObject);
        if (_worker != null)
        {
            _worker.SetTask(tempTask);
        }
        else
        {
            TaskList.AddTaskToGlobalTaskList(tempTask);
        }
    }

    /// <summary>
    /// Check if an exisitng enter vehicle task exists for this vehicle
    /// </summary>
    /// <returns>Bool if task exists</returns>
    bool EnterVehicleTaskExists()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                UnitTask workerTask = worker.GetCurrentTask();
                if (workerTask != null && (workerTask._taskType == UnitTask.TaskType.GetInVehicle && workerTask._targetVehicle == _vehicle))
                {
                    return true;
                }
            }
        }

        return (TaskList.Tasks.Find(task => task._targetVehicle == _vehicle && task._taskType == UnitTask.TaskType.GetInVehicle) != null);
    }
}
