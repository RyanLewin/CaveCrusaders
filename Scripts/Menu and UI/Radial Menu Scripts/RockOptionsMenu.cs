using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockOptionsMenu : MonoBehaviour
{
    RockScript _rock;

    [SerializeField]
    Button _mineButton;
    [SerializeField]
    Text _rockText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open rock radial dial, store rock
    /// </summary>
    /// <param name="rock">Rock selected</param>
    public void Open(RockScript rock)
    {
        _rock = rock;
        _rockText.text = rock.RockType.ToString();
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_rock == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_rock.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_mineButton, CanMineRock() && !MineRockTaskExists());
        //_mineButton.interactable = CanMineRock() && !MineRockTaskExists();
    }

    /// <summary>
    /// When selecting to mine a wall, create mining task and add to task list
    /// </summary>
    public void MineRock()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.Mine, _rock.transform.position, _rock.gameObject));

        //UnitTask tempTask = new UnitTask
        //{
        //    _location = _rock.transform.position,
        //    _taskType = UnitTask.TaskType.Mine,
        //    _targetRock = _rock,
        //    _requiredTool = Unit.UnitTool.MiningTool,
        //    _taskDescription = "Mining a wall"
        //};
        //TaskList.AddTaskToGlobalTaskList(tempTask);
    }

    /// <summary>
    /// When selecting to reinforce a wall, create a reinforce task and add to task list
    /// </summary>
    public void ReinforceRock()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.Reinforce, _rock.transform.position, _rock.gameObject));

        //UnitTask tempTask = new UnitTask
        //{
        //    _location = _rock.transform.position,
        //    _taskType = UnitTask.TaskType.Reinforce,
        //    _targetRock = _rock,
        //    _requiredTool = Unit.UnitTool.Hammer,
        //    _taskDescription = "Reinforcing a wall"
        //};
        //TaskList.AddTaskToGlobalTaskList(tempTask);
    }

    /// <summary>
    /// Check if the rock can be mined based on mining level
    /// </summary>
    /// <returns>Bool if rock can be mined</returns>
    bool CanMineRock()
    {
        return (_rock.RockType == RockScript.Type.Dirt || (_rock.RockType == RockScript.Type.LooseRock && (int)WorldController.GetWorldController._miningLevel >= 1) || (_rock.RockType == RockScript.Type.HardRock && (int)WorldController.GetWorldController._miningLevel == 2));
    }

    /// <summary>
    /// Check if an exisitng mine rock task exists for this rock
    /// </summary>
    /// <returns>Bool if task exists</returns>
    bool MineRockTaskExists()
    {
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                UnitTask workerTask = worker.GetCurrentTask();
                if (workerTask != null && (workerTask._taskType == UnitTask.TaskType.Mine && workerTask._targetRock == _rock))
                {
                    return true;
                }
            }
        }

        return (TaskList.Tasks.Find(task => task._targetRock == _rock && task._taskType == UnitTask.TaskType.Mine) != null);
    }
}