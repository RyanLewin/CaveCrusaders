using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HQOptionsMenu : MonoBehaviour
{
    HQ _hq;

    [SerializeField]
    Button _upgradeButton, _spawnWorkerButton;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open hq radial dial, store hq
    /// </summary>
    /// <param name="hq">HQ selected</param>
    public void Open(HQ hq)
    {
        _hq = hq;
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_hq == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_hq.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_upgradeButton, (WorldController.GetWorldController._energyCrystalsCount >= 1 && _hq.CanUpgrade()));
        RadialMenuScript.instance.SetButtonInteractable(_spawnWorkerButton, (WorldController.GetWorldController._oreCount >= WorldController.GetWorldController._HQ.workerCost && WorldController.GetWorldController._workers.Count < WorldController.GetWorldController._workerLimit));
        //_upgradeButton.interactable = (WorldController.GetWorldController._energyCrystalsCount >= 1 && _hq.CanUpgrade());
        //_spawnWorkerButton.interactable = (WorldController.GetWorldController._oreCount >= WorldController.GetWorldController._HQ.workerCost);
    }

    /// <summary>
    /// When selecting to upgrade, upgrade HQ
    /// </summary>
    public void UpgradeBuilding()
    {
        _hq.UpgradeBuilding();
    }

    /// <summary>
    /// When selecting to spawn a unit, create a new unit
    /// </summary>
    public void SpawnWorker()
    {
        _hq.AddUnit();
    }
}
