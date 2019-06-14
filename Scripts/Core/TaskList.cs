#define VALIDATE_TASK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TaskList
{
    public static List<UnitTask> Tasks = new List<UnitTask>();
    public static void AddTaskToGlobalTaskList(UnitTask task)
    {
        if (task == null)
        {
            return;
        }
        if (DoesTaskExist(task))
        {
            return;
        }
        task.UpdateTask();
        Tasks.Add(task);
    }

    public static void InsertTaskToBeginning(UnitTask task)
    {
        if (task == null)
        {
            return;
        }
        if (DoesTaskExist(task))
        {
            return;
        }
        task.UpdateTask();
        Tasks.Insert(0, task);
    }

    public static void RemoveTask(UnitTask task)
    {
        if (task == null)
        {
            return;
        }
        task.DestroyTask();
        Tasks.Remove(task);
    }
    public static bool DoesTaskExist(UnitTask task)
    {
        if (task == null || !task.IsValid())
        {
            return false;
        }
        for (int i = 0; i < TaskList.Tasks.Count; i++)
        {
            if (TaskMenuScript.AreTasksSame(TaskList.Tasks[i], task))
            {
                LogTask(task);
                return true;
            }
        }
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker w = unit.GetComponent<Worker>();
            if (w != null)
            {
                if (w.GetCurrentTask() != null)
                {
                    if (TaskMenuScript.AreTasksSame(w.GetCurrentTask(), task))
                    {
                        LogTask(task);
                        return true;
                    }
                }
                foreach (UnitTask t in w._localTaskList)
                {
                    if (TaskMenuScript.AreTasksSame(t, task))
                    {
                        LogTask(task);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static bool DoesTaskExist(UnitTask task, out UnitTask outtask)
    {
        outtask = null;
        if (task == null || !task.IsValid())
        {
            return false;
        }
        for (int i = 0; i < TaskList.Tasks.Count; i++)
        {
            if (TaskMenuScript.AreTasksSame(TaskList.Tasks[i], task))
            {
                outtask = TaskList.Tasks[i];
                return true;
            }
        }
        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            
            Worker w = unit.GetComponent<Worker>();
            if (w != null)
            {
                if (w.GetCurrentTask() != null)
                {
                    if (TaskMenuScript.AreTasksSame(w.GetCurrentTask(), task))
                    {
                        outtask = w.GetCurrentTask();
                        return true;
                    }
                }
                foreach (UnitTask t in w._localTaskList)
                {
                    if (TaskMenuScript.AreTasksSame(t, task))
                    {
                        outtask = t;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static void LogTask(UnitTask t)
    {
#if VALIDATE_TASK
        Debug.Log("Task of type" + t._taskType.ToString() + " Is dupe");
#endif
    }
}
