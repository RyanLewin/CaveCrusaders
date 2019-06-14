using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingOptionsMenu : MonoBehaviour
{
    Building _building;

    [SerializeField]
    Button _upgradeButton, _powerButton;
    [SerializeField]
    Text _buildingText, _destroyCostText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open building radial dial, store building
    /// </summary>
    /// <param name="building">Building selected</param>
    public void Open(Building building)
    {
        _building = building;
        _buildingText.text = building.buildingType.ToString();
        DoUpdateChecks();
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_building == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_building.transform.position);
        _destroyCostText.text = ("+ " + (_building._Built ? (Mathf.RoundToInt(_building._BuildCost * 0.8f)).ToString() : _building._BuildCost.ToString()));

        RadialMenuScript.instance.SetButtonInteractable(_upgradeButton, (WorldController.GetWorldController._energyCrystalsCount >= 1 && _building.CanUpgrade()));
        RadialMenuScript.instance.SetButtonInteractable(_powerButton, (WorldController.GetWorldController._energyCrystalsCount >= 1 && _building._Built));
        //_upgradeButton.interactable = (WorldController.GetWorldController._energyCrystalsCount >= 1 && _building.CanUpgrade());
        //_powerButton.interactable = (WorldController.GetWorldController._energyCrystalsCount >= 1 && _building._Built);
    }

    /// <summary>
    /// When selecting to upgrade, upgrade building
    /// </summary>
    public void UpgradeBuilding()
    {
        _building.UpgradeBuilding();
    }

    /// <summary>
    /// When selecting to destroy, destroy building
    /// </summary>
    public void DestroyBuilding()
    {
        _building.DestroyBuilding(!_building._Built);
    }

    /// <summary>
    /// When selecting to power, power building
    /// </summary>
    public void PowerBuilding()
    {
        TaskList.InsertTaskToBeginning(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RefillEnergy, _building.transform.position, _building.gameObject));

        //UnitTask tempTask = new UnitTask
        //{
        //    _location = _building.transform.position,
        //    _taskType = UnitTask.TaskType.RefillEnergy,
        //    _targetBuilding = _building,
        //    _taskDescription = "Refill building energy"
        //};
        //AddTaskToGlobalTaskList(tempTask);
    }
}
