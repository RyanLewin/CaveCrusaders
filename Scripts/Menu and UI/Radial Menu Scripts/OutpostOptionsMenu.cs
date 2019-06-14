using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutpostOptionsMenu : MonoBehaviour
{
    Outpost _outpost;

    [SerializeField]
    Button _spawnWorkerButton;
    [SerializeField]
    Text _destroyCostText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open outpost radial dial, store outpost
    /// </summary>
    /// <param name="outpost">Outpost selected</param>
    public void Open(Outpost outpost)
    {
        _outpost = outpost;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_outpost == null || !_outpost._Built)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_outpost.transform.position);
        _destroyCostText.text = ("+ " + (_outpost._Built ? (Mathf.RoundToInt(_outpost._BuildCost * 0.8f)).ToString() : _outpost._BuildCost.ToString()));

        RadialMenuScript.instance.SetButtonInteractable(_spawnWorkerButton, (WorldController.GetWorldController._oreCount >= WorldController.GetWorldController._HQ.workerCost && WorldController.GetWorldController._workers.Count < WorldController.GetWorldController._workerLimit));
        //_spawnWorkerButton.interactable = (WorldController.GetWorldController._oreCount >= WorldController.GetWorldController._HQ.workerCost);
    }

    /// <summary>
    /// When selecting to power, power outpost
    /// </summary>
    public void PowerBuilding()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RefillEnergy, _outpost.transform.position, _outpost.gameObject));
    }

    /// <summary>
    /// When selecting to destroy, destroy outpost
    /// </summary>
    public void DestroyBuilding()
    {
        _outpost.DestroyBuilding();
    }


    /// <summary>
    /// When selecting to spawn a unit, create a new unit
    /// </summary>
    public void SpawnWorker()
    {
        _outpost.AddUnit();
    }
}
