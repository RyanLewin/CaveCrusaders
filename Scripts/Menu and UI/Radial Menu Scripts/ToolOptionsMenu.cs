using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolOptionsMenu : MonoBehaviour
{
    List<Worker> _workers;
    Worker _hitWorker;
    Vector3 _hqPos;

    [SerializeField]
    Button _weaponButton, _miningToolButton, _hammerButton, _shovelButton, _flamethrowerButton;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open tool radial dial, store worker and hqpos
    /// </summary>
    /// <param name="workers">All selected workers</param>
    /// <param name="hitWorker">Worker hit in mouse selection</param>
    /// <param name="hqPos">Nearest or selected HQ</param>
    public void Open(List<Worker> workers, Worker hitWorker, Vector3 hqPos)
    {
        _workers = workers;
        _hitWorker = hitWorker;
        _hqPos = hqPos;
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
            if (_workers[i] == null || _workers[i].Dead)
            {
                RadialMenuScript.instance.CloseMenu();
                return;
            }
        }

        transform.position = Camera.main.WorldToScreenPoint(_hitWorker.transform.position);

        if (_workers.Count > 1)
        {
            RadialMenuScript.instance.SetButtonInteractable(_weaponButton, ((int)WorldController.GetWorldController._miningLevel >= 1));
            RadialMenuScript.instance.SetButtonInteractable(_flamethrowerButton, ((int)WorldController.GetWorldController._miningLevel >= 1));
            RadialMenuScript.instance.SetButtonInteractable(_miningToolButton, true);
            RadialMenuScript.instance.SetButtonInteractable(_hammerButton, true);
            RadialMenuScript.instance.SetButtonInteractable(_shovelButton, true);
            //_weaponButton.interactable = (int)WorldController.GetWorldController._miningLevel >= 1;
            //_flamethrowerButton.interactable = (int)WorldController.GetWorldController._miningLevel >= 1;
            //_miningToolButton.interactable = true;
            //_hammerButton.interactable = true;
            //_shovelButton.interactable = true;
        }
        else
        {
            RadialMenuScript.instance.SetButtonInteractable(_weaponButton, ((int)WorldController.GetWorldController._miningLevel >= 1) && (_hitWorker._currentTool != Unit.UnitTool.Weapon));
            RadialMenuScript.instance.SetButtonInteractable(_flamethrowerButton, ((int)WorldController.GetWorldController._miningLevel >= 1) && (_hitWorker._currentTool != Unit.UnitTool.FlameThrower));
            RadialMenuScript.instance.SetButtonInteractable(_miningToolButton, (_hitWorker._currentTool != Unit.UnitTool.MiningTool));
            RadialMenuScript.instance.SetButtonInteractable(_hammerButton, (_hitWorker._currentTool != Unit.UnitTool.Hammer));
            RadialMenuScript.instance.SetButtonInteractable(_shovelButton, (_hitWorker._currentTool != Unit.UnitTool.Shovel));
            //_weaponButton.interactable = ((int)WorldController.GetWorldController._miningLevel >= 1) && (_hitWorker._currentTool != Unit.UnitTool.Weapon);
            //_flamethrowerButton.interactable = ((int)WorldController.GetWorldController._miningLevel >= 1) && (_hitWorker._currentTool != Unit.UnitTool.FlameThrower);
            //_miningToolButton.interactable = (_hitWorker._currentTool != Unit.UnitTool.MiningTool);
            //_hammerButton.interactable = (_hitWorker._currentTool != Unit.UnitTool.Hammer);
            //_shovelButton.interactable = (_hitWorker._currentTool != Unit.UnitTool.Shovel);
        }
    }

    /// <summary>
    /// When a new tool is selected, if it is not the workers current tool, create a get tool task
    /// </summary>
    /// <param name="newToolID">Id of the new tool type</param>
    public void SelectTool(int newToolID)
    {
        Unit.UnitTool newTool = (Unit.UnitTool)newToolID;

        for (int i = 0; i < _workers.Count; i++)
        {
            if (newTool != _workers[i]._currentTool)
            {
                RadialMenuScript.instance.NewTool = newTool;
                _workers[i].SetTask(TaskLibrary.Get().CreateTask(UnitTask.TaskType.GetTool, _hqPos, _workers[i].gameObject),(i < 4));

                //UnitTask tempTask = new UnitTask
                //{
                //    _location = _hqPos,
                //    _taskType = UnitTask.TaskType.GetTool,
                //    _taskDescription = "Getting tool",
                //    _requiredTool = newTool
                //};
                //_worker.SetTask(tempTask);
            }
        }
    }
}
