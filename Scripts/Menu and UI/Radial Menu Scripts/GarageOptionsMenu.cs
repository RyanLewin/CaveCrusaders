using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageOptionsMenu : MonoBehaviour
{
    Garage _garage;

    [SerializeField]
    Button _spawnSmallMiningVehicleButton, _spawnSmallTransportVehicleButton;
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
    /// Open garage radial dial, store garage
    /// </summary>
    /// <param name="garage">Garage selected</param>
    public void Open(Garage garage)
    {
        _garage = garage;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_garage == null || _garage.spawning || !_garage._Built)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_garage.transform.position);
        _destroyCostText.text = ("+ " + (_garage._Built ? (Mathf.RoundToInt(_garage._BuildCost * 0.8f)).ToString() : _garage._BuildCost.ToString()));

        RadialMenuScript.instance.SetButtonInteractable(_spawnSmallMiningVehicleButton, (WorldController.GetWorldController._oreCount >= _garage.GetVehicleCost(0)));
        RadialMenuScript.instance.SetButtonInteractable(_spawnSmallTransportVehicleButton, (WorldController.GetWorldController._oreCount >= _garage.GetVehicleCost(1)));
        //_spawnSmallMiningVehicleButton.interactable = (WorldController.GetWorldController._oreCount >= _garage.GetVehicleCost(0));
        //_spawnSmallTransportVehicleButton.interactable = (WorldController.GetWorldController._oreCount >= _garage.GetVehicleCost(1));
    }

    /// <summary>
    /// When selecting to power, power garage
    /// </summary>
    public void PowerBuilding()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RefillEnergy, _garage.transform.position, _garage.gameObject));
    }

    /// <summary>
    /// When selecting to destroy, destroy garage
    /// </summary>
    public void DestroyBuilding()
    {
        _garage.DestroyBuilding();
    }

    /// <summary>
    /// When selecting to upgrade, upgrade garage
    /// </summary>
    public void UpgradeBuilding()
    {
        _garage.UpgradeBuilding();
    }

    /// <summary>
    /// When selecting to spawn a vehicle, create a new vehicle of the specified type
    /// </summary>
    public void SpawnVehicle(int vehicleId)
    {
        _garage.BuildVehicle(vehicleId);
    }
}
