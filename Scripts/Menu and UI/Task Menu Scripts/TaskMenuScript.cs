using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskMenuScript : MonoBehaviour
{
    public static TaskMenuScript instance;

    bool _reorderListEnabled;
    //int _checkTasksCounter;

    [SerializeField]
    GameObject _tasksContentView, _taskContentItemPrefab;
    [SerializeField]
    Text _reorderTasksButtonText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        UpdateTaskList();
        _reorderListEnabled = false;
        //_checkTasksCounter = 0;
    }

    private void Update()
    {
        if (TaskList.Tasks.Count != _tasksContentView.transform.GetComponentsInChildren<TaskMenuItemScript>().Length)
        {
            UpdateTaskList();
        }

        //if (_checkTasksCounter > 10)
        //{
        //    ValidateAllTasks();
        //    _checkTasksCounter = 0;
        //}
        //_checkTasksCounter++;
    }

    /// <summary>
    /// Delete all items from the UI task list display and repopulate it from the task list
    /// </summary>
    public void UpdateTaskList()
    {
        if (_tasksContentView.activeInHierarchy && !_reorderListEnabled)
        {
            if (_tasksContentView.transform.childCount > 0)
            {
                for (int i = 0; i < _tasksContentView.transform.childCount; i++)
                {
                    Destroy(_tasksContentView.transform.GetChild(i).gameObject);
                }
            }

            for (int i = 0; i < TaskList.Tasks.Count; i++)
            {
                GameObject taskListItem = Instantiate(_taskContentItemPrefab, _tasksContentView.transform);
                taskListItem.GetComponentInChildren<Text>().text = TaskList.Tasks[i]._taskDescription;
                taskListItem.GetComponent<TaskMenuItemScript>()._thisTask = TaskList.Tasks[i];
            }
        }
    }

    /// <summary>
    /// Allow task list reordering
    /// </summary>
    public void AllowTaskReordering()
    {
        ReorderListEnabled = !ReorderListEnabled;
        _reorderTasksButtonText.text = (ReorderListEnabled == true) ? "✓" : "↑↓";
    }

    public bool ReorderListEnabled
    {
        get => _reorderListEnabled;
        set
        {
            _reorderListEnabled = value;
            if (value == false)
            {
                ReorderTaskList();
            }
        }
    }

    /// <summary>
    /// Reorder the task list using the UI task list sibling indexes
    /// </summary>
    public void ReorderTaskList()
    {
        List<UnitTask> tempList = new List<UnitTask>();

        for (int i = 0; i < _tasksContentView.transform.childCount; i++)
        {
            tempList.Add(_tasksContentView.transform.GetChild(i).GetComponent<TaskMenuItemScript>()._thisTask);
        }


        for (int i = 0; i < TaskList.Tasks.Count; i++)
        {
            if (!tempList.Contains(TaskList.Tasks[i]))
            {
                tempList.Add(TaskList.Tasks[i]);
            }
        }

        TaskList.Tasks.Clear();
        TaskList.Tasks = tempList;
    }

    ///// <summary>
    ///// Go through all worker tasks and task list tasks and check they are still valid and not duplicates, remove any that aren't
    ///// </summary>
    //public void ValidateAllTasks()
    //{
    //    foreach (Worker worker in WorldController.GetWorldController._workers)
    //    {
    //        UnitTask workerTask = worker.GetCurrentTask();
    //        if (workerTask != null)
    //        {
    //            if (!workerTask.IsValid())
    //            {
    //                worker.CancelCurrentTask();
    //            }
    //            else
    //            {
    //                foreach (Worker worker2 in WorldController.GetWorldController._workers)
    //                {
    //                    if (worker2 != worker && (worker2.GetCurrentTask() != null))
    //                    {
    //                        if (AreTasksSame(workerTask, worker2.GetCurrentTask()))
    //                        {
    //                            worker.CancelCurrentTask();
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    foreach (UnitTask task in TaskList.Tasks)
    //    {
    //        if (!task.IsValid())
    //        {
    //            TaskList.Tasks.Remove(task);
    //        }
    //        else
    //        {
    //            foreach (Worker worker in WorldController.GetWorldController._workers)
    //            {
    //                if (worker.GetCurrentTask() != null)
    //                {
    //                    if (AreTasksSame(task, worker.GetCurrentTask()))
    //                    {
    //                        TaskList.Tasks.Remove(task);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}


    public static bool AreTasksSame(UnitTask task1, UnitTask task2)
    {
        if (task1 == null || task2 == null)
        {
            return false;
        }
        if (task1._taskType == task2._taskType)
        {
            switch (task1._taskType)
            {
                case UnitTask.TaskType.Mine:
                    return (task1._targetRock == task2._targetRock);

                case UnitTask.TaskType.Build:
                case UnitTask.TaskType.RefillEnergy:
                    return ((task1._targetBuilding == task2._targetBuilding) && (task1._location == task2._location));

                case UnitTask.TaskType.Pickup:
                    return (task1._itemToPickUp == task2._itemToPickUp);

                case UnitTask.TaskType.ClearRubble:
                    return ((task1._targetRubble == task2._targetRubble) && (task1._location == task2._location));

                case UnitTask.TaskType.GetInVehicle:
                    return (task1._targetVehicle == task2._targetVehicle);
            }
        }
        return false;
    }
}