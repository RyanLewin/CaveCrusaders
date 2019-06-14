using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleOptionsMenu : MonoBehaviour
{
    Vehicle _vehicle;

    [SerializeField]
    Button _rechargeButton, _disembarkButton;
    [SerializeField]
    Text _vehicleText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open vehicle radial dial, store vehicle
    /// </summary>
    /// <param name="vehicle">Vehicle selected</param>
    public void Open(Vehicle vehicle)
    {
        _vehicle = vehicle;
        _vehicleText.text = vehicle.GetVehicleType().ToString();
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_vehicle == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_vehicle.transform.position);
    }

    /// <summary>
    /// When selecting to recharge a vehicle, create and set recharge vehicle task
    /// </summary>
    public void RechargeVehicle()
    {
        _vehicle.SetTask(TaskLibrary.Get().CreateTask(UnitTask.TaskType.RechargeVehicle, _vehicle.transform.position, _vehicle.gameObject));
    }
    
    /// <summary>
    /// When selecting to recharge a vehicle, create and set recharge vehicle task
    /// </summary>
    public void Disembark()
    {
        _vehicle.Disembark();
    }
}
