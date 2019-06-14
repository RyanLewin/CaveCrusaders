using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RubbleOptionsMenu : MonoBehaviour
{
    RubbleScript _rubble;

    [SerializeField]
    Button _digButton;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open rubble radial dial, store rubble
    /// </summary>
    /// <param name="rubble">Rubble selected</param>
    public void Open(RubbleScript rubble)
    {
        _rubble = rubble;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_rubble == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_rubble.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_digButton, !ClearRubbleTaskExists());
        //_digButton.interactable = !ClearRubbleTaskExists();
    }

    /// <summary>
    /// When selecting to dig rubble, create digging task and add to task list
    /// </summary>
    public void DigRubble()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.ClearRubble, _rubble.transform.position, _rubble.gameObject));

        //UnitTask tempTask = new UnitTask
        //{
        //    _location = _rubble.transform.position,
        //    _taskType = UnitTask.TaskType.ClearRubble,
        //    _targetRubble = _rubble,
        //    _requiredTool = Unit.UnitTool.Shovel,
        //    _taskDescription = "Clearing rubble"
        //};
        //AddTaskToGlobalTaskList(tempTask);
    }

    /// <summary>
    /// Check if an exisitng clear rubble task exists for this rubble
    /// </summary>
    /// <returns>Bool if task exists</returns>
    bool ClearRubbleTaskExists()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                UnitTask workerTask = worker.GetCurrentTask();
                if (workerTask != null && (workerTask._taskType == UnitTask.TaskType.ClearRubble && workerTask._targetRubble == _rubble))
                {
                    return true;
                }
            }
        }

        return (TaskList.Tasks.Find(task => task._targetRubble == _rubble && task._taskType == UnitTask.TaskType.ClearRubble) != null);
    }
}
