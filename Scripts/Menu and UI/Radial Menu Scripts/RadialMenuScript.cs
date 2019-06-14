using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuScript : MonoBehaviour
{
    public static RadialMenuScript instance;

    [SerializeField]
    GameObject _radialContentView, _toolMenu, _rockMenu, _enemyMenu, _workerMenu, _buildingsMenu, _hqMenu, _resourceMenu, _rubbleMenu, _vehicleMenu, _emptyVehicleMenu, _outpostMenu, _garageMenu, _mushroomMenu;

    public bool MenuOpen { get; set; }
    public Unit.UnitTool NewTool { get; set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        MenuOpen = false;
    }

    /// <summary>
    /// Close all radial menus
    /// </summary>
    public void CloseMenu()
    {
        MenuOpen = false;
        for (int i = 0; i < _radialContentView.transform.childCount; i++)
        {
            _radialContentView.transform.GetChild(i).gameObject.SetActive(false);
        }       
    }

    public void ShowToolOptions(List<Worker> workers, Worker hitWorker, Vector3 hqPos)
    {
        CloseMenu();
        if (workers.Count == 0 || hitWorker == null)
        {
            return;
        }
        MenuOpen = true;
        _toolMenu.GetComponent<ToolOptionsMenu>().Open(workers, hitWorker, hqPos);
    }

    public void ShowEmptyVehicleOptions(Worker worker, Vehicle vehicle)
    {
        CloseMenu();
        if (vehicle == null)
        {
            return;
        }
        MenuOpen = true;
        _emptyVehicleMenu.GetComponent<EmptyVehicleOptionsMenu>().Open(worker, vehicle);
    }

    public void ShowVehicleOptions(Vehicle vehicle)
    {
        CloseMenu();
        if (vehicle == null)
        {
            return;
        }
        MenuOpen = true;
        _vehicleMenu.GetComponent<VehicleOptionsMenu>().Open(vehicle);
    }

    public void ShowRockOptions(RockScript rock)
    {
        CloseMenu();
        if (rock == null)
        {
            return;
        }
        if (rock.RockType != RockScript.Type.SolidRock)
        {
            MenuOpen = true;
            _rockMenu.GetComponent<RockOptionsMenu>().Open(rock);
        }
    }

    public void ShowRubbleOptions(RubbleScript rubble)
    {
        CloseMenu();
        if (rubble == null)
        {
            return;
        }
        MenuOpen = true;
        _rubbleMenu.GetComponent<RubbleOptionsMenu>().Open(rubble);
    }

    public void ShowEnemyOptions(Monster monster)
    {
        CloseMenu();
        if (monster == null)
        {
            return;
        }
        MenuOpen = true;
        _enemyMenu.GetComponent<EnemyOptionsMenu>().Open(monster);
    }

    public void ShowWorkerOptions(List<Worker> workers, Worker hitWorker)
    {
        CloseMenu();
        if (workers.Count == 0 || hitWorker == null)
        {
            return;
        }
        MenuOpen = true;
        _workerMenu.GetComponent<WorkerOptionsMenu>().Open(workers, hitWorker);
    }

    public void ShowBuildingOptions(Building building)
    {
        CloseMenu();
        if (building == null)
        {
            return;
        }
        MenuOpen = true;
        _buildingsMenu.GetComponent<BuildingOptionsMenu>().Open(building);
    }

    public void ShowHQOptions(HQ hq)
    {
        CloseMenu();
        if (hq == null)
        {
            return;
        }
        MenuOpen = true;
        _hqMenu.GetComponent<HQOptionsMenu>().Open(hq);
    }

    public void ShowResourceOptionsOre(Ore ore)
    {
        CloseMenu();
        if (ore == null)
        {
            return;
        }
        MenuOpen = true;
        _resourceMenu.GetComponent<ResourceOptionsMenu>().OpenOreMenu(ore);
    }

    public void ShowResourceOptionsEnergyCrystal(EnergyCrystal energyCrystal)
    {
        CloseMenu();
        if (energyCrystal == null)
        {
            return;
        }
        MenuOpen = true;
        _resourceMenu.GetComponent<ResourceOptionsMenu>().OpenEnergyCrystalMenu(energyCrystal);
    }

    public void ShowOutpostOptions(Outpost outpost)
    {
        CloseMenu();
        if (outpost == null)
        {
            return;
        }
        MenuOpen = true;
        _outpostMenu.GetComponent<OutpostOptionsMenu>().Open(outpost);
    }

    public void ShowGarageOptions(Garage garage)
    {
        CloseMenu();
        if (garage == null)
        {
            return;
        }
        MenuOpen = true;
        _garageMenu.GetComponent<GarageOptionsMenu>().Open(garage);
    }

    public void ShowMushroomOptions(MushroomCluster mushroomCluster)
    {
        CloseMenu();
        if (mushroomCluster == null)
        {
            return;
        }
        MenuOpen = true;
        _mushroomMenu.GetComponent<MushroomOptionsMenu>().Open(mushroomCluster);
    }

    public void SetButtonInteractable(Button button, bool state)
    {
        button.interactable = state;
        if (state == true)
        {
            button.GetComponentInChildren<Text>().color = Color.white;
        }
        else
        {
            button.GetComponentInChildren<Text>().color = Color.gray;
        }
    }
}
