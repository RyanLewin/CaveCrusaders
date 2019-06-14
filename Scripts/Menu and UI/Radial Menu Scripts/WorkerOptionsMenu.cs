using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerOptionsMenu : MonoBehaviour
{
    List<Worker> _workers;
    Worker _hitWorker;

    [SerializeField]
    Button _cancelTaskButton, _refillButton;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open worker radial dial, store worker
    /// </summary>
    /// <param name="workers">All selected workers</param>
    /// <param name="hitWorker">Worker hit in mouse selection</param>
    public void Open(List<Worker> workers, Worker hitWorker)
    {
        _workers = workers;
        _hitWorker = hitWorker;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        for (int i = 0; i < _workers.Count; i++)
        {
            if (_workers[i] == null || (_workers[i] != null && _workers[i].Dead))
            {
                RadialMenuScript.instance.CloseMenu();
                return;
            }
        }

        transform.position = Camera.main.WorldToScreenPoint(_hitWorker.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_cancelTaskButton, !(_hitWorker.GetCurrentTask() == null || _hitWorker.GetCurrentTask()._taskType == UnitTask.TaskType.Idle));
        RadialMenuScript.instance.SetButtonInteractable(_refillButton, (WorldController.GetWorldController._buildings.Find(building => building.buildingType == Building.BuildingType.OxyGen && building._Built && building.IsPowered)));
        //_cancelTaskButton.interactable = !(_hitWorker.GetCurrentTask() == null || _hitWorker.GetCurrentTask()._taskType == UnitTask.TaskType.Idle);
        //_refillButton.interactable = (WorldController.GetWorldController._buildings.Find(building => building.buildingType == Building.BuildingType.OxyGen && building._Built && building.IsPowered));
    }

    /// <summary>
    /// When selecting to change a tool, find the nearest HQ and show the tool options menu
    /// </summary>
    public void ChangeTool()
    {
        //Building closestHQ = null;

        //foreach (Building building in WorldController.GetWorldController._buildings)
        //{
        //    if (building.tag == "HQ" && building._Built)
        //    {
        //        if (closestHQ == null || Vector3.Distance(_worker.transform.position, building.transform.position) < Vector3.Distance(_worker.transform.position, closestHQ.transform.position))
        //        {
        //            closestHQ = building;
        //        }
        //    }
        //}
        RadialMenuScript.instance.ShowToolOptions(_workers, _hitWorker, WorldController.GetWorldController._HQ.transform.position);
    }

    /// <summary>
    /// When selecting to refill oxygen, find the nearest oxygen generator and create refill task
    /// </summary>
    public void RefillO2()
    {
        for (int i = 0; i < _workers.Count; i++)
        {
            _workers[i].SetTask(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RefillOxygen, Vector3.zero, _workers[i].gameObject), (i < 4));
        }
    }

    /// <summary>
    /// When selecting to cancel current task, cancel the worker's current task
    /// </summary>
    public void CancelTask()
    {
        for (int i = 0; i < _workers.Count; i++)
        {
            _workers[i].CancelCurrentTask();
        }
    }
}
