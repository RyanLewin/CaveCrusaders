using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MushroomOptionsMenu : MonoBehaviour
{
    MushroomCluster _mushroomCluster;

    [SerializeField]
    Button _burnButton;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open mushroom radial dial, store mushroom cluster
    /// </summary>
    /// <param name="mushroomCluster">Mushroom selected</param>
    public void Open(MushroomCluster mushroomCluster)
    {
        _mushroomCluster = mushroomCluster;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_mushroomCluster == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_mushroomCluster.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_burnButton, !BurnClusterTaskExists());
    }

    /// <summary>
    /// When selecting to dig rubble, create digging task and add to task list
    /// </summary>
    public void BurnCluster()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.flameTarget, _mushroomCluster.transform.position, _mushroomCluster.gameObject));
    }

    /// <summary>
    /// Check if an exisitng burn cluster task exists for this mushroom
    /// </summary>
    /// <returns>Bool if task exists</returns>
    bool BurnClusterTaskExists()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                UnitTask workerTask = worker.GetCurrentTask();
                if (workerTask != null && (workerTask._taskType == UnitTask.TaskType.flameTarget && workerTask._targetMushroom == _mushroomCluster))
                {
                    return true;
                }
            }
        }

        return (TaskList.Tasks.Find(task => task._targetMushroom == _mushroomCluster && task._taskType == UnitTask.TaskType.flameTarget) != null);
    }
}
